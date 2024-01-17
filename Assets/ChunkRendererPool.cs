using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class ChunkRendererPool
{
    private ConcurrentQueue<ChunkRenderer>_pool;
    private readonly ChunkRenderer _prefab;
    private int _maxCapacity = 0;

    public ChunkRendererPool(ChunkRenderer prefab, int loadDistance)
    {
        _pool = new ConcurrentQueue<ChunkRenderer>();
        _prefab = prefab;

        for (int x = 0;x<=(loadDistance+2)*2;x++)
        {
            for (int y = 0;y<=(loadDistance+2)*2;y++)
            {
                if (new Vector2Int(x,y).magnitude<=loadDistance)
                {
                    ChunkRenderer chunkRenderer = UnityEngine.Object.Instantiate(_prefab);
                    chunkRenderer.gameObject.SetActive(false);
                    _pool.Enqueue(chunkRenderer);
                    _maxCapacity++;
                }
            }
        }


    }

    public ChunkRenderer Get()
    {
        ChunkRenderer chunkRenderer;

        if (_pool.TryDequeue(out chunkRenderer))
        {
            chunkRenderer.gameObject.SetActive(true);
        }
        else
        {
            chunkRenderer = UnityEngine.Object.Instantiate(_prefab);
        }

        
        return chunkRenderer;
    }

    public void Return(ChunkRenderer chunkRenderer)
    {
        if (_pool.Count>=_maxCapacity)
        {
            UnityEngine.Object.Destroy(chunkRenderer.gameObject);
        }
        else
        {
            chunkRenderer.gameObject.SetActive(false);
            _pool.Enqueue(chunkRenderer);
        }
    }
}
