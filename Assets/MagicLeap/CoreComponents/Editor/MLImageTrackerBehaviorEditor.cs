// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright company="Magic Leap">
// Copyright (c) 2017 Magic Leap, Inc. (COMPANY) All Rights Reserved.
// Magic Leap, Inc. Confidential and Proprietary
//
// NOTICE: All information contained herein is, and remains the property
// of COMPANY. The intellectual and technical concepts contained herein
// are proprietary to COMPANY and may be covered by U.S. and Foreign
// Patents, patents in process, and are protected by trade secret or
// copyright law. Dissemination of this information or reproduction of
// this material is strictly forbidden unless prior written permission is
// obtained from COMPANY. Access to the source code contained herein is
// hereby forbidden to anyone except current COMPANY employees, managers
// or contractors who have executed Confidentiality and Non-disclosure
// agreements explicitly covering such access.
//
// The copyright notice above does not evidence any actual or intended
// publication or disclosure of this source code, which includes
// information that is confidential and/or proprietary, and is a trade
// secret, of COMPANY. ANY REPRODUCTION, MODIFICATION, DISTRIBUTION,
// PUBLIC PERFORMANCE, OR PUBLIC DISPLAY OF OR THROUGH USE OF THIS
// SOURCE CODE WITHOUT THE EXPRESS WRITTEN CONSENT OF COMPANY IS
// STRICTLY PROHIBITED, AND IN VIOLATION OF APPLICABLE LAWS AND
// INTERNATIONAL TREATIES. THE RECEIPT OR POSSESSION OF THIS SOURCE
// CODE AND/OR RELATED INFORMATION DOES NOT CONVEY OR IMPLY ANY RIGHTS
// TO REPRODUCE, DISCLOSE OR DISTRIBUTE ITS CONTENTS, OR TO MANUFACTURE,
// USE, OR SELL ANYTHING THAT IT MAY DESCRIBE, IN WHOLE OR IN PART.
// </copyright>
//
// %COPYRIGHT_END%
// --------------------------------------------------------------------
// %BANNER_END%

using System.IO;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.Experimental.XR.MagicLeap
{
    /// <summary>
    /// MLImageTrackerBehaviorEditor is an editor script that performs
    /// extra validation on the imported texture and displays errors to
    /// the user if the import settings are not optimal for image tracking.
    /// </summary>
    [CustomEditor(typeof(MLImageTrackerBehavior))]
    public class MLImageTrackerBehaviorEditor : Editor
    {
        /// <summary>
        /// Adds on-screen validation that examines the selected image
        /// to ensure it meets the requirements for image tracking.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Texture2D image = (target as MLImageTrackerBehavior).Image;

            if (image == null)
            {
                return;
            }

            // From the image object's instance, we can get the path to it's file
            // in the editor hierarchy, and in turn get the instance of it's importer.
            string assetPath = AssetDatabase.GetAssetPath(image);
            TextureImporter importer = TextureImporter.GetAtPath(assetPath) as TextureImporter;

            string assetFileName = Path.GetFileName(assetPath);
            string helpBoxHeader = "Check the import settings for " + assetFileName + "\n\n";

            GUILayout.Space(5);

            if (image.format != TextureFormat.DXT1 && image.format != TextureFormat.RGBA32 && image.format != TextureFormat.RGB24)
            {
                EditorGUILayout.HelpBox("The image format is invalid, only (DXT1, RGBA32, RGB24) formats may be used.", MessageType.Error, true);
            }
            else
            {
                if (!importer.sRGBTexture)
                {
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.HelpBox(helpBoxHeader + "sRGB (Texture Color) should be set to 'True'", MessageType.Error, true);

                        if (GUILayout.Button("Fix", GUILayout.Width(75)))
                        {
                            importer.sRGBTexture = true;
                            importer.SaveAndReimport();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                if (importer.npotScale != TextureImporterNPOTScale.None)
                {
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.HelpBox(helpBoxHeader + "Non Power of 2 should be set to 'None'", MessageType.Error, true);

                        if (GUILayout.Button("Fix", GUILayout.Width(75)))
                        {
                            importer.npotScale = TextureImporterNPOTScale.None;
                            importer.SaveAndReimport();

                        }
                    }
                    GUILayout.EndHorizontal();
                }
                if (!importer.isReadable)
                {
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.HelpBox(helpBoxHeader + "Read/Write Enabled should be set to 'True'", MessageType.Error, true);

                        if (GUILayout.Button("Fix", GUILayout.Width(75)))
                        {
                            importer.isReadable = true;
                            importer.SaveAndReimport();

                        }
                    }
                    GUILayout.EndHorizontal();
                }
                if (importer.mipmapEnabled)
                {
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.HelpBox(helpBoxHeader + "Generate Mip Maps should be set to 'False'", MessageType.Error, true);

                        if (GUILayout.Button("Fix", GUILayout.Width(75)))
                        {
                            importer.mipmapEnabled = false;
                            importer.SaveAndReimport();

                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}
