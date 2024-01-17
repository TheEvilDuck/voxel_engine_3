using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleNoiseTerrainGenerator
{
    private int _chunkWidth;
    private int _chunkHeight;

    public SimpleNoiseTerrainGenerator(int chunkWidth, int chunkHeight)
    {
        _chunkHeight = chunkHeight;
        _chunkWidth = chunkWidth;
    }
    public ChunkData GenerateChunkDataAt(Vector2Int chunkPosition, int seed)
    {
        ChunkData chunkData = new ChunkData(_chunkWidth, _chunkHeight, chunkPosition);

        for (int x = 0;x< _chunkWidth;x++)
            for (int z = 0; z< _chunkWidth; z++)
            {
                float noise = Mathf.PerlinNoise((chunkPosition.x*_chunkWidth+x)/100f,(chunkPosition.y*_chunkWidth+z)/100f);
                
                int height = Mathf.RoundToInt((float)_chunkHeight*noise);
                //Debug.Log(height);
                for (int y =0; y<height;y++)
                    chunkData.ModifyBlockAt(new Vector3Int(x,y,z), BlockType.Dirt);
            }

        return chunkData;
    }
}
