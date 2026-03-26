using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.WorldGeneration
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class ChunkRenderer : MonoBehaviour
    {
        private Chunk chunk;
        private List<Vector3> vertices;
        private List<int> triangles;
        private List<Vector2> uvs;

        private MeshFilter meshFilter;
        private MeshCollider meshCollider;
        private MeshRenderer meshRenderer;

        public void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshCollider = GetComponent<MeshCollider>();
            meshRenderer = GetComponent<MeshRenderer>();
            
            meshRenderer.materials = World.Instance.blockDefinitions.Select((def)=>def.material).ToArray();
        }

        /// <summary>
        /// Generates a mesh for the chunk with face culling
        /// </summary>
        public Mesh GenerateMesh(Chunk chunk)
        {
            this.chunk = chunk;

            vertices = new List<Vector3>();
            triangles = new List<int>();
            uvs = new List<Vector2>();

            // Iterate through all blocks in the chunk
            for (int x = 0; x < Chunk.chunkSize; x++)
            {
                for (int y = 0; y < Chunk.chunkSize; y++)
                {
                    for (int z = 0; z < Chunk.chunkSize; z++)
                    {
                        int blockIndex = chunk.GetIndex(x, y, z);
                        Block block = chunk.blocks[blockIndex];

                        // Skip air blocks
                        if (block.id == 0 || !block.IsSolid)
                            continue;

                        // Check each face of the block
                        for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                        {
                            // Only add face if adjacent block is air/not solid
                            if (ShouldRenderFace(x, y, z, faceIndex))
                            {
                                AddFaceToMesh(x, y, z, faceIndex);
                            }
                        }
                    }
                }
            }

            // Create and populate the mesh
            Mesh mesh = new Mesh();
            mesh.name = "ChunkMesh";
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;

            return mesh;
        }

        /// <summary>
        /// Determines if a face should be rendered based on adjacent block solidity
        /// </summary>
        private bool ShouldRenderFace(int x, int y, int z, int faceIndex)
        {
            Vector3 neighborOffset = BlockData.neighbors[faceIndex];
            int neighborX = x + (int)neighborOffset.x;
            int neighborY = y + (int)neighborOffset.y;
            int neighborZ = z + (int)neighborOffset.z;

            // Check bounds
            if (neighborX < 0 || neighborX >= Chunk.chunkSize ||
                neighborY < 0 || neighborY >= Chunk.chunkSize ||
                neighborZ < 0 || neighborZ >= Chunk.chunkSize)
            {
                // Edge of chunk - render face (would connect to adjacent chunks in full implementation)
                return true;
            }

            // Get neighbor block
            int neighborIndex = chunk.GetIndex(neighborX, neighborY, neighborZ);
            Block neighborBlock = chunk.blocks[neighborIndex];

            // Render if neighbor is air or not solid
            return neighborBlock.id == 0 || !neighborBlock.IsSolid;
        }

        /// <summary>
        /// Adds a single face to the mesh
        /// </summary>
        private void AddFaceToMesh(int x, int y, int z, int faceIndex)
        {
            Vector3 blockPosition = new Vector3(x, y, z);
            int vertexOffset = vertices.Count;

            // Get vertices for this face
            int[] faceVertexIndices = new int[4];
            for (int i = 0; i < 4; i++)
            {
                faceVertexIndices[i] = BlockData.sideVertices[faceIndex, i];
            }

            // Add vertices
            for (int i = 0; i < 4; i++)
            {
                Vector3 vertexPos = BlockData.vertices[faceVertexIndices[i]] + blockPosition;
                vertices.Add(vertexPos);
            }

            // Add triangles (2 triangles per face)
            int[] triangleTemplate = BlockData.trianglesTemplate;
            for (int i = 0; i < triangleTemplate.Length; i++)
            {
                triangles.Add(vertexOffset + triangleTemplate[i]);
            }

            // Add UVs
            foreach (Vector2 uv in BlockData.uvs)
            {
                uvs.Add(uv);
            }
        }
    }
}
