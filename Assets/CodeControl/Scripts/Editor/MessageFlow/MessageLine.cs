/// <copyright file="MessageLine.cs">Copyright (c) 2015 All Rights Reserved</copyright>
/// <author>Joris van Leeuwen</author>
/// <date>01/27/2014</date>

using UnityEngine;
using System.Collections;
using CodeControl;
using System;
using UnityEditor;
using System.Collections.Generic;
using CodeControl.Internal;

namespace CodeControl.Editor {

    public class MessageLine {

        public MessageActorWidget CallerActor { get; private set; }
        public MessageActorWidget HandlerActor { get; private set; }

        private class WayPoint {
            public Vector2 GridPosition;
            public Vector2 Normal;

            public float GridDistance;
            public float CoverageOfTotal;

            public Vector2 GetPosition(MessageActorWidget callerActor, float offset) {
                Vector2 relativeGridPosition = GridPosition - callerActor.GridPosition;
                Vector2 wayPointPosition = callerActor.Position + new Vector2(relativeGridPosition.x * MessageFlowRenderer.ActorOffset.x, relativeGridPosition.y * MessageFlowRenderer.ActorOffset.y);
                wayPointPosition += Normal * offset;
                return wayPointPosition;
            }
        }

        private static List<MessageLine> instances = new List<MessageLine>();
        private static int focusedInstanceIndex = 0;

        private const float TwoWayLineMargin = 10.0f;
        private const float ArrowSize = 5.0f;
        private const float DotMaxAge = 6.0f;

        private bool isTwoWay;
        private float age;
        private float lastSendTime;
        private float lastReverseSendTime;
        private Color lineColor;
        private List<LineMessage> lineMessages;

        private List<WayPoint> wayPoints;
        private Vector2 callerGridPositionOnWayPointsUpdate;
        private Vector2 handlerGridPositionOnWayPointsUpdate;

        private float totalGridDistance;

        public static void UpdateHeavy() {
            for (int i = instances.Count - 1; i >= 0; i--) {
                if (instances[i] == null) {
                    instances.RemoveAt(i);
                }
            }

            if (instances.Count == 0) { return; }
            
            if (focusedInstanceIndex > instances.Count - 1) { focusedInstanceIndex = 0; }
            MessageLine line = instances[focusedInstanceIndex];
            line.UpdateWayPoints();
            focusedInstanceIndex++;
        }

        public MessageLine(MessageActorWidget callerActor, MessageActorWidget handlerActor) {
            CallerActor = callerActor;
            HandlerActor = handlerActor;
            lineMessages = new List<LineMessage>();
            wayPoints = new List<WayPoint>();

            instances.Add(this);
            UpdateWayPoints();
        }

        public void SendMessage(Type messageType, string messageName, bool reversed) {
            if (reversed) {
                isTwoWay = true;
                lastReverseSendTime = age;
            } else {
                lastSendTime = age;
            }
            lineMessages.Add(new LineMessage(messageType, messageName, reversed));
        }

        public void Update() {
            age += CodeControlMonitorWindow.DeltaTime;

            for (int i = lineMessages.Count - 1; i >= 0; i--) {
                lineMessages[i].Update();
                if (lineMessages[i].IsDone) {
                    lineMessages.RemoveAt(i);
                }
            }
        }

        public void RenderLine() {
            lineColor = CodeControlEditorStyles.MessageLineColor;

            int lineWidth = 1;
            if (CallerActor.IsHovered || HandlerActor.IsHovered) {
                lineWidth = 3;
            }

            if (!isTwoWay) {
                RenderLineAcrossWayPoints(0.0f, lineWidth, false);

                RenderArrow(false, 0.0f, lineWidth);

                RenderDots(false, 0.0f);
            } else {
                RenderLineAcrossWayPoints(.5f * TwoWayLineMargin, lineWidth, false);
                RenderLineAcrossWayPoints(-.5f * TwoWayLineMargin, lineWidth, false);

                RenderArrow(false, .5f * TwoWayLineMargin, lineWidth);
                RenderArrow(true, .5f * TwoWayLineMargin, lineWidth);

                RenderDots(false, .5f * TwoWayLineMargin);
                RenderDots(true, -.5f * TwoWayLineMargin);
            }
        }

