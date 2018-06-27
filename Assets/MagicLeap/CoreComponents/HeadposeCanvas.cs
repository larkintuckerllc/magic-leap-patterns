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

using UnityEngine;

namespace MagicLeap
{
    ///<summary>
    /// Script used to position this Canvas object directly in front of the user by
    /// using lerp functionality to give it a smooth look. Components on the canvas
    /// should function normally.
    ///</summary>
    [RequireComponent(typeof(Canvas))]
    public class HeadposeCanvas : MonoBehaviour
    {
        #region Public Variables
        [Tooltip("The distance from the camera that this object should be placed.")]
        public float CanvasDistance = 1.5f;

        [Tooltip("The speed at which this object changes it's position.")]
        public float PositionLerpSpeed = 5f;

        [Tooltip("The speed at which this object changes it's rotation.")]
        public float RotationLerpSpeed = 5f;
        #endregion

        #region Private Varibles
        // The canvas that is attached to this object.
        private Canvas _canvas;

        // The camera this object will be in front of.
        private Camera _camera;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes variables and verifies that necesary components exist.
        /// </summary>
        void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _camera = _canvas.worldCamera;

            // Disable this component if
            // it failed to initialzie properly.
            if(_canvas == null)
            {
                Debug.LogError("Error HeadposeCanvas._canvas is not set, disabling script.");
                enabled = false;
                return;
            }
            if(_camera == null)
            {
                Debug.LogError("Error HeadposeCanvas._camera is not set, disabling script.");
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// Update position and rotation of this canvas object to face the camera using lerp for smoothness.
        /// </summary>
        void Update()
        {
            // Move the object CanvasDistance units in front of the camera.
            float posSpeed = Time.deltaTime * PositionLerpSpeed;
            Vector3 posTo = _camera.transform.position + (_camera.transform.forward * CanvasDistance);
            transform.position = Vector3.SlerpUnclamped(transform.position, posTo, posSpeed);

            // Rotate the object to face the camera.
            float rotSpeed = Time.deltaTime * RotationLerpSpeed;
            Quaternion rotTo = Quaternion.LookRotation(transform.position - _camera.transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotTo, rotSpeed);
        }
        #endregion
    }
}
