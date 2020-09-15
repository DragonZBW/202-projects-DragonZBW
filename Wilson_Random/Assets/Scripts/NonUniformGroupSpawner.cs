// Suppress warnings about variables being set to null or having no initial value
#pragma warning disable 649

using UnityEngine;
using UnityEditor;

/// <summary>
/// - Author: Zachary Wilson <br/>
/// - Spawns a group of GameObjects in a certain area, using non-uniform
///   random probabilities to determine where the objects are spawned.
/// - The positions of the GameObjects will always be in front of this object
///   (in the positive z direction) and their rotatins will always face in
///   the opposite direction of this object.
/// </summary>
public class NonUniformGroupSpawner : MonoBehaviour
{
    // The prefab to spawn
    [SerializeField] private GameObject prefab;
    // How many of the prefabs to spawn
    [SerializeField] private int numSpawns = 20;

    // The size of the spawn area
    [SerializeField] private float areaSizeX = 6;
    [SerializeField] private float areaSizeZ = 16;

    // The number of spawn regions to divide the spawn area into
    [SerializeField] private int regions = 4;
    // The amount to multiply the spawn chance by for each region
    // to make later regions have lower spawn chances
    [Range(0f,1f)]
    [SerializeField] private float spawnChanceFalloff = .75f;

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
    /// Spawn GameObjects with a non-uniform random distribution in the area
    /// specified by the fields areaSizeX and areaSizeZ and according to the
    /// spawning parameter fields.
    /// - Precondition: prefab must not be null
    /// </summary>
    private void SpawnObjects()
    {
        // Add up regions' spawn chances to get their actual probabilities
        float[] regionProbabilities = new float[regions];
        float probSum = 0;
        for(int i = 0; i < regions; i++)
        {
            regionProbabilities[i] = Mathf.Pow(spawnChanceFalloff, i);
            probSum += regionProbabilities[i];
        }
        // Normalize probabilities between 0 and 1
        for(int i = 0; i < regions; i++)
        {
            regionProbabilities[i] /= probSum;
        }

        // Spawn numSpawns number of objects
        for(int i = 0; i < numSpawns; i++)
        {
            // Generate a random number to determine which region to spawn in
            float regionDet = Random.Range(0f, 1f);

            // Check regions' probabilities to determine which region to spawn in
            float curProbability = 0;
            int spawnRegion = -1;
            for(int r = 0; r < regions; r++)
            {
                curProbability += regionProbabilities[r];
                if(regionDet <= curProbability)
                {
                    spawnRegion = r;
                    break;
                }
            }

            // Spawn a GameObject in the selected region at a random position
            float direction = transform.eulerAngles.y;
            Vector3 forwardVector = new Vector3(-Mathf.Sin(Mathf.Deg2Rad * -direction), 0, Mathf.Cos(Mathf.Deg2Rad * -direction));
            Vector3 perpVector = new Vector3(forwardVector.z, 0, -forwardVector.x);
            Instantiate(prefab,
                transform.position +
                    (spawnRegion * (areaSizeZ / regions) * forwardVector) +
                    (Random.Range(0f,1f) * (areaSizeZ / regions) * forwardVector) +
                    (Random.Range(-.5f,.5f) * areaSizeX * perpVector),
                Quaternion.Euler(0, transform.eulerAngles.y + 180, 0));
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Called in edit mode and draws a box to show the direction and area
    /// the objects will be spawned in.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Only draw the box if the object is selected or persistGizmo is true
        if (!Selection.Contains(gameObject) && !persistGizmo)
        {
            return;
        }

        // Draw a box in the direction that this object faces.

        // Get the direction the spawner faces
        float direction = transform.eulerAngles.y;

        // Find a foward vector
        Vector3 forward = new Vector3(-Mathf.Sin(Mathf.Deg2Rad * -direction), 0, Mathf.Cos(Mathf.Deg2Rad * -direction));
        // Find a vector perpendicular to the direction the spawner faces, for displaying horizontal spawn range
        Vector3 perp = new Vector3(Mathf.Cos(Mathf.Deg2Rad * -direction), 0, Mathf.Sin(Mathf.Deg2Rad * -direction)).normalized * (areaSizeX / 2f);

        // Draw the box
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position - perp,
            transform.position - perp + areaSizeZ * forward);
        Gizmos.DrawLine(transform.position + perp,
            transform.position + perp + areaSizeZ * forward);
        Gizmos.DrawLine(transform.position - perp, transform.position + perp);
        Gizmos.DrawLine(transform.position - perp + areaSizeZ * forward,
            transform.position + perp + areaSizeZ * forward);

        // Draw region separators
        Gizmos.color = Color.yellow;
        for(int i = 0; i < regions; i++)
        {
            Vector3 regionOffset = forward * i * (areaSizeZ / regions);
            Gizmos.DrawLine(transform.position - perp + regionOffset, transform.position + perp + regionOffset);
        }
    }
#endif
}

#pragma warning restore 649