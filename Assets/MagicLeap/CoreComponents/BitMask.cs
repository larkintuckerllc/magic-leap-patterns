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

using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary> 
/// Custom attribute to make it easy to turn enum fields into bit masks in
/// the inspector. The enum type must be defined in order for the inspector
/// to be able to know what the bits should be set to.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class BitMask : PropertyAttribute
{
    /// <summary>
    /// The Type of the Enum that is being turned into a bit mask.
    /// </summary>
    public Type PropertyType;

    /// <summary> 
    /// Creates a new instance of BitMask with the passed in
    /// enum Type. This constructor call is automatic when
    /// decorating a field with this Attribute.
    /// </summary>
    /// <param name="propertyType">The Type value of the enum</param>
    public BitMask(Type propertyType)
    {
        PropertyType = propertyType;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(BitMask))]
public class BitMaskPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Type propertyType = (attribute as BitMask).PropertyType;

        string[] enumNames = Enum.GetNames(propertyType);
        int[] enumValues = (int[])Enum.GetValues(propertyType);

        int curIntValue = property.intValue;
        int curMaskValue = 0;

        for(int index = 0; index < enumValues.Length; ++index)
        {
            if ((curIntValue & enumValues[index]) == enumValues[index])
            {
                curMaskValue |= 1 << index;
            }
        }

        // Draw the field using the built in MaskField functionality
        // However, since MaskField has no reference to the System.Type
        // of our enum, the value that is returned will not be shifted
        int newMaskValue = EditorGUI.MaskField(position, label, curMaskValue, enumNames);

        // Reset the current value
        curIntValue = 0;

        // Go through each value in the new mask and set the correct bit
        for(int index = 0; index < enumValues.Length; ++index)
        {
            if((newMaskValue & (1 << index)) == (1 << index))
            {
                curIntValue |= enumValues[index];
            }
        }

        // Make sure to set the value of the property in the end
        property.intValue = curIntValue;
    }
}
#endif
