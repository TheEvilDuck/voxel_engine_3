using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

public class World
{
    public static readonly Vector2Int[]chunkDirections = {Vector2Int.down, Vector2Int.right,Vector2Int.up,Vector2Int.left};
    private Dictionary<Vector2Int, ChunkData>_currentChunks;
    private Dictionary<Vector2Int, ChunkRenderer>_currentChunkRenderers;
    private ConcurrentQueue<(Vector2Int, MeshData)>_renderQueue;
    private List<Vector2Int>_unrenderQueue;
    private ChunkRendererPool _chunkRendererPool;
    private SimpleNoiseTerrainGenerator _terrainGenerator;
    private readonly int _chunkWidth;
    private readonly int _chunkHeight;
    private readonly float _blockSize;
    private Random _random;
    private int _seed;

    public World(int chunkWidth, int chunkHeight, float blockSize, ChunkRendererPool chunkRendererPool, SimpleNoiseTerrainGenerator terrainGenerator)
    {
        _chunkWidth = chunkWidth;
        _chunkHeight = chunkHeight;
        _blockSize = blockSize;
        _chunkRendererPool = chunkRendererPool;
        _terrainGenerator = terrainGenerator;
        _random = new Random();

        _currentChunks = new Dictionary<Vector2Int, ChunkData>();
        _currentChunkRenderers = new Dictionary<Vector2Int, ChunkRenderer>();
        _renderQueue = new ConcurrentQueue<(Vector2Int, MeshData)>();
        _unrenderQueue = new List<Vector2Int>();

        _seed = _random.Next(1,2147483);
    }

    public async void LoadChunksAround(Vector2Int centerChunkPosition, int loadDistance)
    {
        ChunkData[] newChunks = await GenerateChunkDataAround(centerChunkPosition,loadDistance);

        List<ChunkData>chunksNeedToRemesh = new List<ChunkData>();

        foreach (ChunkData newChunk in newChunks)
        {
            if (newChunk==null)
                continue;

            foreach (Vector2Int direction in chunkDirections)
            {
                if (_currentChunks.TryGetValue(newChunk.ChunkPosition+direction, out ChunkData chunkData))
                {
                    if (!chunksNeedToRemesh.Contains(chunkData))
                    {
                        chunkData.state = ChunkState.MarkedToMeshing;
                        chunksNeedToRemesh.Add(chunkData);
                    }
                }
            }
        }

        foreach (KeyValuePair<Vector2Int,ChunkData>positionAndChunk in _currentChunks)
        {
            if (Vector2.Distance(positionAndChunk.Key,centerChunkPosition)>loadDistance)
            {
                _unrenderQueue.Add(positionAndChunk.Key);

                foreach (Vector2Int neighborDirection in chunkDirections)
                {
                    if (_currentChunks.TryGetValue(positionAndChunk.Key+neighborDirection, out ChunkData neighbor))
                    {
                        if (neighbor==null)
                            continue;

                        if (Vector2.Distance(neighbor.ChunkPosition,centerChunkPosition)<=loadDistance)
                            if (!chunksNeedToRemesh.Contains(neighbor))
                            {
                                neighbor.state = ChunkState.MarkedToMeshing;
                                chunksNeedToRemesh.Add(neighbor);
                            }  
                    }
                }

                if (positionAndChunk.Value!=null)
                    positionAndChunk.Value.state = ChunkState.MarkedToUnrender;
            }
        }

        foreach (ChunkData newChunk in newChunks)
        {
            if (newChunk!=null)
                _currentChunks.TryAdd(newChunk.ChunkPosition,newChunk);
        }

        chunksNeedToRemesh.AddRange(newChunks);
        
        (Vector2Int chunkPosition, MeshData meshData)[] renderDatas = await GenerateMeshDataFrom(chunksNeedToRemesh.ToArray());

        foreach (var renderData in renderDatas)
        {
            if (_currentChunks.TryGetValue(renderData.chunkPosition, out ChunkData chunkData))
            {
                if (chunkData==null)
                {
                    _unrenderQueue.Add(renderData.chunkPosition);
                    continue;
                }

                if (chunkData.state!=ChunkState.Meshing)
                    continue;

                chunkData.state = ChunkState.MarkedToRender;
                _renderQueue.Enqueue(renderData);
            }
        }
    }

