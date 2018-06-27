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

using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// This class provides the functionality for the object's transform assigned
    /// to this script to match the 6dof data from input when using a Control
    /// and the 3dof data when using the Mobile App.
    /// </summary>
    public class ControllerTransform : MonoBehaviour
    {
        #region Public Enum
        [System.Flags]
        public enum DeviceTypesAllowed : byte
        {
            Controller = 1,
            MobileApp = 2,
            All = Controller | MobileApp,
        }
        #endregion

        #region Private Variables
        [Space, SerializeField, Tooltip("Hand to get 6Dof data for.")]
        private MLInput.Hand _hand = MLInput.Hand.Left;

        [Space, SerializeField, Tooltip("Device types to get 6Dof data for.")]
        private DeviceTypesAllowed _devices = DeviceTypesAllowed.All;

        private MLInputController _mlInputController;
        private Camera _camera;

        private const float MLMA_FORWARD_DISTANCE_FROM_CAMERA = 0.75f;
        private const float MLMA_UP_DISTANCE_FROM_CAMERA = -0.1f;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initialize variables, callbacks and check null references.
        /// </summary>
        void Start()
        {
            MLResult result = MLInput.Start();
            if (!result.IsOk)
            {
                Debug.LogError("Error ControllerTransform starting MLInput, disabling script.");
                enabled = false;
                return;
            }

            _camera = Camera.main;

            MLInputController controller = MLInput.GetController(_hand);
            if (controller != null && controller.Connected && ((uint)(controller.Type) & (uint)(_devices)) != 0)
            {
                _mlInputController = controller;
            }
            else
            {
                _mlInputController = null;
            }

            MLInput.OnControllerConnected += HandleOnControllerConnected;
            MLInput.OnControllerDisconnected += HandleOnControllerDisconnected;
        }

        /// <summary>
        /// Update controller input based feedback.
        /// </summary>
        void Update()
        {
            if (_mlInputController != null && ((uint)(_mlInputController.Type) & (uint)(_devices)) != 0)
            {
                // Positional data not supported on Mobile App
                if (_mlInputController.Type == MLInputControllerType.Control)
                {
                    transform.position = _mlInputController.Position;
                }
                else if (_mlInputController.Type == MLInputControllerType.MobileApp)
                {
                    transform.position = _camera.transform.position + _camera.transform.forward * MLMA_FORWARD_DISTANCE_FROM_CAMERA + Vector3.up * MLMA_UP_DISTANCE_FROM_CAMERA;
                }

                transform.rotation = _mlInputController.Orientation;
            }
            else if(_mlInputController != null)
            {
                _mlInputController = null;
            }
        }

        /// <summary>
        /// Stop input api and unregister callbacks.
        /// </summary>
        void OnDestroy()
        {
            if (MLInput.IsStarted)
            {
                MLInput.OnControllerDisconnected -= HandleOnControllerDisconnected;
                MLInput.OnControllerConnected -= HandleOnControllerConnected;
                MLInput.Stop();
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the event for controller connected.
        /// Assign controller to connected controller if desired hand matches
        /// with new connected controller.
        /// </summary>
        /// <param name="controllerId"> The id of the controller. </param>
        private void HandleOnControllerConnected(byte controllerId)
        {
            if (_hand == MLInput.GetController(controllerId).Hand)
            {
                _mlInputController = MLInput.GetController(controllerId);
            }
        }

        /// <summary>
        /// Handles the event for controller disconnected.
        /// Remove controller reference if controller id matches
        /// with disconnected controller.
        /// </summary>
        /// <param name="controllerId"> The id of the controller. </param>
        private void HandleOnControllerDisconnected(byte controllerId)
        {
            if (_mlInputController != null && _mlInputController.Id == controllerId)
            {
                _mlInputController = null;
            }
        }
        #endregion
    }
}
