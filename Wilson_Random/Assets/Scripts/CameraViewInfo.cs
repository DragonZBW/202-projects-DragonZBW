// Suppress warnings about variables being set to null or having no initial value
#pragma warning disable 649

using UnityEngine;

/// <summary>
/// - Author: Zachary Wilson  <br/>
/// - A component that provides basic information about a camera view and displays
///   it on screen if this camera is active. <br/>
/// - The GameObject this is attached to must have a Camera component.
/// </summary>
public class CameraViewInfo : MonoBehaviour
{
    // The information to display on screen
    [TextArea]
    [SerializeField] private string dispInfo;

    /// <summary>
    /// Called when this object is active to display GUI.
    /// Displays a box with text describing the camera
    /// </summary>
    private void OnGUI()
    {
        GUI.skin.box.fontSize = 24;
        GUILayout.Box(dispInfo);
    }
}

#pragma warning restore 649