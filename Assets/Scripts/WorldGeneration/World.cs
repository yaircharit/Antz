
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public static World Instance { get; private set; }

    public BlockDefinition[] blockDefinitions; // Assign in inspector
    public Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();


    [SerializeField] private int renderDistance = 5;

    // TODO: Most funcs and vars should be static?


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
            RenderChunks(Vector3Int.zero); // Render the initial chunk at the start of the game
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

    public Block GetBlock(int x, int y, int z)
    {
        return GetChunk(Chunk.WorldToChunkCoord(x,y,z))
        .GetBlock(Chunk.WorldToLocalCoord(x,y,z));
    }
    public Block GetBlock(Vector3Int pos)
    {
        return GetBlock(pos.x,pos.y,pos.z);
    }
}