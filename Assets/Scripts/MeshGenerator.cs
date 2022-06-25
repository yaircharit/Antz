using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int board_size;
    [SerializeField] private Gradient gradient;

    [SerializeField] private float noise1_scale;
    [SerializeField] private float noise1_amp;

    Mesh mesh;
    MeshCollider collider;
    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colors;

    private float Y_min, Y_max;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();

        GetComponent<MeshFilter>().mesh = mesh;
        collider = GetComponent<MeshCollider>();
        

        CreateMash();
        UpdateMash();
    }

    void CreateMash()
    {
        // Initialize mesh parameters
        vertices = new Vector3[(board_size.x + 1) * (board_size.y + 1)];
        triangles = new int[(board_size.x * board_size.y * 6)];
        colors = new Color[vertices.Length];


        // Generate Mesh
        Vector3 pos = Vector3.zero;
        for ( int i = 0, tris = 0; pos.z <= board_size.y; pos.z++ )
        {
            for ( pos.x = 0; pos.x <= board_size.x; pos.x++ )
            {
                if ( pos.x < board_size.x && pos.z < board_size.y )
                {
                    //First triangle
                    triangles[tris] = i;
                    triangles[tris + 1] = i + board_size.x + 1;
                    triangles[tris + 2] = i + 1;
                    //second triangle
                    triangles[tris + 3] = i + 1;
                    triangles[tris + 4] = i + board_size.x + 1;
                    triangles[tris + 5] = i + board_size.x + 2;
                    tris += 6;
                }
                // Generate noise
                pos.y = Mathf.PerlinNoise(pos.x * noise1_scale, pos.z * noise1_scale) * noise1_amp;

                if ( Y_min > pos.y )
                    Y_min = pos.y;
                if ( Y_max > pos.y )
                    Y_max = pos.y;

                vertices[i++] = pos;
            }
        }

        //Postproccessing
        for ( int i = 0; i < vertices.Length; i++ )
        {
            //Apply colors
            colors[i] = gradient.Evaluate(Mathf.InverseLerp(Y_min, Y_max, vertices[i].y));


        }
    }

    void UpdateMash()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        
        mesh.RecalculateNormals();

        //collider.convex = mesh;
        collider.sharedMesh = mesh;
    }

    public Vector2Int GetBoardSize()
    {
        return board_size;
    }

    //private void OnDrawGizmos()
    //{
    //    foreach(var vertex in vertices )
    //    {
    //        Gizmos.DrawSphere(vertex, .001f);
    //    }
    //}
}
