using UnityEngine;

/// <summary>
/// - Author: Zachary Wilson <br/>
/// - A component that will simply lock the mouse cursor when the game
///   starts or when the user clicks the screen. Unlock the cursor by
///   pressing Escape.
/// </summary>
public class LockCursor : MonoBehaviour
{
    /// <summary>
    /// Called when the scene starts.
    /// This method will lock the cursor in place.
    /// </summary>
    private void Start()
    {
        // Lock the cursor in place
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Called every frame.
    /// Check if the user clicked the LMB, and if they did, lock the cursor.
    /// </summary>
    private void Update()
    {
        // If the user clicks, lock the cursor
        if(Input.GetMouseButtonDown(0))
        {
            // Lock the cursor in place
            Cursor.lockState = CursorLockMode.Locked;
        }

        // If the user presses escape, unlock the cursor
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            // Unlock the cursor
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
