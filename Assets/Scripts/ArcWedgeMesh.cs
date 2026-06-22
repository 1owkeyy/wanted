using UnityEngine;

// Generates a flat, tapered wedge mesh: narrow at the origin (player), wide at the far end.
// Attach this to the ArcVisual GameObject - it builds its own MeshFilter/MeshRenderer mesh on Awake.
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ArcWedgeMesh : MonoBehaviour
{
    [Header("Wedge Shape")]
    [SerializeField] private float length = 2.5f;      // how far the wedge extends forward (+Z)
    [SerializeField] private float tipWidth = 0.05f;    // width at the player end (near 0 = sharp point)
    [SerializeField] private float baseWidth = 1.2f;    // width at the far end

    void Awake()
    {
        BuildMesh();
    }

    private void BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "ArcWedge";

        // Flat quad-like shape on the XZ plane (Y stays 0 so it reads flat from top-down).
        // Triangle layout: narrow tip at origin, wide base at +Z (length).
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-tipWidth * 0.5f, 0f, 0f),     // 0: near-left (tip)
            new Vector3(tipWidth * 0.5f, 0f, 0f),      // 1: near-right (tip)
            new Vector3(baseWidth * 0.5f, 0f, length), // 2: far-right (base)
            new Vector3(-baseWidth * 0.5f, 0f, length) // 3: far-left (base)
        };

        int[] triangles = new int[6]
        {
            0, 2, 1,
            0, 3, 2
        };

        Vector3[] normals = new Vector3[4]
        {
            Vector3.up, Vector3.up, Vector3.up, Vector3.up
        };

        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;

        GetComponent<MeshFilter>().mesh = mesh;
    }
}