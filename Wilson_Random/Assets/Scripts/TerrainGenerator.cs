// Suppress warnings about variables being set to null or having no initial value
#pragma warning disable 649

using UnityEngine;
using UnityEditor;

/// <summary>
/// - Author: Zachary Wilson <br/>
/// - Terrain generator using multiple layers of Perlin noise. This script will generate
///   heightmaps and can also automatically apply textures to the terrain based on heights.
///   This script is to be placed on a terrain GameObject and will operate on that
///   terrain's terrainData. <br/>
/// - This script only works on single terrain chunks. Additionally, there are some restrictions
///   regarding the water prefab. Graphically, its Z and X size should both be 1. Its scale
///   should also be set to 1,1,1. Otherwise it may not scale correctly to fill the terrain's
///   area. It should also have its anchor point in the center of the water plane.
/// </summary>
[RequireComponent(typeof(TerrainCollider))]
public class TerrainGenerator : MonoBehaviour
{
    // EXPOSED PRIVATE FIELDS

    // Should the terrain generate when the scene is started?
    [SerializeField] private bool generateOnStart = true;

    [Header("Generation Parameters")]
    // Seed for the Perlin noise (determines offset)
    [SerializeField] private float seed;
    // Step length between perlin values
    [Range(0f, 1f)]
    [SerializeField] private float step = .05f;
    // Number of octaves to layer
    [SerializeField] private int octaves = 3;
    // How much the frequency (step) of octaves increases
    [SerializeField] private float lacunarity = 2;
    // How much the data of octaves should be multiplied by before being added to the heights
    [Range(0f,1f)]
    [SerializeField] private float persistence = .5f;

    [Header("Water")]
    [SerializeField] private GameObject waterPrefab;
    // How high is the water
    [Range(0f, 1f)]
    [SerializeField] private float waterLevel = .5f;

    [Header("Terrain Textures")]
    // Textures to be applied to the terrain based on heights
    [SerializeField] private HeightmappedTexture[] textures;

    // PRIVATE FIELDS

    // Terrain data of the attached Terrain component
    private TerrainData terrainData;
    // Heightmap array
    private float[,] heights;


    // METHODS

    /// <summary>
    /// Called when the scene starts.
    /// Do setup and call the terrain gen method if necessary.
    /// </summary>
    private void Start()
    {
        // Set variables that need to be set from other components
        terrainData = GetComponent<Terrain>().terrainData;
        heights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

        // Generate terrain
        if (generateOnStart)
        {
            Generate();
        }
    }

    /// <summary>
    /// Generates terrain based on the fields.
    /// </summary>
    private void Generate()
    {
        // SETTING HEIGHTS
        // Keep track of maximum generated height so the heightmap can be normalized to have a max height of 1
        float maxHeight = 0f;

        // Generating heights
        for(int i = 0; i < heights.GetLength(0); i++)
        {
            for(int j = 0; j < heights.GetLength(1); j++)
            {
                // Temp variable to keep track of height for addition of octaves
                float height = 0;

                // Generate octaves of noise and add them
                for(int octave = 0; octave < octaves; octave++)
                {
                    // Calculate lacunarity multiplier for this octave
                    float lacunarityMult = Mathf.Pow(lacunarity, octave);
                    // Calculate noise for this octave and add to height
                    height += Mathf.PerlinNoise(seed + step * lacunarityMult * i, seed + step * lacunarityMult * j) * Mathf.Pow(persistence, octave);
                }

                // Once all the octaves have been added together, store the temp height in the heights array
                heights[i, j] = height;
                // Need to check for max height
                if (height > maxHeight)
                {
                    maxHeight = height;
                }
            }
        }

        // Normalize the heightmap's values between 0 and 1 (based on max height)
        for(int i = 0; i < heights.GetLength(0); i++)
        {
            for(int j = 0; j < heights.GetLength(1); j++)
            {
                heights[i, j] /= maxHeight;
            }
        }

        // Set the terrain heights to the generated heightmap
        terrainData.SetHeights(0, 0, heights);


        // SETTING TEXTURES
        // Get terrain layers from textures array
        TerrainLayer[] layers = new TerrainLayer[textures.Length];
        for (int i = 0; i < textures.Length; i++)
        {
            // Get the appropriate terrain layer
            layers[i] = textures[i].texture;
        }
        // Set the terrain data's layers to the array of layers created from the textures
        terrainData.terrainLayers = layers;

        // Painting textures on the terrain
        // Make an alphamap array to keep track of texture painting
        /* Unity terrains use a 3D array of floats for alphamaps/splatmaps, where the first 
         * two indices are the coordinates and the 3rd is the texture. The value represents 
         * the opacity of the texture at those coordinates. */
        float[,,] alphamap = new float[terrainData.alphamapResolution, terrainData.alphamapResolution, textures.Length];

        // Iterate through terrain alphamap
        for(int i = 0; i < terrainData.alphamapResolution; i++)
        {
            for(int j = 0; j < terrainData.alphamapResolution; j++)
            {
                // Sample the height at the current position
                float height = terrainData.GetInterpolatedHeight((float)j / terrainData.alphamapResolution, (float)i / terrainData.alphamapResolution);
                // Map height to a number 0-1
                height /= terrainData.size.y;

                // Determine the texture to use based on height
                int texToUse = -1;
                for (int t = 0; t < textures.Length; t++)
                {
                    HeightmappedTexture tex = textures[t];
                    if (tex.minHeight <= height && (texToUse == -1 || tex.minHeight > textures[texToUse].minHeight))
                    {
                        texToUse = t;
                    }
                }

                // Set the texture on the alphamap array. Just set opacity to 1 (full).
                // All other textures have an opacity of 0 by default.
                alphamap[i, j, texToUse] = 1;
            }
        }
        // Set the terrain's textures
        terrainData.SetAlphamaps(0, 0, alphamap);

        // ADDING WATER
        // Instantiate a plane of water
        Transform waterT = Instantiate(waterPrefab, transform).transform;
        // Move the water to the center of the terrain
        waterT.localPosition = new Vector3(terrainData.size.x / 2f,
                                           waterLevel * terrainData.size.y,
                                           terrainData.size.z / 2f);
        // Scale the water to cover the whole terrain
        waterT.localScale = new Vector3(terrainData.size.x, 1, terrainData.size.z);
    }
}

/// <summary>
/// - Author: Zachary Wilson <br/>
/// - A texture for terrain that is to be applied only at or above a certain height.
///   Used by the TerrainGenerator component to apply height-based textures. <br/>
/// - Note that although it is possible to programatically set a minHeight of less
///   than 0 or greater than 1, it serves no functional purpose as the TerrainGenerator
///   only works with height values in the range [0,1].
/// </summary>
[System.Serializable]
public struct HeightmappedTexture
{
    /// <summary>
    /// The terrain layer. This is the texture that will be painted on a terrain at or above minHeight.
    /// </summary>
    public TerrainLayer texture;
    /// <summary>
    /// The minimum height that this texture can be painted at on a terrain.
    /// Should be between 0 and 1.
    /// </summary>
    [Range(0f,1f)] public float minHeight;
}

#pragma warning restore 649