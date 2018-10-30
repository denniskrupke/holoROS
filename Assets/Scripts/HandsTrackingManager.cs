// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;



namespace HoloToolkit.Unity
{
    /// <summary>
    /// HandsManager determines if the hand is currently detected or not.
    /// </summary>
    public partial class HandsTrackingManager : Singleton<HandsTrackingManager>
    {
        /// <summary>
        /// HandDetected tracks the hand detected state.
        /// Returns true if the list of tracked hands is not empty.
        /// </summary>
        public bool HandDetected
        {
            get { return trackedHands.Count > 0; }
        }

        public GameObject TrackingObject;
        public Transform PlayerPosition;
        public LineRenderer lineRenderer;
        public GameObject objectToMoveByHand;

        private HashSet<uint> trackedHands = new HashSet<uint>();
        private Dictionary<uint, GameObject> trackingObject = new Dictionary<uint, GameObject>();

        private Vector3 lastDirection = new Vector3(0, 0, 1);
        private Vector3 correction = new Vector3(0, .15f, 0);


        public Vector3 GetLastDirection() { return this.lastDirection; }

        void Awake()
        {
            InteractionManager.InteractionSourceDetectedLegacy += InteractionManager_SourceDetected;
            InteractionManager.InteractionSourceLostLegacy += InteractionManager_SourceLost;
            InteractionManager.InteractionSourceUpdatedLegacy += InteractionManager_SourceUpdated;
        }

        private void InteractionManager_SourceUpdated(UnityEngine.XR.WSA.Input.InteractionSourceState state)
        {
            uint id = state.source.id;
            Vector3 pos;

            if (state.source.kind == UnityEngine.XR.WSA.Input.InteractionSourceKind.Hand)
            {
                if (trackingObject.ContainsKey(state.source.id))
                {
                    if (state.sourcePose.TryGetPosition(out pos))
                    {
                        trackingObject[state.source.id].transform.position = pos;
                        objectToMoveByHand.transform.position = pos;


                        //var heading = pos - PlayerPosition.position;
                        var heading = pos - PlayerPosition.position;
                        var distance = heading.magnitude;
                        var direction = heading / distance; // This is now the normalized direction.

                        var correctedHeading = (pos + correction) - PlayerPosition.position;
                        var correctedDistance = correctedHeading.magnitude;
                        var correctedDirection = correctedHeading / correctedDistance;

                        this.lastDirection = correctedDirection;

                        //Debug.DrawRay(PlayerPosition.position, direction, Color.green);
                        //lineRenderer.SetPosition(0, PlayerPosition.position);
                        //lineRenderer.SetPosition(1, pos + correction);// + direction * 2.0f);

                        if (RobotAR.StateMachine.GetCurrentState() == RobotAR.State.Pick)
                        {

                        }

                    }
                }
            }

        }

        private void InteractionManager_SourceDetected(UnityEngine.XR.WSA.Input.InteractionSourceState state)
        {
            // Check to see that the source is a hand.
            if (state.source.kind != UnityEngine.XR.WSA.Input.InteractionSourceKind.Hand)
            {
                return;
            }
            trackedHands.Add(state.source.id);

            var obj = Instantiate(TrackingObject) as GameObject;
            Vector3 pos;

            if (state.sourcePose.TryGetPosition(out pos))
            {
                obj.transform.position = pos;
                
                var heading = pos - PlayerPosition.position;
                var distance = heading.magnitude;
                var direction = heading / distance; // This is now the normalized direction.

                var correctedHeading = (pos + correction) - PlayerPosition.position;
                var correctedDistance = correctedHeading.magnitude;
                var correctedDirection = correctedHeading / correctedDistance;

                this.lastDirection = correctedDirection;

                //Debug.DrawRay(PlayerPosition.position, lastDirection, Color.green);
                //lineRenderer.SetPosition(0, PlayerPosition.position);
                //lineRenderer.SetPosition(1, pos);
            }

            trackingObject.Add(state.source.id, obj);
        }

        private void InteractionManager_SourceLost(UnityEngine.XR.WSA.Input.InteractionSourceState state)
        {
            // Check to see that the source is a hand.
            if (state.source.kind != UnityEngine.XR.WSA.Input.InteractionSourceKind.Hand)
            {
                return;
            }

            if (trackedHands.Contains(state.source.id))
            {
                trackedHands.Remove(state.source.id);
            }

            if (trackingObject.ContainsKey(state.source.id))
            {
                var obj = trackingObject[state.source.id];
                trackingObject.Remove(state.source.id);
                Destroy(obj);
            }
        }

        void OnDestroy()
        {
            InteractionManager.InteractionSourceDetectedLegacy -= InteractionManager_SourceDetected;
            InteractionManager.InteractionSourceLostLegacy -= InteractionManager_SourceLost;
            InteractionManager.InteractionSourceUpdatedLegacy -= InteractionManager_SourceUpdated;
        }
    }
}