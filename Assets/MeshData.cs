using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    private List<int> _triangles;
    private List<Vector3> _verticies;
    private List<Vector2> _uvs;
    public int[] Triangles => _triangles.ToArray();
    public Vector3[] Verticies => _verticies.ToArray();
    public Vector2[] Uvs => _uvs.ToArray();

    private static readonly Dictionary<Vector3Int, Vector3[]> _verticiesInBlockSide = new Dictionary<Vector3Int, Vector3[]>()
    {
        { 
            Vector3Int.left, new Vector3[] 
            {
                new Vector3(0,0,0),
                new Vector3(0,0,1),
                new Vector3(0,1,0),
                new Vector3(0,1,1)
            }
        },
        {
            Vector3Int.right, new Vector3[]
            {
                new Vector3(1,0,0),
                new Vector3(1,1,0),
                new Vector3(1,0,1),
                new Vector3(1,1,1) 
            }
        },
        {
            Vector3Int.up, new Vector3[]
            {
                new Vector3(0,1,0),
                new Vector3(0,1,1),
                new Vector3(1,1,0),
                new Vector3(1,1,1) 
            }
        },
        {
            Vector3Int.down, new Vector3[]
            {
                new Vector3(0,0,0),
                new Vector3(1,0,0),
                new Vector3(0,0,1),
                new Vector3(1,0,1) 
            }
        },
        {
            Vector3Int.back, new Vector3[]
            {
                new Vector3(0,0,0),
                new Vector3(0,1,0),
                new Vector3(1,0,0),
                new Vector3(1,1,0) 
            }
        },
        {
            Vector3Int.forward, new Vector3[]
            {
                new Vector3(0,0,1),
                new Vector3(1,0,1),
                new Vector3(0,1,1),
                new Vector3(1,1,1) 
            }
        }
    };
    public MeshData(int chunkWidth, int chunkHeight, float blockSize, ChunkData chunkData)
    {
        _triangles = new List<int>();
        _verticies = new List<Vector3>();
        _uvs = new List<Vector2>();

        for (int x = 0;x< chunkWidth;x++)
            for (int y = 0; y< chunkHeight; y++)
                for (int z = 0; z< chunkWidth; z++)
                    if (chunkData.GetBlockAtPosition(new Vector3Int(x,y,z))!=BlockType.Air)
                        GenerateDataAt(new Vector3Int(x,y,z), blockSize, chunkData);

    }

    private void GenerateDataAt(Vector3Int blockPos,float blockSize, ChunkData chunkData)
    {
        foreach (KeyValuePair<Vector3Int,Vector3[]> verticiesInBlockSide in _verticiesInBlockSide)
        {
            if ((blockPos+verticiesInBlockSide.Key).y<0)
                continue;

            if (chunkData.GetBlockAtPosition(blockPos+verticiesInBlockSide.Key)==BlockType.Air)
            {
                foreach (Vector3 vertice in verticiesInBlockSide.Value)
                    _verticies.Add(blockPos+vertice);

                AddTriangles();
            }
                
        }
    }

    private void AddTriangles()
    {
        
        _triangles.Add(_verticies.Count-4);
        _triangles.Add(_verticies.Count-3);
        _triangles.Add(_verticies.Count-2);

        _triangles.Add(_verticies.Count-3);
        _triangles.Add(_verticies.Count-1);
        _triangles.Add(_verticies.Count-2);
    }
}
