using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField]private ChunkRenderer _chunkRendererPrefab;
    [SerializeField]private Character _playerCharacterPrefab;
    [SerializeField]private int _chunkWidth = 20;
    [SerializeField]private int _chunkHeight = 60;
    [SerializeField]private float _blockSize = 1f;
    [SerializeField]private int _loadDistance = 10;

    private World _world;
    private PlayerInput _playerInput;
    private Character _playerCharacter;
    private InputCharacterMediator _inputCharacterMediator;
    private Vector2Int _prevCharacterChunk;
    private ChunkRendererPool _chunkRendererPool;
    private SimpleNoiseTerrainGenerator _terrainGenerator;

    private void Start() 
    {
        _chunkRendererPool = new ChunkRendererPool(_chunkRendererPrefab, _loadDistance+2);
        _terrainGenerator = new SimpleNoiseTerrainGenerator(_chunkWidth,_chunkHeight);
        _world = new World(_chunkWidth,_chunkHeight,_blockSize,_chunkRendererPool,_terrainGenerator);

        _playerInput = new PlayerInput();
        _playerCharacter = Instantiate(_playerCharacterPrefab);
        _inputCharacterMediator = new InputCharacterMediator(_playerInput, _playerCharacter);

        _world.LoadChunksAround(new Vector2Int(0,0), _loadDistance);
        _prevCharacterChunk = _world.WorldPositionToChunkPosition(_playerCharacter.transform.position);
    }

    private void Update() 
    {
        _playerInput.Update();
        
        Vector2Int characterChunk = _world.WorldPositionToChunkPosition(_playerCharacter.transform.position);

        if (characterChunk!=_prevCharacterChunk)
        {
            _world.LoadChunksAround(characterChunk, _loadDistance);
        }

        _prevCharacterChunk = characterChunk;

        _world.OnUpdate();
    }

    private void OnDestroy() 
    {
        _inputCharacterMediator.Dispose();
    }
}
