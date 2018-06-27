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
using UnityEngine.Experimental.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// This class implements the behavior for the object with this component to constantly
    /// move around an input transform.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PathAroundObject : MonoBehaviour
    {
        #region Public Methods
        [Tooltip("Transform of the object to path around.")]
        public Transform TargetObject;
        #endregion

        #region Private Methods
        [SerializeField, Tooltip("Maximum distance from target to go to. (Min Value: 2)")]
        private float _maxDistance = 2.0f;

        [SerializeField, Tooltip("Maximum speed for the object to move in.")]
        private float _maxSpeed = 0.1f;

        private Vector3 _targetPos;
        private Vector3 _maxDistanceVect;
        private const float _maxRotationDelta = Mathf.PI / 6.0f;
        private const float _maxTime = 3.0f;
        private float _timer;
        private Rigidbody _rigid;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Validate that _maxDistance is not less than minimium required.
        /// </summary>
        public void OnValidate()
        {
            if (_maxDistance < 2.0f)
            {
                Debug.LogWarning("You can not have a MaxDistance less than 2, setting back to default!");
                _maxDistance = 2.0f;
            }
        }

        /// <summary>
        /// Initialize variables
        /// </summary>
        void Awake()
        {
            if (TargetObject == null)
            {
                Debug.Log("Error PathAroundObject.TargetObject is not set, disabling script.");
                enabled = false;
                return;
            }
            _rigid = GetComponent<Rigidbody>();

            _maxDistanceVect = new Vector3(_maxDistance, _maxDistance, _maxDistance);
        }

        /// <summary>
        /// Set correct position and RigidBody properties.
        /// </summary>
        private void Start()
        {
            _rigid.useGravity = false;

            transform.position = TargetObject.position;
            _targetPos = transform.position;
        }

        /// <summary>
        /// Update object's transform to target position or get a new random position at max distance
        /// _maxDistance if object is close to target or enough time is passed.
        /// </summary>
        void Update()
        {
            if (TargetObject == null)
            {
                Debug.Log("Error PathAroundObject.TargetObject is not set, disabling script.");
                enabled = false;
                return;
            }

            _timer += Time.deltaTime;

            if (Vector3.Distance(transform.position, _targetPos) < 0.2f || _timer >= _maxTime)
            {
                Vector3 added = TargetObject.position + _maxDistanceVect;
                Vector3 subst = TargetObject.position - _maxDistanceVect;

                _targetPos = new Vector3(Random.Range(subst.x, added.x), Random.Range(subst.y, added.y), Random.Range(subst.z, added.z));

                _timer = 0.0f;
            }

            Quaternion _targetRot = Quaternion.LookRotation(_targetPos - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRot, _maxRotationDelta);
            _rigid.velocity = transform.forward * _maxSpeed;
        }
    }
    #endregion
}