        public void RenderMiniMap() {
            lineColor = new Color(1.0f, 1.0f, 1.0f, .3f);

            if (!isTwoWay) {
                RenderLineAcrossWayPoints(0.0f, 1, true);
            } else {
                RenderLineAcrossWayPoints(.5f * TwoWayLineMargin, 1, true);
                RenderLineAcrossWayPoints(-.5f * TwoWayLineMargin, 1, true);
            }
        }

        public void RenderMessages(bool renderNames) {
            foreach (LineMessage lineMessage in lineMessages) {
                float positionFactor = lineMessage.AgeFactor;
                float easedPositionFactor = (positionFactor == 1.0f) ? 1.0f : 1.0f * (-Mathf.Pow(2, -10 * positionFactor) + 1);
                if (lineMessage.IsReversed) {
                    easedPositionFactor = 1.0f - easedPositionFactor;
                }

                lineMessage.Render(CodeControlMonitorWindow.WindowOffset + GetPositionOnLine(easedPositionFactor), renderNames);
            }
        }

        public bool IsConnectedTo(MessageLine other) {
            return other.HandlerActor == HandlerActor ||
                   other.HandlerActor == CallerActor ||
                   other.CallerActor == HandlerActor ||
                   other.CallerActor == CallerActor;
        }

        private void UpdateWayPoints() {
            if ((Mathf.Abs(HandlerActor.GridPosition.x - CallerActor.GridPosition.x) <= .5f && Mathf.Abs(HandlerActor.GridPosition.y - CallerActor.GridPosition.y) <= 1.0f) || 
                (Mathf.Abs(HandlerActor.GridPosition.y - CallerActor.GridPosition.y) <= .5f && Mathf.Abs(HandlerActor.GridPosition.x - CallerActor.GridPosition.x) <= 1.0f)) {
                totalGridDistance = 1.0f;
                ClearWayPoints();
            } else if (CallerActor.GridPosition != callerGridPositionOnWayPointsUpdate || HandlerActor.GridPosition != handlerGridPositionOnWayPointsUpdate) {
                callerGridPositionOnWayPointsUpdate = CallerActor.GridPosition;
                handlerGridPositionOnWayPointsUpdate = HandlerActor.GridPosition;

                ClearWayPoints();

                Vector2 direction = HandlerActor.TargetPosition - CallerActor.TargetPosition;

                Vector2 startGrid = new Vector2(CallerActor.GridPosition.x + (direction.x > 0 ? .5f : -.5f),
                                                CallerActor.GridPosition.y + (direction.y > 0 ? .5f : -.5f));
                Vector2 endGrid = new Vector2(HandlerActor.GridPosition.x - (direction.x >= 0 ? .5f : -.5f),
                                              HandlerActor.GridPosition.y - (direction.y >= 0 ? .5f : -.5f));

                Vector2 currentGrid = startGrid;
                wayPoints.Add(new WayPoint() { GridPosition = currentGrid });

                // Add wayPoints until reached destination
                while (currentGrid != endGrid) {
                    if (currentGrid.y > endGrid.y) {
                        currentGrid.y = currentGrid.y - 1.0f;
                    } else if (currentGrid.y < endGrid.y) {
                        currentGrid.y = currentGrid.y + 1.0f;
                    } else if (currentGrid.x > endGrid.x) {
                        currentGrid.x = currentGrid.x - 1.0f;
                    } else if (currentGrid.x < endGrid.x) {
                        currentGrid.x = currentGrid.x + 1.0f;
                    }
                    wayPoints.Add(new WayPoint() { GridPosition = currentGrid });
                }

                // Calculate Normals
                if (wayPoints.Count == 1) {
                    wayPoints[0].Normal = (HandlerActor.GridPosition - wayPoints[0].GridPosition).GetPerpendicular().normalized;
                }else{
                    wayPoints[0].Normal = (wayPoints[1].GridPosition - wayPoints[0].GridPosition).GetPerpendicular().normalized;
                    for (int i = 0; i < wayPoints.Count; i++) {
                        Vector2 from = wayPoints[i].GridPosition - (i == 0 ? CallerActor.GridPosition : wayPoints[i - 1].GridPosition);
                        Vector2 to = (i >= wayPoints.Count - 1 ? HandlerActor.GridPosition : wayPoints[i + 1].GridPosition) - wayPoints[i].GridPosition;
                        wayPoints[i].Normal = (from + to).GetPerpendicular().normalized;
                    }
                }

                // Calculate Coverage of total length
                totalGridDistance = 0.0f;
                Vector2 previousGridPos = .5f * (CallerActor.GridPosition + wayPoints[0].GridPosition);
                foreach (WayPoint wayPoint in wayPoints) {
                    wayPoint.GridDistance = Vector2.Distance(wayPoint.GridPosition, previousGridPos);
                    totalGridDistance += wayPoint.GridDistance;
                    previousGridPos = wayPoint.GridPosition;
                }
                totalGridDistance += Vector2.Distance(.5f * (HandlerActor.GridPosition + wayPoints[wayPoints.Count - 1].GridPosition), wayPoints[wayPoints.Count - 1].GridPosition);
                foreach (WayPoint wayPoint in wayPoints) {
                    wayPoint.CoverageOfTotal = wayPoint.GridDistance / totalGridDistance;
                }
            }
        }

