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
using UnityEngine;
using UnityEngine.Experimental.XR;

namespace MagicLeap
{
    /// <summary>
    /// This class allows you to change meshing properties at runtime, including the rendering mode.
    /// Manages the MLSpatialMapper behaviour and tracks the meshes.
    /// </summary>
    public class MeshingVisualizer : MonoBehaviour
    {
        public enum RenderMode
        {
            None,
            Wireframe,
            Occlusion
        }

        #region Private Variables
        [SerializeField, Tooltip("The spatial mapper from which to get update on mesh types.")]
        private SpatialMapper _spatialMapper;

        [SerializeField, Tooltip("The material to apply for occlusion.")]
        private Material _occlusionMaterial;

        [SerializeField, Tooltip("The material to apply for wireframe rendering.")]
        private Material _wireframeMaterial;

        private Dictionary<TrackableId, MeshRenderer> _meshRenderers = new Dictionary<TrackableId, MeshRenderer>();
        private RenderMode _renderMode = RenderMode.Wireframe;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Start listening for MLSpatialMapper events.
        /// </summary>
        void Awake()
        {
            // Validate all required game objects.
            if (_spatialMapper == null)
            {
                Debug.LogError("MeshingVisualizer._spatialMapper is not set, disabling script!");
                enabled = false;
                return;
            }

            _spatialMapper.OnClearMesh += HandleClearMeshes;
        }

        /// <summary>
        /// Register for new and updated freagments.
        /// </summary>
        void Start()
        {
            _spatialMapper.MLSpatialMapper.meshAdded += HandleOnMeshReady;
            _spatialMapper.MLSpatialMapper.meshUpdated += HandleOnMeshReady;
        }

        /// <summary>
        /// Unregister callbacks.
        /// </summary>
        void OnDestroy()
        {
            _spatialMapper.MLSpatialMapper.meshAdded -= HandleOnMeshReady;
            _spatialMapper.MLSpatialMapper.meshUpdated -= HandleOnMeshReady;
            _spatialMapper.OnClearMesh -= HandleClearMeshes;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Set the render material on the meshes.
        /// </summary>
        /// <param name="mode">The render mode that should be used on the material.</param>
        public void SetRenderers(RenderMode mode)
        {
            // Set the render mode.
            _renderMode = mode;

            // Update the material applied to all the MeshRenderers.
            foreach (MeshRenderer mesh in _meshRenderers.Values)
            {
                UpdateRenderer(mesh);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates the currently selected render material on the MeshRenderer.
        /// </summary>
        /// <param name="meshRenderer">The MeshRenderer that should be updated.</param>
        private void UpdateRenderer(MeshRenderer meshRenderer)
        {
            if (meshRenderer != null)
            {
                // Toggle the GameObject(s) and set the correct materia based on the current RenderMode.
                if (_renderMode == RenderMode.None)
                {
                    meshRenderer.enabled = false;
                }
                else if (_renderMode == RenderMode.Wireframe)
                {
                    meshRenderer.enabled = true;
                    meshRenderer.material = _wireframeMaterial;
                }
                else if (_renderMode == RenderMode.Occlusion)
                {
                    meshRenderer.enabled = true;
                    meshRenderer.material = _occlusionMaterial;
                }
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the MeshReady event, which tracks and assigns the correct mesh renderer materials.
        /// </summary>
        /// <param name="meshId">Id of the mesh that got added / upated.</param>
        private void HandleOnMeshReady(TrackableId meshId)
        {
            if (_spatialMapper.MLSpatialMapper.meshIdToGameObjectMap.ContainsKey(meshId))
            {
                MeshRenderer meshRenderer = _spatialMapper.MLSpatialMapper.meshIdToGameObjectMap[meshId].GetComponent<MeshRenderer>();

                // Append the unique MeshRenderer to the dictonary and update the material on that instance.
                if (!_meshRenderers.ContainsKey(meshId))
                {
                    _meshRenderers.Add(meshId, meshRenderer);
                }
                UpdateRenderer(meshRenderer);
            }
        }

        /// <summary>
        /// Clear all meshes.
        /// </summary>
        public void HandleClearMeshes()
        {
            _meshRenderers.Clear();
        }
        #endregion
    }
}

#endif
