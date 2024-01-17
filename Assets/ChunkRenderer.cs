using UnityEngine;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer), typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;
    private void Awake() 
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
    }

    public void Render(MeshData meshData)
    {
        Mesh mesh = new Mesh
        {
            vertices = meshData.Verticies,
            triangles = meshData.Triangles,
            uv = meshData.Uvs
        };

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        _meshCollider.sharedMesh = mesh;
        _meshFilter.mesh = mesh;
    }
    public void Unrender()
    {
        _meshCollider.sharedMesh.Clear();
        _meshFilter.mesh.Clear();
    }
}
