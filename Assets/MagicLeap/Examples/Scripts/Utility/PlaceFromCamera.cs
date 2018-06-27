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
    /// <summary>
    /// This class handles setting the position and rotation of the
    /// transform to match the camera's based on input distance and height
    /// </summary>
    public class PlaceFromCamera : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("The distance from the camera through its forward vector.")]
        private float _distance = 0.0f;

        [SerializeField, Tooltip("The distance on the Y axis from the camera.")]
        private float _height = 0.0f;

        [SerializeField, Tooltip("The approximate time it will take to reach the current position.")]
        private float _positionSmoothTime = 5f;
        private Vector3 _positionVelocity = Vector3.zero;

        [SerializeField, Tooltip("The approximate time it will take to reach the current rotation.")]
        private float _rotationSmoothTime = 5f;

        [SerializeField, Tooltip("Toggle to set position on awake.")]
        private bool _placeOnAwake = false;

        [SerializeField, Tooltip("Toggle to set position on update.")]
        private bool _placeOnUpdate = false;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Set the transform from latest position if flag is checked.
        /// </summary>
        void Awake()
        {
            if (_placeOnAwake)
            {
                UpdateTransform();
            }
        }

        private void Update()
        {
            if (_placeOnUpdate && Camera.main.transform.hasChanged)
            {
                UpdateTransform();
            }
        }

        private void OnValidate()
        {
            _positionSmoothTime = Mathf.Max(0.01f, _positionSmoothTime);
            _rotationSmoothTime = Mathf.Max(0.01f, _rotationSmoothTime);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Reset position and rotation to match current camera values.
        /// </summary>
        public void UpdateTransform()
        {
            Camera camera = Camera.main;

            // Move the object CanvasDistance units in front of the camera.
            Vector3 upVector = new Vector3(0.0f, _height, 0.0f);
            Vector3 targetPosition = camera.transform.position + upVector + (camera.transform.forward * _distance);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _positionVelocity, _positionSmoothTime);

            // Rotate the object to face the camera.
            Quaternion targetRotation = Quaternion.LookRotation(transform.position - camera.transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / _rotationSmoothTime);
        }
        #endregion
    }
}
