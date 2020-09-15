// Suppress warnings about variables being set to null or having no initial value
#pragma warning disable 649

using UnityEngine;

public class CameraSwapper : MonoBehaviour
{
    // Camera array that holds a reference to every camera in the scene
    // Made this an array of GameObjects so the FPSController could be included
    [SerializeField] private GameObject[] cameras;

    // Index of the current camera in the cameras array
    private int currentCameraIndex = 0;

    /// <summary>
    /// Called when the scene starts.
    /// Find all cameras and set things up.
    /// </summary>
    private void Start()
    {
        // If there are no cameras, no need to continue
        if (cameras.Length < 1)
        {
            return;
        }

        // Turn all cameras off, except the inital camera
        for(int i = 1; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(false);
        }

        // Make sure the current camera is enabled
        cameras[currentCameraIndex].SetActive(true);
    }

    /// <summary>
    /// Called every frame.
    /// Check input to allow camera switching.
    /// </summary>
    private void Update()
    {
        // Press the 'C' key to cycle through cameras
        if(Input.GetKeyDown(KeyCode.C))
        {
            // Cycle to the next camera
            currentCameraIndex++;

            // Check if the index is in bounds and set cameras active/inactive accordingly
            if (currentCameraIndex < cameras.Length)
            {
                cameras[currentCameraIndex - 1].SetActive(false);
                cameras[currentCameraIndex].SetActive(true);
            }
            else
            {
                cameras[currentCameraIndex - 1].SetActive(false);
                currentCameraIndex = 0;
                cameras[currentCameraIndex].SetActive(true);
            }
        }
    }
}

#pragma warning restore 649