        private void ClearWayPoints() {
            while (wayPoints.Count > 0) {
                wayPoints.RemoveAt(0);
            }
        }

        private Vector2 GetPositionOnLine(float progress, float offset = 0.0f) {
            if (wayPoints.Count == 0) {
                Vector2 globalPosOffset = (HandlerActor.Position - CallerActor.Position).GetPerpendicular().normalized * offset;
                return Vector2.Lerp(CallerActor.Position, HandlerActor.Position, .25f + .5f * progress) + globalPosOffset;
            } else {
                float totalCoverage = 0.0f;
                for (int i = 0; i < wayPoints.Count; i++) {
                    WayPoint wayPoint = wayPoints[i];
                    if (progress <= totalCoverage + wayPoint.CoverageOfTotal) {
                        Vector2 previousPos;
                        if (i == 0) {
                            Vector2 startPosOffset = (wayPoints[0].GetPosition(CallerActor, offset) - CallerActor.Position).GetPerpendicular().normalized * offset;
                            previousPos = .5f * (CallerActor.Position + wayPoints[0].GetPosition(CallerActor, offset)) + startPosOffset;
                        } else {
                            previousPos = wayPoints[i - 1].GetPosition(CallerActor, offset);
                        }
                        return Vector2.Lerp(previousPos, wayPoint.GetPosition(CallerActor, offset), (progress - totalCoverage) / wayPoint.CoverageOfTotal);
                    }
                    totalCoverage += wayPoint.CoverageOfTotal;
                }
                float endCoverageOfTotal = Vector2.Distance(wayPoints[wayPoints.Count-1].GridPosition, .5f * (wayPoints[wayPoints.Count-1].GridPosition + HandlerActor.GridPosition)) / totalGridDistance;
                Vector2 lastWayPointPosition = wayPoints[wayPoints.Count - 1].GetPosition(CallerActor, offset);
                Vector2 endPosOffset = (HandlerActor.Position - lastWayPointPosition).GetPerpendicular().normalized * offset;
                return Vector2.Lerp(lastWayPointPosition, .5f * (HandlerActor.Position + endPosOffset + lastWayPointPosition), (progress - totalCoverage) / endCoverageOfTotal);
            }
        }

