// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2017 Magic Leap, Inc. (COMPANY) All Rights Reserved.
// Magic Leap, Inc. Confidential and Proprietary
//
//  NOTICE:  All information contained herein is, and remains the property
//  of COMPANY. The intellectual and technical concepts contained herein
//  are proprietary to COMPANY and may be covered by U.S. and Foreign
//  Patents, patents in process, and are protected by trade secret or
//  copyright law.  Dissemination of this information or reproduction of
//  this material is strictly forbidden unless prior written permission is
//  obtained from COMPANY.  Access to the source code contained herein is
//  hereby forbidden to anyone except current COMPANY employees, managers
//  or contractors who have executed Confidentiality and Non-disclosure
//  agreements explicitly covering such access.
//
//  The copyright notice above does not evidence any actual or intended
//  publication or disclosure  of  this source code, which includes
//  information that is confidential and/or proprietary, and is a trade
//  secret, of  COMPANY.   ANY REPRODUCTION, MODIFICATION, DISTRIBUTION,
//  PUBLIC  PERFORMANCE, OR PUBLIC DISPLAY OF OR THROUGH USE  OF THIS
//  SOURCE CODE  WITHOUT THE EXPRESS WRITTEN CONSENT OF COMPANY IS
//  STRICTLY PROHIBITED, AND IN VIOLATION OF APPLICABLE LAWS AND
//  INTERNATIONAL TREATIES.  THE RECEIPT OR POSSESSION OF  THIS SOURCE
//  CODE AND/OR RELATED INFORMATION DOES NOT CONVEY OR IMPLY ANY RIGHTS
//  TO REPRODUCE, DISCLOSE OR DISTRIBUTE ITS CONTENTS, OR TO MANUFACTURE,
//  USE, OR SELL ANYTHING THAT IT  MAY DESCRIBE, IN WHOLE OR IN PART.
//
// %COPYRIGHT_END%
// --------------------------------------------------------------------*/
// %BANNER_END%

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    [Flags]
    public enum KeyPoseTypes
    {
        Finger = (1 << 0),
        Fist = (1 << 1),
        Pinch = (1 << 2),
        Thumb = (1 << 3),
        L = (1 << 4),
        OpenHandBack = (1 << 5),
        Ok = (1 << 6),
        C = (1 << 7),
        NoPose = (1 << 8)
    }

    /// <summary>
    /// Component used to communicate with the MLHands API and manage
    /// which KeyPoses are currently being tracked by each hand.
    /// KeyPoses can be added and removed from the tracker during runtime.
    /// </summary>
    public class HandTracking : MonoBehaviour
    {
        #region Private Variables
        [Space, SerializeField, BitMask(typeof(KeyPoseTypes)), Tooltip("All KeyPoses to be tracked")]
        private KeyPoseTypes _trackedKeyPoses;

        [SerializeField]
        private MLKeyPointFilterLevel _keyPointFilterLevel = MLKeyPointFilterLevel.ExtraSmoothed;

        [SerializeField]
        private MLPoseFilterLevel _PoseFilterLevel = MLPoseFilterLevel.ExtraRobust;
        #endregion

        #region Public Properties
        public KeyPoseTypes TrackedKeyPoses { get; private set; }
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes and finds references to all relevant components in the
        /// scene and registers required events.
        /// </summary>
        void OnEnable()
        {
            MLResult result = MLHands.Start();
            if (!result.IsOk)
            {
                Debug.LogError("Error HandTracking starting MLHands, disabling script.");
                enabled = false;
                return;
            }

            UpdateKeyPoseStates(true);

            MLHands.KeyPoseManager.SetKeyPointsFilterLevel(_keyPointFilterLevel);
            MLHands.KeyPoseManager.SetPoseFilterLevel(_PoseFilterLevel);
        }

        /// <summary>
        /// Stops the communication to the MLHands API and unregisters required events.
        /// </summary>
        void OnDisable()
        {
            if (MLHands.IsStarted)
            {
                // Disable all KeyPoses if MLHands was started
                // and is about to stop
                UpdateKeyPoseStates(false);
            }
            MLHands.Stop();
        }

        /// <summary>
        /// Update KeyPoses tracked if enum value changed.
        /// </summary>
        void Update()
        {
            if ((_trackedKeyPoses ^ TrackedKeyPoses) != 0)
            {
                UpdateKeyPoseStates(true);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds KeyPose if it's not there already.
        /// </summary>
        /// <param name="keyPose"> KeyPose to add. </param>
        public void AddKeyPose(KeyPoseTypes keyPose)
        {
            if ((keyPose & _trackedKeyPoses) != keyPose)
            {
                _trackedKeyPoses |= keyPose;
                UpdateKeyPoseStates(true);
            }
        }

        /// <summary>
        /// Removes KeyPose if it's there.
        /// </summary>
        /// <param name="keyPose"> KeyPose to remove. </param>
        public void RemoveKeyPose(KeyPoseTypes keyPose)
        {
            if ((keyPose & _trackedKeyPoses) == keyPose)
            {
                _trackedKeyPoses ^= keyPose;
                UpdateKeyPoseStates(true);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Get the KeyPoses enabled.
        /// </summary>
        /// <returns> The array of KeyPoses being tracked.</returns>
        private MLHandKeyPose[] GetKeyPoseTypes()
        {
            int[] enumValues = (int[])Enum.GetValues(typeof(KeyPoseTypes));
            List<MLHandKeyPose> keyPoses = new List<MLHandKeyPose>();

            TrackedKeyPoses = 0;
            KeyPoseTypes current;
            for (int i = 0; i < enumValues.Length; ++i)
            {
                current = (KeyPoseTypes)enumValues[i];
                if ((_trackedKeyPoses & current) == current)
                {
                    TrackedKeyPoses |= current;
                    keyPoses.Add((MLHandKeyPose)i);
                }
            }

            return keyPoses.ToArray();
        }

        /// <summary>
        /// Updates the list of KeyPoses internal to the magic leap device,
        /// enabling or disabling KeyPoses that should be tracked.
        /// </summary>
        private void UpdateKeyPoseStates(bool enableState = true)
        {
            MLHandKeyPose[] keyPoseTypes = GetKeyPoseTypes();

            // Early out in case there are no KeyPoses to enable.
            if (keyPoseTypes.Length == 0)
            {
                MLHands.KeyPoseManager.DisableAllKeyPoses();
                return;
            }

            bool status = MLHands.KeyPoseManager.EnableKeyPoses(keyPoseTypes, enableState, true);
            if (!status)
            {
                Debug.LogError("HandTracking failed during a call to enable tracked KeyPoses.\n"
                    + "Disabling HandTracking component.");
                enabled = false;
                return;
            }
        }
        #endregion
    }
}
