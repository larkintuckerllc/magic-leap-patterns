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
    /// This class is responsible for creating the planet, moving the planet, and destroying the planet
    /// as well as creating the explorers
    /// </summary>
    public class DeepSpaceExplorerLauncher : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("Position offset of the explorer's target relative to Reference Transform")]
        private Vector3 _positionOffset;

        [Header("Planet")]
        [SerializeField, Tooltip("Prefab of the Planet")]
        private Animator _planetPrefabAnimator;
        private Transform _planetInstance;
        private Vector3 _planetVel;

        [Header("Explorer")]
        [SerializeField, Tooltip("Prefab of the Deep Space Explorer")]
        private GameObject _explorerPrefab;

        private float _timeLastLaunch = 0;
        [SerializeField, Tooltip("Time interval between instances (seconds)")]
        private float _timeInterval = 0.5f;

        [SerializeField, Tooltip("Minimum distance from the center of the planet")]
        private float _minOrbitRadius = 0.1f;
        [SerializeField, Tooltip("Maximum distance from the center of the planet")]
        private float _maxOrbitRadius = 0.2f;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Validates input variables
        /// </summary>
        void Awake()
        {
            if (null == _planetPrefabAnimator)
            {
                Debug.LogError("DeepSpaceExplorerPivot._planetPrefabAnimator not set, disabling script.");
                enabled = false;
                return;
            }
            if (null == _explorerPrefab)
            {
                Debug.LogError("DeepSpaceExplorerPivot._explorerPrefab not set, disabling script");
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// Creates an instance of the planet
        /// </summary>
        void OnEnable()
        {
            _planetInstance = Instantiate(_planetPrefabAnimator.transform, GetPosition(), Quaternion.identity);
        }

        /// <summary>
        /// Destroys the planet instance
        /// </summary>
        void OnDisable()
        {
            if (null != _planetInstance)
            {
                _planetInstance.GetComponent<Animator>().Play("EarthShrinking");
                Destroy(_planetInstance.gameObject, 1.1f);
                _planetInstance = null;
            }
        }

        /// <summary>
        /// Update planet position and launch explorers
        /// </summary>
        void Update()
        {
            Vector3 position = GetPosition();

            // Update planet position
            _planetInstance.position = Vector3.SmoothDamp(_planetInstance.position, position, ref _planetVel, 1.0f);

            // Launch explorers
            if (Time.time - _timeInterval > _timeLastLaunch)
            {
                _timeLastLaunch = Time.time;
                GameObject explorer = Instantiate(_explorerPrefab, position, Random.rotation);
                DeepSpaceExplorerController explorerController = explorer.GetComponent<DeepSpaceExplorerController>();
                if (explorerController)
                {
                    explorerController.OrbitRadius = Random.Range(_minOrbitRadius, _maxOrbitRadius);
                }
            }
        }
        #endregion

        #region Private Methods
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
