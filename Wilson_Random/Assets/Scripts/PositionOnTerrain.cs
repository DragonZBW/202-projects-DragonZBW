using UnityEngine;

/// <summary>
/// - Author: Zachary Wilson <br/>
/// - Simple script to automatically move a GameObject up to the active terrain's
///   height at the GameObject's x/z position when the scene starts. <br/>
/// - This moves the GameObject's center, so there is no guarantee that the object
///   will be placed entirely above the terrain. Also note that this script will not
///   work if used on a GameObject that also has the FPSController script.
/// </summary>
public class PositionOnTerrain : MonoBehaviour
{
    // How far above the terrain to position the object
    [SerializeField] private float offsetAboveTerrain = 0.5f;

    /// <summary>
    /// Called when the scene starts.
    /// Positions the object on the active terrain.
    /// </summary>
    private void Start()
    {
        // Move this object to the terrain height
        float height = Terrain.activeTerrain.SampleHeight(transform.position) + Terrain.activeTerrain.GetPosition().y;
        transform.position = new Vector3(transform.position.x, height + offsetAboveTerrain, transform.position.z);
    }
}
