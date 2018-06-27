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
    /// Utility class to look at an absolute position
    /// </summary>
    public class FaceTargetPosition : MonoBehaviour
    {
        #region Private Variables
        private Vector3 _targetPosition;

        [SerializeField, Tooltip("Turning Speed (degrees per sec)")]
        private float _turningSpeed = 45.0f;
        #endregion

        #region Properties
        public Vector3 TargetPosition
        {
            set
            {
                _targetPosition = value;
            }
        }

        public float TurningSpeed
        {
            set
            {
                _turningSpeed = value;
            }
        }
        #endregion

        #region Unity Methods
        /// <summary>
        /// Face towards target position while maintaining global up
        /// </summary>
        void Update ()
        {
            Vector3 desiredForward = _targetPosition - transform.position;
            if (desiredForward.sqrMagnitude < Mathf.Epsilon)
            {
                return;
            }
            Quaternion desiredOrientation = Quaternion.LookRotation(desiredForward, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredOrientation, _turningSpeed * Time.deltaTime);
        }
        #endregion
    }
}
