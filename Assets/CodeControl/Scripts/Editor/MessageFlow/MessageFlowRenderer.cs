/// <copyright file="MessageFlowRenderer.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using CodeControl;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace CodeControl.Editor {

    public class MessageFlowRenderer : IWindowRenderer {

        public static Vector2 ActorOffset { get { return new Vector2(175, 75); } }

        public Rect BoundingBox { get { return boundingBox; } }
        public string Title { get { return "Message Flow"; } }
        public bool ShowMessages { get { return EditorPrefs.HasKey(showMessagesPrefKey) ? EditorPrefs.GetBool(showMessagesPrefKey) : true; } }

        private const string showMessagesPrefKey = "UCC Message Flow Show Messages";
        private const float chainMarginVert = 60;

        private Rect boundingBox;
        private List<MessageActorWidget> messageActors;
        private List<MessageLine> messageLines;
        private List<MessageChain> messageChains;

        private bool chainesAreDirty = false;

        public MessageFlowRenderer() {
            messageActors = new List<MessageActorWidget>();
            messageLines = new List<MessageLine>();
            messageChains = new List<MessageChain>();
            Message.OnMessageHandle += OnMessageHandleHandler;
            boundingBox = new Rect();
        }

        public void Deinit() {
            Message.OnMessageHandle -= OnMessageHandleHandler;
        }

        public void ShowContextMenu() {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("What is this?"), false, delegate() {
                Application.OpenURL(CodeControlMonitorWindow.MonitorHelpUrl);
            });

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Show Messages"), ShowMessages, delegate() {
                EditorPrefs.SetBool(showMessagesPrefKey, !ShowMessages);                
            });

            menu.ShowAsContext();
        }

        public void RemoveActor(MessageActorWidget actor) {
            FilterLinesAndActors(x => x.CallerActor != actor && x.HandlerActor != actor);
        }

        public void RemoveIndirectActors(MessageActorWidget actor) {
            MessageChain chain = GetChainOfActor(actor);
            FilterLinesAndActors(x => x.CallerActor == actor || x.HandlerActor == actor || !chain.Lines.Contains(x));
        }

        public void RemoveUnrelatedActors(MessageActorWidget actor) {
            MessageChain chain = GetChainOfActor(actor);
            if (chain == null) { return; }
            FilterLinesAndActors(x => chain.Lines.Contains(x));
        }

        public bool ContainsIndirectActors(MessageActorWidget actor) {
            MessageChain chain = GetChainOfActor(actor);
            return messageLines.FindAll(x => x.HandlerActor != actor && x.CallerActor != actor && chain == GetChainOfActor(x.CallerActor)).Count > 0;
        }

        public bool ContainsUnrelatedActors(MessageActorWidget actor) {
            return messageChains.Count > 1;
        }

        public void Update() {
            foreach (MessageLine messageLine in messageLines) {
                messageLine.Update();
            }
            foreach (MessageActorWidget actor in messageActors) {
                actor.Update();
            }

            MessageLine.UpdateHeavy();
        }

        public void Render() {
            if (chainesAreDirty) {
                chainesAreDirty = false;
                ReconstructMessageChains();
            }

            if (messageActors.Count > 0) {
                foreach (MessageLine line in messageLines) {
                    line.RenderLine();
                }

                foreach (MessageActorWidget actor in messageActors) {
                    actor.Render();
                }

                
                foreach (MessageLine line in messageLines) {
                    line.RenderMessages(ShowMessages);
                }
            } else {
                CodeControlEditorStyles.SetLabelStyle(CodeControlEditorStyles.LabelStyle.WarningMessage);
                GUI.color = CodeControlEditorStyles.NoContentColor;
                GUI.Label(new Rect(18, 15, 300, 100), "No Messages Sent.");
                CodeControlEditorStyles.ResetLabelStyle();
            }
        }

        public void RenderMiniMap() {
            foreach (MessageLine line in messageLines) {
                line.RenderMiniMap();
            }
            foreach (MessageActorWidget actor in messageActors) {
                actor.RenderMiniMap();
            }
        }

        private void OnMessageHandleHandler(Type callerType, Type handlerType, Type messageType, string messageName, string handlerMethodName) {
            MessageActorWidget callerActor = GetOrCreateActor(callerType);
            MessageActorWidget handlerActor = GetOrCreateActor(handlerType);

            callerActor.LogSentMessage(handlerActor, messageType, messageName);
            handlerActor.LogHandledMessage(callerActor, messageType, messageName);

            MessageLine line = messageLines.Find(x => (x.CallerActor == callerActor && x.HandlerActor == handlerActor));
            bool newLineCreated = false;
            bool reversed = false;
            if (line == null) {
                // Check for reversed to change it to TwoWay line
                line = messageLines.Find(x => (x.HandlerActor == callerActor && x.CallerActor == handlerActor));
                if (line != null) {
                    reversed = true;
                } else {
                    line = new MessageLine(callerActor, handlerActor);
                    messageLines.Add(line);
                    newLineCreated = true;
                }
            }
            
            line.SendMessage(messageType, messageName == "" ? messageType.ToString() : '"' + messageName + '"', reversed);

            if (newLineCreated) {
                chainesAreDirty = true;
            }
        }

        private MessageChain GetChainOfActor(MessageActorWidget actor) {
            return messageChains.Find(x => x.Lines.Find(y => y.CallerActor == actor || y.HandlerActor == actor) != null);
        }

        private MessageActorWidget GetOrCreateActor(Type type) {
            MessageActorWidget actor = messageActors.Find(x => x.ActorType == type);
            if (actor != null) {
                return actor;
            }
            actor = new MessageActorWidget(type, this);
            messageActors.Add(actor);
            return actor;
        }

        private void FilterLinesAndActors(Predicate<MessageLine> where) {
            List<MessageLine> linesToStay = messageLines.FindAll(where);
            messageActors.Clear();
            messageLines.Clear();

            foreach (MessageLine line in linesToStay) {
                if (!messageActors.Contains(line.CallerActor)) { messageActors.Add(line.CallerActor); }
                if (!messageActors.Contains(line.HandlerActor)) { messageActors.Add(line.HandlerActor); }
                messageLines.Add(line);
            }

            ReconstructMessageChains();
        }

        private void ReconstructMessageChains(){
            messageChains.Clear();
            List<MessageLine> looseLines = new List<MessageLine>(messageLines);
            
            while (looseLines.Count > 0) {
                MessageChain chain = new MessageChain();

                chain.Lines.Add(looseLines[0]);
                looseLines.RemoveAt(0);

                bool addedNewLine = true;
                while (addedNewLine) {
                    addedNewLine = false;
                    if (looseLines.Count > 0) {
                        for (int i = looseLines.Count - 1; i >= 0; i--) {
                            MessageLine looseLine = looseLines[i];
                            foreach (MessageLine lineInChain in chain.Lines) {
                                if (lineInChain.IsConnectedTo(looseLine)) {
                                    chain.Lines.Add(looseLine);
                                    looseLines.RemoveAt(i);
                                    addedNewLine = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                chain.RepositionInternally();
                messageChains.Add(chain);
            }

            float totalHeight = 0.0f;
            float maxWidth = 0.0f;
            foreach (MessageChain chain in messageChains) {
                chain.Position = new Vector2(0.0f, totalHeight);
                totalHeight += chain.Height;
                maxWidth = Mathf.Max(maxWidth, chain.Width);
                totalHeight += chainMarginVert;
            }

            boundingBox = new Rect(0, 0, maxWidth, totalHeight);
        }

    }

}