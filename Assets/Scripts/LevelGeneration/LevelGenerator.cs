using UnityEngine;

// This script creates a flat level out of prefabs (like ground tiles and walls).
// It can be expanded to spawn enemies, collectibles, and more in the future.
// Attach this to an empty GameObject in your scene and assign prefab references in Inspector.
public class LevelGenerator : MonoBehaviour
{
    public GameObject[] groundTiles; // Array of ground tile prefabs to pick from randomly
    public GameObject wallTile; // Single wall prefab
    public GameObject foregroundWire; // Optional foreground asset to place every X tiles

    public int levelLength = 100; // Number of tiles to spawn horizontally
    public float tileWidth = 1f; // How wide each tile is
    public float heightVariance = 1f; // How much the ground can change up/down between tiles

    private float currentHeight = 0f; // Keeps track of vertical offset for each tile

    void Start()
    {
        GenerateLevel(); // Run generation once at the start
    }

    void GenerateLevel()
    {
        for (int i = 0; i < levelLength; i++)
        {
            // Pick a random tile from the array
            GameObject tile = groundTiles[Random.Range(0, groundTiles.Length)];

            // Randomly adjust vertical position a little to add variation
            float randomHeightChange = Random.Range(-heightVariance, heightVariance);
            currentHeight = Mathf.Clamp(currentHeight + randomHeightChange, -2f, 2f);

            // Calculate where to place the tile
            Vector2 pos = new Vector2(i * tileWidth, currentHeight);
            Instantiate(tile, pos, Quaternion.identity);

            // Occasionally add a wall above the tile (10% chance)
            if (Random.value < 0.1f)
            {
                Vector2 wallPos = pos + Vector2.up; // Place wall one unit above ground
                Instantiate(wallTile, wallPos, Quaternion.identity);
            }

            // Every 20 tiles, add a foreground wire (optional visual detail)
            if (foregroundWire != null && i % 20 == 0)
            {
                Vector2 wirePos = pos + Vector2.up * 5; // Higher up so it's over Carl
                Instantiate(foregroundWire, wirePos, Quaternion.identity);
            }
        }
    }
}
