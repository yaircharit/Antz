
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public static World Instance { get; private set; }

    public BlockDefinition[] blockDefinitions; // Assign in inspector
    public Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();


    [SerializeField] private int renderDistance = 5;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            blockDefinitions = Resources.LoadAll<BlockDefinition>("Blocks");
        }
    }

    public Chunk AddChunk(Vector3Int chunkCoord)
    {
        if (!Instance.chunks.ContainsKey(chunkCoord))
        {
            Chunk newChunk = new Chunk(chunkCoord);
            newChunk.chunkCoord = chunkCoord;
            Instance.chunks.Add(chunkCoord, newChunk);
            return newChunk;
        }
        return Instance.chunks[chunkCoord];
    }

    public void RemoveChunk(Vector3Int chunkCoord)
    {
        if (Instance.chunks.ContainsKey(chunkCoord))
        {
            Instance.chunks.Remove(chunkCoord);
        }
    }

    public Chunk GetChunk(Vector3Int chunkCoord)
    {
        return AddChunk(chunkCoord); // Ensure chunk exists
    }

    public void RenderChunk(Vector3Int chunkCoord)
    {
        Chunk chunk = AddChunk(chunkCoord); // Ensure chunk exists
        chunk.Render();
    }

    public void RenderChunks(Vector3Int pos)
    {
        // Render chunks within render distance
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                Vector3Int chunkCoord = pos + new Vector3Int(x, 0, z);
                RenderChunk(chunkCoord);
            }
        }
    }
}