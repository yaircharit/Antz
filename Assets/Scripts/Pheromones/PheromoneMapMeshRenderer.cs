using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PheromoneMapMeshRenderer : MonoBehaviour
{
    private Mesh mesh;
    private MeshFilter meshFilter;

    // Cached data lists reused every frame for performance
    private readonly List<Vector3> verts = new();
    private readonly List<int> tris = new();
    private readonly List<Color> colors = new();

    public float cellSize = 0.9f;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh
        {
            name = "PheromoneMesh"
        };

        // Mesh needs per-vertex colors enabled
        meshFilter.mesh = mesh;
    }

    /// <summary>
    /// Build mesh from pheromone map dictionary.
    /// </summary>
    public void BuildMesh(Dictionary<PheromoneType, Dictionary<Vector3Int, Pheromone>> grids)
    {
        verts.Clear();
        tris.Clear();
        colors.Clear();

        int index = 0;

        foreach (var typeGrid in grids)
        {
            foreach (var phero in typeGrid.Value.Values)
            {
                if (phero.Value <= 0) continue;

                AddQuad(phero.Position +Vector3.up*0.1f, phero.Color, ref index);
            }
        }

        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetColors(colors);
        mesh.RecalculateBounds();
    }

    private void AddQuad(Vector3 pos, Color c, ref int index)
    {
        float half = cellSize * 0.5f;

        // Quad on floor (XZ plane)
        verts.Add(new Vector3(pos.x - half, pos.y, pos.z - half));
        verts.Add(new Vector3(pos.x - half, pos.y, pos.z + half));
        verts.Add(new Vector3(pos.x + half, pos.y, pos.z + half));
        verts.Add(new Vector3(pos.x + half, pos.y, pos.z - half));

        colors.Add(c);
        colors.Add(c);
        colors.Add(c);
        colors.Add(c);

        // Two triangles
        tris.Add(index + 0);
        tris.Add(index + 1);
        tris.Add(index + 2);
        tris.Add(index + 0);
        tris.Add(index + 2);
        tris.Add(index + 3);

        index += 4;
    }
}
