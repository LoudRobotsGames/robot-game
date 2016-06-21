/// <copyright file="MessageChain.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CodeControl.Editor {

    public class MessageChain {

        private class ContainedMessageActorWidget {
            public Vector2 LocalGridPosition;
            public MessageActorWidget Actor { get; private set; }
            public ContainedMessageActorWidget(MessageActorWidget actor, Vector2 localPosition) {
                LocalGridPosition = localPosition;
                Actor = actor;
            }
        }

        public List<MessageLine> Lines { get; private set; }

        public float Height {
            get {
                float top = 0.0f;
                float bottom = 0.0f;
                foreach (ContainedMessageActorWidget containedActor in containedActors) {
                    top = Mathf.Min(top, containedActor.LocalGridPosition.y);
                    bottom = Mathf.Max(bottom, containedActor.LocalGridPosition.y);
                }
                return (bottom - top) * MessageFlowRenderer.ActorOffset.y + MessageActorWidget.Height;
            }
        }

        public float Width {
            get {
                float left = 0.0f;
                float right = 0.0f;
                bool isSet = false;
                foreach (ContainedMessageActorWidget containedActor in containedActors) {
                    if (!isSet || containedActor.Actor.TargetPosition.x < left) {
                        left = containedActor.Actor.TargetPosition.x;
                    }
                    if (!isSet || containedActor.Actor.TargetPosition.x > right) {
                        right = containedActor.Actor.TargetPosition.x;
                    }
                    isSet = true;
                }
                if (!isSet) {
                    return 0.0f;
                }
                return right - left + MessageActorWidget.Width;
            }
        }

        public Vector2 Position {
            get {
                return position;
            }
            set {
                position = value;
                UpdateActorPositions();
            }
        }

        private Vector2 position;
        private List<ContainedMessageActorWidget> containedActors;

        public MessageChain() {
            position = Vector2.zero;
            Lines = new List<MessageLine>();
            containedActors = new List<ContainedMessageActorWidget>();
        }

        public void RepositionInternally() {
            containedActors.Clear();
            ContainedMessageActorWidget rootActor = new ContainedMessageActorWidget(GetRootMessageActor(), new Vector2(0, 0));
            containedActors.Add(rootActor);
            PopulateCallersAndHandlersOfActor(rootActor);
        }

        private void UpdateActorPositions() {
            Vector2 topLeftGridPosition = Vector2.zero;
            foreach (ContainedMessageActorWidget a in containedActors) {
                topLeftGridPosition.x = Mathf.Min(topLeftGridPosition.x, a.LocalGridPosition.x);
                topLeftGridPosition.y = Mathf.Min(topLeftGridPosition.y, a.LocalGridPosition.y);
            }
            foreach (ContainedMessageActorWidget a in containedActors) {
                Vector2 localGridPosWithOffset = a.LocalGridPosition - topLeftGridPosition;
                a.Actor.GridPosition = a.LocalGridPosition;
                a.Actor.TargetPosition = position + 
                                       new Vector2(localGridPosWithOffset.x * MessageFlowRenderer.ActorOffset.x, 
                                                   localGridPosWithOffset.y * MessageFlowRenderer.ActorOffset.y) + 
                                       .5f * new Vector2(MessageActorWidget.Width, MessageActorWidget.Height);
            }
        }

        private void PopulateCallersAndHandlersOfActor(ContainedMessageActorWidget containedActor) {
            List<ContainedMessageActorWidget> newActors = new List<ContainedMessageActorWidget>();
            for (int i = 0; i < 2; i++) {
                bool checkForCallers = i == 0;
                foreach (MessageLine line in Lines) {
                    if (checkForCallers && line.HandlerActor != containedActor.Actor) { continue; }
                    if (checkForCallers && containedActors.Find(x => x.Actor == line.CallerActor) != null) { continue; }
                    if (!checkForCallers && line.CallerActor != containedActor.Actor) { continue; }
                    if (!checkForCallers && containedActors.Find(x => x.Actor == line.HandlerActor) != null) { continue; }
                    ContainedMessageActorWidget newContainerActor = new ContainedMessageActorWidget(checkForCallers ? line.CallerActor : line.HandlerActor, GetFreeGridPositionAroundActor(containedActor));
                    containedActors.Add(newContainerActor);
                    newActors.Add(newContainerActor);
                }   
            }
            foreach (ContainedMessageActorWidget caller in newActors) {
                PopulateCallersAndHandlersOfActor(caller);
            }
        }

        private Vector2 GetFreeGridPositionAroundActor(ContainedMessageActorWidget containedActor) {
            int radius = 2;
            while (true) {
                List<Vector2> possiblePositions = new List<Vector2>();
                for (int x = 0; x < radius; x++) {
                    for (int y = 0; y < radius; y++) {
                        bool xIsEdge = x == 0 || x == radius-1;
                        bool yIsEdge = y == 0 || y == radius-1;
                        if (xIsEdge || yIsEdge || (xIsEdge && yIsEdge)) {
                            Vector2 position = containedActor.LocalGridPosition + new Vector2(-radius * .5f + x, -radius * .5f + y);
                            if (containedActors.Find(pos => pos.LocalGridPosition == position) != null) { continue; }
                            possiblePositions.Add(position);
                        }
                    }
                }
                Vector2 closestPosition = Vector2.zero;
                float shortestDistance = 0.0f;
                float distanceToCenter = 0.0f;
                foreach (Vector2 possiblePosition in possiblePositions) {
                    float thisDistance = Vector2.Distance(containedActor.LocalGridPosition, possiblePosition);
                    float thisDistanceToCenter = Vector2.Distance(Vector2.zero, possiblePosition);
                    if ((shortestDistance == 0.0f || shortestDistance > thisDistance) ||
                        (shortestDistance == thisDistance && distanceToCenter > thisDistanceToCenter)) {
                        closestPosition = possiblePosition;
                        shortestDistance = thisDistance;
                        distanceToCenter = thisDistanceToCenter;
                    }
                }
                if (shortestDistance != 0.0f) {
                    return closestPosition;
                }
                radius += 2;
            }
        }

        private MessageActorWidget GetRootMessageActor() {
            MessageActorWidget rootActor = null;
            Dictionary<MessageActorWidget, int> handlerCounts = new Dictionary<MessageActorWidget, int>();
            foreach (MessageLine line in Lines) {
                if (handlerCounts.ContainsKey(line.HandlerActor)) {
                    handlerCounts[line.HandlerActor]++;
                } else {
                    handlerCounts.Add(line.HandlerActor, 1);
                }
            }
            foreach (KeyValuePair<MessageActorWidget, int> pair in handlerCounts) {
                if (rootActor == null) {
                    rootActor = pair.Key;
                    continue;
                }
                if (pair.Value > handlerCounts[rootActor]) {
                    rootActor = pair.Key;
                }
            }
            return rootActor;
        }

    }

}