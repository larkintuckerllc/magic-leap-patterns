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
    /// Updates followers to face this object
    /// </summary>
    public class DeepSeaExplorerLauncher : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("Position offset of the explorer's target relative to Reference Transform")]
        private Vector3 _positionOffset;

        [SerializeField, Tooltip("Prefab of the Deep Sea Explorer")]
        private GameObject _explorerPrefab;
        private FaceTargetPosition[] _followers;

        [SerializeField, Tooltip("Desired number of explorers. Each explorer will have a different mass and turning speed combination")]
        private int _numExplorers = 3;
        private float _minMass = 4;
        private float _maxMass = 16;
        private float _minTurningSpeed = 30;
        private float _maxTurningSpeed = 90;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Validates variables and creates the deep sea explorers
        /// </summary>
        void Awake ()
        {
            if (null == _explorerPrefab)
            {
                Debug.LogError("DeepSeaExplorerLauncher._deepSeaExplorer not set, disabling script.");
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// Recreate explorers if we are reenabled while a target is found
        /// </summary>
        void OnEnable()
        {
            CreateExplorers();
        }

        /// <summary>
        /// Destroy all explorers immediately
        /// </summary>
        void OnDisable()
        {
            DestroyExplorers();
        }
        
        /// <summary>
        /// Update followers of the new position
        /// </summary>
        void Update()
        {
            Vector3 position = GetPosition();
            foreach (FaceTargetPosition follower in _followers)
            {
                if (follower)
                {
                    follower.TargetPosition = position;
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Create the Deep Sea Explorers with unique parameters
        /// </summary>
        private void CreateExplorers()
        {
            if (null == _followers)
            {
                _followers = new FaceTargetPosition[_numExplorers];
            }

            float massInc = (_maxMass - _minMass) / _numExplorers;
            float turningSpeedInc = (_maxTurningSpeed - _minTurningSpeed) / _numExplorers;
            Vector3 position = GetPosition();
            for (int i = 0; i < _numExplorers; ++i)
            {
                if (_followers[i])
                {
                    continue;
                }

                GameObject explorer = Instantiate(_explorerPrefab, position, Quaternion.identity);

                _followers[i] = explorer.AddComponent<FaceTargetPosition>();
                _followers[i].TurningSpeed = _minTurningSpeed + (i * turningSpeedInc);

                // Mass would be inversely proportional to turning speed (lower mass leads to lower acceleration -> needs higher turning rate)
                Rigidbody body = explorer.GetComponent<Rigidbody>();
                if (body)
                {
                    body.mass = _maxMass - (i * massInc);
                }
            }
        }

        /// <summary>
        /// Destroy all explorers
        /// </summary>
        private void DestroyExplorers()
        {
            foreach (FaceTargetPosition follower in _followers)
            {
                if (follower)
                {
                    Destroy(follower.gameObject);
                }
            }
        }

        /// <summary>
        /// Calculate and return the position which the explorers should look at
        /// </summary>
        /// <returns>The absolute position of the new target</returns>
        private Vector3 GetPosition()
        {
            return transform.position + transform.TransformDirection(_positionOffset);
        }
        #endregion
    }
}
