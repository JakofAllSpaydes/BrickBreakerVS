using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public GameObject gridSquarePrefab;
    public GameObject playerBallPrefab1, playerBallPrefab2;
    public GameObject wallPrefab;
    public int gridWidth;
    public int gridHeight;
    public float squareSpacing = 1f;
    public List<Material> materials = new List<Material>();

    public float ballSpawnYAdjustment = 10;
    public GameManager gameManager;

    private void Start()
    {
        GenerateGrid();
        SpawnPlayerBalls();
        CreatePerimeterWalls();
        gameManager = FindObjectOfType<GameManager>();
        int halfTheBlocks = (gridWidth * gridHeight) / 2;
        gameManager.player1Blocks = halfTheBlocks;
        gameManager.player2Blocks = halfTheBlocks;
        gameManager.SetTotalBlocks(gridWidth * gridHeight);
    }

    void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                GameObject square = Instantiate(gridSquarePrefab, new Vector3(x * squareSpacing, 0, z * squareSpacing), Quaternion.identity);
                square.transform.parent = this.transform;

                // Determine ownership based on position
                int owner = x < gridWidth / 2 ? 1 : 2;

                // Use the Block component to set ownership, which will handle material and layer
                Block blockComponent = square.GetComponent<Block>();
                if (blockComponent != null)
                {
                    blockComponent.SetOwnership(owner);
                }

                square.name = $"Square_{x}_{z}";
            }
        }
    }

    void SpawnPlayerBalls()
    {
        // Calculate the middle position for player 1 and player 2
        Vector3 middleWhite = new Vector3((gridWidth / 4) * squareSpacing, ballSpawnYAdjustment, (gridHeight / 2) * squareSpacing);
        Vector3 middleBlack = new Vector3((3 * gridWidth / 4) * squareSpacing, ballSpawnYAdjustment, (gridHeight / 2) * squareSpacing);

        Instantiate(playerBallPrefab1, middleWhite, Quaternion.identity);
        Instantiate(playerBallPrefab2, middleBlack, Quaternion.identity);
    }

    void CreatePerimeterWalls()
    {
        // Top and bottom walls
        for (int x = 0; x < gridWidth; x++)
        {
            Instantiate(wallPrefab, new Vector3(x * squareSpacing, 0, -squareSpacing), Quaternion.identity, transform); // Bottom
            Instantiate(wallPrefab, new Vector3(x * squareSpacing, 0, gridHeight * squareSpacing), Quaternion.identity, transform); // Top
        }

        // Left and right walls
        for (int z = 0; z < gridHeight; z++)
        {
            Instantiate(wallPrefab, new Vector3(-squareSpacing, 0, z * squareSpacing), Quaternion.identity, transform); // Left
            Instantiate(wallPrefab, new Vector3(gridWidth * squareSpacing, 0, z * squareSpacing), Quaternion.identity, transform); // Right
        }
    }
}