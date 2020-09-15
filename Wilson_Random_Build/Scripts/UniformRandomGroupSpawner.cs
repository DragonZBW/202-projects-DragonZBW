// Suppress warnings about variables being set to null or having no initial value
#pragma warning disable 649

using UnityEngine;

/// <summary>
/// - Author: Zachary Wilson <br/>
/// - Spawns a group of GameObjects in a region using basic uniform random distribution.
/// - The spawn area is centered around this GameObject's position and consists of a
///   rectangle rotated according to this GameObject's y rotation.
/// </summary>
public class UniformRandomGroupSpawner : MonoBehaviour
{
    // The prefab to spawn
    [SerializeField] private GameObject prefab;
    // How many of the prefabs to spawn
    [SerializeField] private int numSpawns = 20;

    // The size of the spawn area
    [SerializeField] private float areaSizeX = 10;
    [SerializeField] private float areaSizeZ = 10;

    // Should the spawned objects' y rotation be randomized
    [SerializeField] private bool randomizeYRotation = true;

    // Should the gizmo showing the spawn area persist when the object is deselected
    [SerializeField] private bool persistGizmo = false;

    /// <summary>
    /// Called when the scene starts.
    /// Calls the method to spawn the objects.
    /// </summary>
    private void Start()
    {
        // Check to see if the prefab is null. If it is, display a warning and don't spawn
        if (prefab == null)
        {
            Debug.LogWarning("Prefab not set, nothing will be spawned.");
            return;
        }

        SpawnObjects();
    }

    /// <summary>
    /// Spawn the GameObjects based on the fields.
    /// - Precondition: prefab field must not be null.
    /// </summary>
    private void SpawnObjects()
    {
        // Spawn numSpawns number of objects
        for(int i = 0; i < numSpawns; i++)
        {
            // Determine rotation
            // - Identity if no randomization
            // - Otherwise randomize y rotation
            Quaternion rotation = Quaternion.identity;
            if(randomizeYRotation)
            {
                rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            }

            // Instantiate the object
            Instantiate(prefab,
                new Vector3(transform.position.x + Random.Range(-areaSizeX * .5f, areaSizeX * .5f),
                            transform.position.y,
                            transform.position.z + Random.Range(-areaSizeZ * .5f, areaSizeZ * .5f)),
                rotation);
        }
    }

    /// <summary>
    /// Called in edit mode and draws a box to show the area
    /// the objects will be spawned in.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Only draw the box if the object is selected or persistGizmo is true
        if (!UnityEditor.Selection.Contains(gameObject) && !persistGizmo)
        {
            return;
        }

        // Draw a box showing the spawn area

        // Get the direction the spawner faces
        float direction = transform.eulerAngles.y;

        // Find a foward vector
        Vector3 forward = new Vector3(-Mathf.Sin(Mathf.Deg2Rad * -direction), 0, Mathf.Cos(Mathf.Deg2Rad * -direction));
        // Find a vector perpendicular to the direction the spawner faces, for displaying horizontal spawn range
        Vector3 perp = new Vector3(forward.z, 0, -forward.x);

        // Determine box's points
        Vector3 p1 = transform.position - (perp * areaSizeX * .5f) - (forward * areaSizeZ * .5f);
        Vector3 p2 = transform.position - (perp * areaSizeX * .5f) + (forward * areaSizeZ * .5f);
        Vector3 p3 = transform.position + (perp * areaSizeX * .5f) + (forward * areaSizeZ * .5f);
        Vector3 p4 = transform.position + (perp * areaSizeX * .5f) - (forward * areaSizeZ * .5f);

        // Draw the box
        Gizmos.color = Color.white;
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
    }
}

#pragma warning restore 649