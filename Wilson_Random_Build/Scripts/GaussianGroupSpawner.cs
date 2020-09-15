// Suppress warnings about variables being set to null or having no initial value
#pragma warning disable 649

using UnityEngine;

/// <summary>
/// - Author: Zachary Wilson <br/>
/// - Spawns a group of GameObjects in a row with normally (gaussian) distributed
///   heights and widths.
/// </summary>
public class GaussianGroupSpawner : MonoBehaviour
{
    // The prefab to spawn
    [SerializeField] private GameObject prefab;
    // How many of the prefabs to spawn
    [SerializeField] private int numSpawns = 1;
    // How much space to put between the prefabs
    [SerializeField] private float spacing = 1;

    // How far from the base line the objects can be placed (perpendicularly)
    [SerializeField] private float horizontalVariation = .5f;

    // Gaussian distribution that controls the heights of the spawned objects
    [SerializeField] GaussianDistribution heightDistribution;
    // Gaussian distribution that controls the X/Z scale of the spawned objects
    [SerializeField] GaussianDistribution widthDistribution;

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
    /// Spawn GameObjects with Gaussian distributed heights and widths in a line
    /// according to the fields.
    /// - Precondition: prefab must not be null
    /// </summary>
    private void SpawnObjects()
    {
        // Determine the direction the objects will be spawned in based on this object's rotation
        float direction = transform.eulerAngles.y;

        // Calculate a vector that will be used to space out the spawns, based on direction
        Vector3 spacingVector = spacing * new Vector3(-Mathf.Sin(Mathf.Deg2Rad * -direction), 0, Mathf.Cos(Mathf.Deg2Rad * -direction));
        // Calculate a vector that will be used to space out the spawns randomly along the axis perpendicular to direction
        Vector3 perpVector = spacingVector.normalized;
        perpVector.x *= -1;
        perpVector = new Vector3(perpVector.z, 0, perpVector.x);

        // Instantiate the objects
        for(int i = 0; i < numSpawns; i++)
        {
            // Instantiate the object at the proper position and rotation
            Transform instance = Instantiate(prefab, transform.position + (i * spacingVector), Quaternion.Euler(0,direction,0)).transform;
            // Move the object randomly along the axis perpendicular to direction, within the range [-horizontalVariation, horizontalVariation]
            instance.Translate(perpVector * Random.Range(-horizontalVariation, horizontalVariation));

            // Set the object's scale according to Gaussian distributed random values
            float width = widthDistribution.RandomNum();
            float height = heightDistribution.RandomNum();
            instance.localScale = new Vector3(width, height, width);
        }
    }

    /// <summary>
    /// Called in edit mode and draws a box to show the direction and area
    /// the objects will be spawned in.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Only draw the box if the object is selected or persistGizmo is true
        if (!UnityEditor.Selection.Contains(gameObject) && !persistGizmo)
        {
            return;
        }

        // Draw a box in the direction that this object faces.

        /* The box's length is equal to spacing * (numSpawns - 1),
         * as this is the amount of space the spawned objects will take up. */
        float boxLength = spacing * (numSpawns - 1);

        // Get the direction the spawner faces
        float direction = transform.eulerAngles.y;

        // Find a foward vector
        Vector3 forward = new Vector3(-Mathf.Sin(Mathf.Deg2Rad * -direction), 0, Mathf.Cos(Mathf.Deg2Rad * -direction));
        // Find a vector perpendicular to the direction the spawner faces, for displaying horizontal spawn range
        Vector3 perp = new Vector3(Mathf.Cos(Mathf.Deg2Rad * -direction), 0, Mathf.Sin(Mathf.Deg2Rad * -direction)).normalized * horizontalVariation;

        // Draw the box
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position - perp,
            transform.position - perp + boxLength * forward);
        Gizmos.DrawLine(transform.position + perp,
            transform.position + perp + boxLength * forward);
        Gizmos.DrawLine(transform.position - perp, transform.position + perp);
        Gizmos.DrawLine(transform.position - perp + boxLength * forward,
            transform.position + perp + boxLength * forward);
    }
}

/// <summary>
/// - Author: Zachary Wilson <br/>
/// - Representation of a Gaussian distribution with a mean and standard deviation.
///   Can be used to generate random values according to the Gaussian distribution.
/// </summary>
[System.Serializable]
public struct GaussianDistribution
{
    /// <summary>
    /// The mean (average) value of the distribution.
    /// </summary>
    [SerializeField] private float mean;
    /// <summary>
    /// The standard deviation of the distribution. A high
    /// standard deviation indicates that values are likely to be further
    /// from the mean value than with a low standard deviation.
    /// </summary>
    [SerializeField] private float standardDeviation;

    /// <summary>
    /// Generate a random number using Gaussian/normal distribution. (Taken from the Random slides)
    /// </summary>
    /// <returns>A random number generated according to the given Gaussian distribution parameters.</returns>
    public float RandomNum()
    {
        float val1 = Random.Range(0f, 1f);
        float val2 = Random.Range(0f, 1f);
        float gaussValue =
                 Mathf.Sqrt(-2.0f * Mathf.Log(val1)) *
                 Mathf.Sin(2.0f * Mathf.PI * val2);
        return mean + standardDeviation * gaussValue;
    }
}

#pragma warning restore 649