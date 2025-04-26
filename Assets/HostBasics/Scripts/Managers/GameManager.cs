using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Fusion;
using HostBasics.Scripts.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace HostBasics.Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private EntityManager _entityManager;
        private IInterestManager _interestManager;
        
        BasicSpawner _basicSpawner;
        public Transform playerTransform;

        private void Awake()
        {
            SpawnDebugTiles();
        }

        public void InitializeEntities(BasicSpawner spawner, bool runnerIsServer)
        {
            _basicSpawner = spawner;
            _entityManager.Init();
            
            if(!runnerIsServer) return;
            
            _entityManager.SpawnRandomBatched(GameConfig.EntityCount, GameConfig.GridSize);
            SetupInterestManagement();
        }

        private void SetupInterestManagement()
        {
            _interestManager = new InterestManager();
            
            foreach (var e in _entityManager.Entities)
            {
                _interestManager.RegisterEntity(e);
            }
        }
        
        private bool _updateLock = false;
        
        public void SendEntityUpdateToClients()
        {
            // if (_updateLock) return;
            
            _updateLock = true;
            
            var entities = _interestManager.GetUpdatedEntities();

            if (entities.Count > 0)
            {
                _basicSpawner.SendReliableDataToPlayer(entities);
            }

            _updateLock = false;
        }

        public void JoinSession()
        {

        }

        public void StopSession()
        {
            
        }

        public void ProcessReliableData(ArraySegment<byte> data)
        {
            var span = MemoryMarshal.Cast<byte, EntityUpdateMessage>(data);
            
            if(playerTransform) _entityManager.UpdateNotInterested(playerTransform.position);
            
            foreach (var e in span)
                _entityManager.UpdateEntityClient(e.Id, e.Position, e.Destination);
        }
        
        GameObject[,] TileIndicators = new GameObject[GameConfig.GridChunks.x, GameConfig.GridChunks.y];

        public GameObject _debugTile;
        
        private void SpawnDebugTiles()
        {
            var halfChunkSize = GameConfig.ChunkSize / 2f;
            
            for (int i = 0; i < GameConfig.GridChunks.x; i++)
            {
                for (int j = 0; j < GameConfig.GridChunks.y; j++)
                {
                    TileIndicators[i, j] = Instantiate(_debugTile,
                        new Vector3(i * GameConfig.ChunkSize + halfChunkSize, 
                            -1,
                            j * GameConfig.ChunkSize + halfChunkSize), Quaternion.identity);
                    
                    TileIndicators[i, j].transform.localScale = new Vector3(GameConfig.ChunkSize, 0.05f, GameConfig.ChunkSize) * 0.95f;
                }
            }
        }
        
        private void Update()
        {
            if(!playerTransform) return;
            
            for (int i = 0; i < GameConfig.GridChunks.x; i++)
            {
                for (int j = 0; j < GameConfig.GridChunks.y; j++)
                {
                    TileIndicators[i, j].gameObject.SetActive(
                        InterestManager.IsInRadiusChunks(playerTransform.position, new Vector2Int(i, j), GameConfig.InterestRadius));
                }
            }
        }

        public void PlayerJoined(PlayerRef player, Transform transform1)
        {
            _interestManager.RegisterPlayer(player, transform1);
        }

        public void PlayerLeft(PlayerRef player)
        {
            _interestManager.UnregisterPlayer(player);
        }
    }
}