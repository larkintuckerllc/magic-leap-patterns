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

#if UNITY_EDITOR || PLATFORM_LUMIN

using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// Wraps the MLSpatialMapper and sets it's query params based on the transform of
    /// the object with this compontent.
    /// This class also provides the functionality of clearing the mesh
    /// </summary>
    public class SpatialMapper : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("Reference to MLSpatialMapper")]
        private MLSpatialMapper _MLSpatialMapper;

        [SerializeField, Tooltip("A visual representation of the meshing bounds.")]
        private GameObject _visualBounds;
        #endregion

        #region Events
        public event Action OnClearMesh;
        #endregion

        #region Public Properties
        public MLSpatialMapper MLSpatialMapper
        {
            get
            {
                return _MLSpatialMapper;
            }
        }
        #endregion

        #region Unity Methods
        /// <summary>
        /// Check for input parameters to be initialized.
        /// </summary>
        void Awake()
        {
            if (_MLSpatialMapper == null)
            {
                Debug.LogError("Error SpatialMapper._MLSpatialMapper is not set, disabling script.");
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// Updates the query parameters for the MLSpatialMapper.
        /// </summary>
        void Update()
        {
            _MLSpatialMapper.transform.position = transform.position;
            _MLSpatialMapper.transform.rotation = transform.rotation;
            _MLSpatialMapper.transform.localScale = transform.localScale;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Switches the meshtype to the next type.
        /// </summary>
        public void ClearMesh()
        {
            foreach (GameObject obj in _MLSpatialMapper.meshIdToGameObjectMap.Values)
            {
                Destroy(obj);
            }
            _MLSpatialMapper.meshIdToGameObjectMap.Clear();

            if (OnClearMesh != null)
            {
                OnClearMesh();
            }
        }

        public void ShowBounds(bool enabled)
        {
            if(_visualBounds != null)
            {
                _visualBounds.SetActive(enabled);
            }
        }
        #endregion
    }
}

#endif
