using Assets.Scripts.WorldGeneration;
using UnityEngine;

public class Chunk
{
    public static readonly int chunkSize = 16;
    public static readonly int floorHeight = 16;

    // Derived values computed once
    private static readonly int chunkBitCount;
    private static readonly int mask;
    private static readonly int volume;

    public Vector3Int chunkCoord;
    public Block[] blocks; // Flattened 1D array
    public bool isDirty;
    public bool hasMesh;
    private ChunkRenderer renderer;

    static Chunk()
    {
        // Validate power-of-two and compute bitcount
        if ((chunkSize & (chunkSize - 1)) != 0)
        {
            throw new System.InvalidOperationException("chunkSize must be a power of two.");
        }

        int s = chunkSize;
        int count = 0;
        while (s > 1)
        {
            s >>= 1;
            count++;
        }
        chunkBitCount = count;
        mask = chunkSize - 1;
        volume = 1 << (chunkBitCount * 3); // chunkSize^3
    }

    public Chunk(Vector3Int coord)
    {
        chunkCoord = coord;
        // Allocate using precomputed volume (1 << (3*bitcount))
        blocks = new Block[volume];
        GenerateBlocks();
    }

    // Convert x,y,z to 1D index using bit shifts (faster when chunkSize is power of two)
    public int GetIndex(int x, int y, int z)
    {
        // x occupies lower chunkBitCount bits,
        // y is shifted by chunkBitCount, z by chunkBitCount*2
        return x | (y << chunkBitCount) | (z << (chunkBitCount * 2));
    }

    // World to Chunk Coord (uses arithmetic right shift -> floor division for powers of two)
    public static Vector3Int WorldToChunkCoord(Vector3Int worldPos)
    {
        return new Vector3Int(
            worldPos.x >> chunkBitCount,
            worldPos.y >> chunkBitCount,
            worldPos.z >> chunkBitCount
        );
    }

    // World to Local Tile Coord (bitwise modulo)
    public static Vector3Int WorldToLocalCoord(Vector3Int worldPos)
    {
        return new Vector3Int(
            worldPos.x & mask,
            worldPos.y & mask,
            worldPos.z & mask
        );
    }

    // Replace the lines in GenerateBlocks() that attempt to use a non-existent Block constructor
    public void GenerateBlocks()
    {
        // Simple example: fill bottom half with solid blocks, top half with air
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < floorHeight; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    int index = GetIndex(x, y, z);
                    // Ensure the struct is updated to represent a solid block
                    blocks[index].id = 1; // Solid block (e.g., dirt)
                    blocks[index].flags = 1; // Set IsSolid bit
                }
            }
        }
        isDirty = true;
    }

    public void Render()
    {
        if (renderer == null)
        {
            GameObject chunkObj = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}_{chunkCoord.z}");
            chunkObj.transform.parent = World.Instance.transform;
            chunkObj.transform.position = new Vector3(chunkCoord.x * chunkSize, chunkCoord.y * chunkSize, chunkCoord.z * chunkSize);
            renderer = chunkObj.AddComponent<ChunkRenderer>();
        }
        Mesh mesh = renderer.GenerateMesh(this);
        renderer.GetComponent<MeshFilter>().mesh = mesh;
        renderer.GetComponent<MeshCollider>().sharedMesh = mesh;

        hasMesh = true;
    }
}