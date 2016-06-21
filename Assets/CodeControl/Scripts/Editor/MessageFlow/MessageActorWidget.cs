/// <copyright file="MessageActorWidget.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using System.Collections.Generic;
using CodeControl.Internal;

namespace CodeControl.Editor {

    public class MessageActorWidget : ButtonWidget {

        public Type ActorType { get; private set; }
        public string ActorTypeName { get; private set; }

        public Vector2 GridPosition;

        private class LoggedMessage {
            public string MessageName;
            public List<MessageActorWidget> AssociatedActors;
            public LoggedMessage(string messageName) {
                MessageName = messageName;
                AssociatedActors = new List<MessageActorWidget>();
            }
        }

        private const float minSpeed = 1.1f;

        private readonly MessageFlowRenderer messageFlowRenderer;

        private Dictionary<Type, List<LoggedMessage>> sentMessageLogs;
        private Dictionary<Type, List<LoggedMessage>> handledMessageLogs;

        public MessageActorWidget(Type actorType, MessageFlowRenderer messageRelationsRenderer) : base() {
            this.messageFlowRenderer = messageRelationsRenderer;
            ActorType = actorType;
            ActorTypeName = CodeControlEditorHelper.GetActualTypeName(ActorType);

            sentMessageLogs = new Dictionary<Type, List<LoggedMessage>>();
            handledMessageLogs = new Dictionary<Type, List<LoggedMessage>>();
        }

        public void LogSentMessage(MessageActorWidget handler, Type messageType, string messageName) {
            LogMessage(sentMessageLogs, handler, messageType, messageName);
        }

        public void LogHandledMessage(MessageActorWidget trigger, Type messageType, string messageName) {
            LogMessage(handledMessageLogs, trigger, messageType, messageName);
        }

        protected override string GetText() {
            return ActorTypeName;
        }

        protected override void ShowContextMenu() {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Open " + ActorTypeName + ".cs"), false, delegate() {
                CodeControlEditorHelper.OpenCodeOfType(ActorType);
            });

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Hide"), false, delegate() {
                messageFlowRenderer.RemoveActor(this);
            });

            if (!messageFlowRenderer.ContainsIndirectActors(this)) {
                menu.AddDisabledItem(new GUIContent("Hide Indirect"));
            } else {
                menu.AddItem(new GUIContent("Hide Indirect"), false, delegate() {
                    messageFlowRenderer.RemoveIndirectActors(this);
                });
            }
            
            if (!messageFlowRenderer.ContainsUnrelatedActors(this)) {
                menu.AddDisabledItem(new GUIContent("Hide Unrelated"));
            } else {
                menu.AddItem(new GUIContent("Hide Unrelated"), false, delegate() {
                    messageFlowRenderer.RemoveUnrelatedActors(this);
                });
            }

            menu.AddSeparator("");

            AddContextMenuLoggedMessages(menu, "Sent Messages", "To ", sentMessageLogs);
            AddContextMenuLoggedMessages(menu, "Received Messages", "From ", handledMessageLogs);

            menu.ShowAsContext();
        }

        private void AddContextMenuActors(GenericMenu menu, string listName, List<MessageActorWidget> actors) {
            if (actors.Count == 0) {
                menu.AddDisabledItem(new GUIContent(listName));
                return;
            }
            foreach (MessageActorWidget actor in actors) {
                menu.AddItem(new GUIContent(listName + "/" + actor.ActorType.ToString() + ".cs"), false, delegate() {
                    CodeControlEditorHelper.OpenCodeOfType(actor.ActorType);
                });
            }
        }

        private void AddContextMenuLoggedMessages(GenericMenu menu, string listName, string messageActorPrefix, Dictionary<Type, List<LoggedMessage>> logs) {
            if (logs.Count == 0) {
                menu.AddDisabledItem(new GUIContent(listName));
                return;
            }

            const string typelessMessagesName = "Typeless Messages";

            foreach (KeyValuePair<Type, List<LoggedMessage>> pair in logs) {
                bool isTypeless = pair.Key == typeof(Message);
                string typeName = isTypeless ? typelessMessagesName : pair.Key.ToString();

                // Check if there is no nameless message of a typed message
                if (!isTypeless) {
                    menu.AddItem(new GUIContent(listName + "/" + typeName + "/Open " + typeName + ".cs"), false, delegate() {
                        CodeControlEditorHelper.OpenCodeOfType(pair.Key);
                    });
                    menu.AddSeparator(listName + "/" + typeName + "/");
                }

                // Make sure the nameless message shows first
                List<LoggedMessage> sortedLogMessages = new List<LoggedMessage>();
                LoggedMessage namelessMessage = pair.Value.Find(x => x.MessageName == "");
                if (namelessMessage != null) { sortedLogMessages.Add(namelessMessage); }
                sortedLogMessages.AddList<LoggedMessage>(pair.Value.FindAll(x => x.MessageName != ""));

                int messageIndex = 0;
                foreach (LoggedMessage logMessage in sortedLogMessages) {
                    // If this message is nameless, show title as nameless
                    if (logMessage.MessageName == "") {
                        menu.AddDisabledItem(new GUIContent(listName + "/" + typeName + "/Nameless Message"));
                    } else {
                        menu.AddDisabledItem(new GUIContent(listName + "/" + typeName + "/" + '"' + logMessage.MessageName + '"'));
                    }

                    foreach (MessageActorWidget actor in logMessage.AssociatedActors) {
                        string itemName = messageActorPrefix + actor.ActorTypeName + ".cs";

                        // Add blank spaces as postfix to force the ContextMenu to show these items more than once
                        for (int i = 0; i < messageIndex; i++) {
                            itemName += ' ';
                        }

                        menu.AddItem(new GUIContent(listName + "/" + typeName + "/" + itemName), false, delegate() {
                            CodeControlEditorHelper.OpenCodeOfType(actor.ActorType);
                        });
                    }

                    if (pair.Value.IndexOf(logMessage) < pair.Value.Count - 1) {
                        menu.AddSeparator(listName + "/" + typeName + "/");
                    }

                    messageIndex++;
                }
            }

        }

        private void AddContextMenuTypeMessages(Type type, List<LoggedMessage> loggedMessages) {

        }

        private void LogMessage(Dictionary<Type, List<LoggedMessage>> logs, MessageActorWidget associatedActor, Type messageType, string messageName) {
            if (!logs.ContainsKey(messageType)) {
                logs.Add(messageType, new List<LoggedMessage>());
            }
            LoggedMessage loggedMessage = logs[messageType].Find(x => x.MessageName == messageName);
            if (loggedMessage == null) {
                loggedMessage = new LoggedMessage(messageName);
                logs[messageType].Add(loggedMessage);
            }
            if (!loggedMessage.AssociatedActors.Contains(associatedActor)) {
                loggedMessage.AssociatedActors.Add(associatedActor);
            }
        }
        
    }

}