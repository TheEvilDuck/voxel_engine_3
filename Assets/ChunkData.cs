using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkData
{
    public ChunkState state;
    private readonly Vector2Int _chunkPosition;
    private Dictionary<Vector2Int, Func<ChunkData>>_neighbors;
    private BlockType[,,]_blocks;
    public Vector2Int ChunkPosition => _chunkPosition;

    public ChunkData(int chunkWidth, int chunkHeight, Vector2Int chunkPosition)
    {
        _blocks = new BlockType[chunkWidth,chunkHeight,chunkWidth];
        _chunkPosition = chunkPosition;
        _neighbors = new Dictionary<Vector2Int, Func<ChunkData>>();
    }

    public bool TrySetNeighbor(Vector2Int direction, Func<ChunkData>getChunkDataFunc)
    {
        return _neighbors.TryAdd(direction, getChunkDataFunc);
    }

    public ChunkData GetNeighbor(Vector2Int direction)
    {
        if (_neighbors.TryGetValue(direction, out Func<ChunkData> getChunkDataFunc))
        {
            if (getChunkDataFunc== null)
                return null;

            return getChunkDataFunc.Invoke();
        }

        return null;
    }

    public bool ModifyBlockAt(Vector3Int blockPosition, BlockType newBlockType)
    {
        if (!ValidateBlockPosition(blockPosition))
            return false;

        if (_blocks[blockPosition.x,blockPosition.y,blockPosition.z]==newBlockType)
            return false;

        _blocks[blockPosition.x,blockPosition.y,blockPosition.z]=newBlockType;
        return true;
    }

    public BlockType GetBlockAtPosition(Vector3Int blockPosition)
    {
        if (blockPosition.x<0&&_neighbors.TryGetValue(Vector2Int.left, out Func<ChunkData> getLeftNeighbor))
        {
            ChunkData leftNeighbor = getLeftNeighbor?.Invoke();

            if (leftNeighbor==null)
                return BlockType.Air;

            if (leftNeighbor.state== ChunkState.MarkedToUnrender)
                return BlockType.Air;

            return leftNeighbor.GetBlockAtPosition(new Vector3Int(_blocks.GetLength(0)+blockPosition.x,blockPosition.y,blockPosition.z));
        }

        if (blockPosition.x>=_blocks.GetLength(0)&&_neighbors.TryGetValue(Vector2Int.right, out Func<ChunkData> getRightNeighbor))
        {
            ChunkData rightNeighbor = getRightNeighbor?.Invoke();

            if (rightNeighbor==null)
                return BlockType.Air;
            
            if (rightNeighbor.state== ChunkState.MarkedToUnrender)
                return BlockType.Air;

            return rightNeighbor.GetBlockAtPosition(new Vector3Int(blockPosition.x-_blocks.GetLength(0),blockPosition.y,blockPosition.z));
        }

        if (blockPosition.z<0&&_neighbors.TryGetValue(Vector2Int.down, out Func<ChunkData> getDownNeighbor))
        {
            ChunkData downNeighbor = getDownNeighbor?.Invoke();

            if (downNeighbor==null)
                return BlockType.Air;

            if (downNeighbor.state== ChunkState.MarkedToUnrender)
                return BlockType.Air;

            return downNeighbor.GetBlockAtPosition(new Vector3Int(blockPosition.x,blockPosition.y,blockPosition.z+_blocks.GetLength(1)));
        }

        if (blockPosition.z>=_blocks.GetLength(1)&&_neighbors.TryGetValue(Vector2Int.up, out Func<ChunkData> getUpNeighbor))
        {
            ChunkData upNeighbor = getUpNeighbor?.Invoke();

            if (upNeighbor==null)
                return BlockType.Air;

            if (upNeighbor.state== ChunkState.MarkedToUnrender)
                return BlockType.Air;

            return upNeighbor.GetBlockAtPosition(new Vector3Int(blockPosition.x,blockPosition.y,blockPosition.z-_blocks.GetLength(1)));
        }

        if (!ValidateBlockPosition(blockPosition))
            return BlockType.Air;

        return _blocks[blockPosition.x,blockPosition.y,blockPosition.z];
    }

    private bool ValidateBlockPosition(Vector3Int blockPosition)
    {
        return 
            blockPosition.x>=0&&
            blockPosition.y>=0&&
            blockPosition.z>=0&&
            blockPosition.x<_blocks.GetLength(0)&&
            blockPosition.y<_blocks.GetLength(1)&&
            blockPosition.z<_blocks.GetLength(2);
    }
}