        private void RenderLineAcrossWayPoints(float offset, int width, bool inMiniMap) {
            Vector2 previousWayPointPosition = CallerActor.Position;

            if (wayPoints.Count > 0) {
                previousWayPointPosition += (wayPoints[0].GridPosition - CallerActor.GridPosition).GetPerpendicular().normalized * offset;
            } else {
                previousWayPointPosition += (HandlerActor.GridPosition - CallerActor.GridPosition).GetPerpendicular().normalized * offset;
            }

            for (int i = 0; i < wayPoints.Count; i++) {
                WayPoint wayPoint = wayPoints[i];
                Vector2 wayPointPosition = wayPoint.GetPosition(CallerActor, offset);

                if (inMiniMap) {
                    RenderingHelper.RenderLineInMiniMap(previousWayPointPosition, wayPointPosition, lineColor, width);
                } else {
                    RenderingHelper.RenderLineInMonitorWindow(previousWayPointPosition, wayPointPosition, lineColor, width);
                }                
                
                previousWayPointPosition = wayPointPosition;
            }

            Vector2 endPosition = HandlerActor.Position;
            if (wayPoints.Count > 0) {
                endPosition += (wayPoints[wayPoints.Count - 1].GridPosition - CallerActor.GridPosition).GetPerpendicular().normalized * offset;
            } else {
                endPosition += (HandlerActor.GridPosition - CallerActor.GridPosition).GetPerpendicular().normalized * offset;
            }

            if (inMiniMap) {
                RenderingHelper.RenderLineInMiniMap(previousWayPointPosition, endPosition, lineColor, width);
            } else {
                RenderingHelper.RenderLineInMonitorWindow(previousWayPointPosition, endPosition, lineColor, width);
            }
        }

        private void RenderArrow(bool reversed, float offset, int width) {
            Vector2 direction;
            Vector2 position;

            Vector2 startPos;
            Vector2 endPos;
            if (!reversed) {
                startPos = CallerActor.Position;
                if (wayPoints.Count == 0) {
                    endPos = HandlerActor.Position;
                } else {
                    endPos = wayPoints[0].GetPosition(CallerActor, 0.0f);
                }
            } else {
                startPos = HandlerActor.Position;
                if (wayPoints.Count == 0) {
                    endPos = CallerActor.Position;
                } else {
                    endPos = wayPoints[wayPoints.Count - 1].GetPosition(CallerActor, 0.0f);
                }
            }

            direction = (endPos - startPos).normalized;
            position = Vector2.Lerp(startPos, endPos, wayPoints.Count == 0 ? .45f : .9f);

            Vector2 perpendicular = direction.GetPerpendicular();
            Vector2 leftLine = position + ArrowSize * (-direction + perpendicular);
            Vector2 rightLine = position + ArrowSize * (-direction - perpendicular);

            Vector2 positionOffset = perpendicular * offset;

            RenderingHelper.RenderLineInMonitorWindow(position + positionOffset, leftLine + positionOffset, lineColor, width);
            RenderingHelper.RenderLineInMonitorWindow(position + positionOffset, rightLine + positionOffset, lineColor, width);
        }

        private void RenderDots(bool reversed, float offset) {
            const int dotCount = 2;
            for (int i = 0; i < dotCount; i++) {
                float timeSinceLastSend = age - (reversed ? lastReverseSendTime : lastSendTime);
                if (timeSinceLastSend <= DotMaxAge) {
                    lineColor.a = Mathf.Sin((1.0f - timeSinceLastSend / DotMaxAge) * Mathf.PI * 0.5f);
                    float lerp = (age / (totalGridDistance) + (float)i / dotCount) % 1;
                    if (reversed) {
                        lerp = 1.0f - lerp;
                    }
                    lineColor.a *= Mathf.Sin(lerp * Mathf.PI);
                    Vector2 position = GetPositionOnLine(lerp, offset);
                    RenderingHelper.RenderDotInMonitorWindow(position, lineColor, 6, 6);
                }
            }
        }

    }

}