    public async Task<ChunkData[]> GenerateChunkDataAround(Vector2Int centerChunkPosition, int loadDistance)
    {
        List<ChunkData>newChunks = new List<ChunkData>();

        await Task.Factory.StartNew(()=>
        {
            for (int x = centerChunkPosition.x-loadDistance;x<=centerChunkPosition.x+loadDistance;x++)
            {
                for (int y = centerChunkPosition.y-loadDistance;y<=centerChunkPosition.y+loadDistance;y++)
                {
                    Vector2Int chunkPosition = new Vector2Int(x,y);

                    if (Vector2.Distance(centerChunkPosition,chunkPosition)>loadDistance)
                        continue;

                    if (_currentChunks.ContainsKey(chunkPosition))
                        continue;

                    ChunkData chunkData = _terrainGenerator.GenerateChunkDataAt(chunkPosition,_seed);
                    chunkData.state = ChunkState.MarkedToMeshing;

                    foreach (Vector2Int direction in chunkDirections)
                    {
                        chunkData.TrySetNeighbor(direction, ()=>{
                            ChunkData neighbor;

                            _currentChunks.TryGetValue(chunkData.ChunkPosition+direction, out neighbor);

                            return neighbor;
                        });
                    }
                    
                    newChunks.Add(chunkData); 

                    //_currentChunks.TryAdd(chunkPosition,_terrainGenerator.GenerateChunkDataAt(chunkPosition+new Vector2Int(1,0)));
                }
            }
        });

        return newChunks.ToArray();
    }

    public async Task<(Vector2Int chunkPosition, MeshData meshData)[]> GenerateMeshDataFrom(ChunkData[] chunkDatas)
    {
        List<(Vector2Int chunkPosition, MeshData meshData)>renderDatas = new List<(Vector2Int chunkPosition, MeshData meshData)>();

        await Task.Factory.StartNew(()=>
        {
            foreach (ChunkData chunkData in chunkDatas)
            {
                if (chunkData==null)
                    continue;

                if (chunkData.state!=ChunkState.MarkedToMeshing)
                    continue;



                MeshData meshData = new MeshData(_chunkWidth,_chunkHeight,_blockSize, chunkData);
                (Vector2Int chunkPosition, MeshData meshData) renderData = new ();
                renderData.chunkPosition = chunkData.ChunkPosition;
                renderData.meshData = meshData;
                renderDatas.Add(renderData);
                chunkData.state = ChunkState.Meshing;
            }
        });

        return renderDatas.ToArray();
    }



    public void OnUpdate()
    {
        for (int i = 0;i<=_renderQueue.Count/10;i++)
        {
            if (_renderQueue.TryDequeue(out (Vector2Int chunkPosition, MeshData meshData) renderData))
            {
                if (!_currentChunks.TryGetValue(renderData.chunkPosition, out ChunkData chunkData))
                    continue;

                if (chunkData.state!=ChunkState.MarkedToRender)
                    continue;

                ChunkRenderer chunkRenderer;

                if (!_currentChunkRenderers.TryGetValue(renderData.chunkPosition, out chunkRenderer))
                    chunkRenderer = _chunkRendererPool.Get();

                chunkRenderer.Render(renderData.meshData);
                chunkRenderer.transform.position = new Vector3(renderData.chunkPosition.x*_chunkWidth*_blockSize,0,renderData.chunkPosition.y*_chunkWidth*_blockSize);

                _currentChunkRenderers.TryAdd(renderData.chunkPosition,chunkRenderer);
                chunkData.state = ChunkState.Rendered;
            }
            else
                break;
        }

        while (_unrenderQueue.Count>0)
        {
            Vector2Int chunkPosition = _unrenderQueue[0];
            _unrenderQueue.RemoveAt(0);

            _currentChunks.TryGetValue(chunkPosition, out ChunkData chunkData);

            if (_currentChunkRenderers.TryGetValue(chunkPosition,out ChunkRenderer chunkRenderer))
            {
                if (chunkData!=null)
                {
                    if (chunkData.state!=ChunkState.MarkedToUnrender)
                        continue;
                }

                _currentChunks.Remove(chunkPosition);

                _currentChunkRenderers.Remove(chunkPosition);
                _chunkRendererPool.Return(chunkRenderer);
            }
        }
    }

    public Vector2Int WorldPositionToChunkPosition(Vector3 worldPosition)
    {
        return new Vector2Int((int)worldPosition.x/_chunkWidth,(int)worldPosition.z/_chunkWidth);
    }
}
