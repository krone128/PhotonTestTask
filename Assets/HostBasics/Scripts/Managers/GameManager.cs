using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Fusion;
using HostBasics.Scripts.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace HostBasics.Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private EntityManager _entityManager;
        private InterestManager _interestManager;
        
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
            NetworkUpdateProxy.OnFixedUpdateNetwork += OnFixedUpdateNetwork;
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
        
        public void SendEntityUpdateToClients()
        {
            var entities = _interestManager.GetUpdatedEntities();

            if (entities.Count > 0)
            {
                _basicSpawner.SendReliableDataToPlayer(entities);
            }
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

            if (playerTransform)
            {
                _entityManager.UpdateNotInterested(playerTransform.position);
            }
            
            foreach (var e in span)
            {
                _entityManager.UpdateEntityClient(e.Id, e.Position, e.Destination);
            }
        }
        
        GameObject[,] TileIndicators = new GameObject[GameConfig.GridChunks.x, GameConfig.GridChunks.y];

        public GameObject _debugTile;
        
        private void SpawnDebugTiles()
        {
            var halfChunkSize = GameConfig.ChunkSize / 2f;
            
            var root = new GameObject();
            
            for (int i = 0; i < GameConfig.GridChunks.x; i++)
            {
                for (int j = 0; j < GameConfig.GridChunks.y; j++)
                {
                    var tile = Instantiate(_debugTile,
                        new Vector3(i * GameConfig.ChunkSize + halfChunkSize, 
                            -1,
                            j * GameConfig.ChunkSize + halfChunkSize), Quaternion.identity, root.transform);
                    tile.transform.localScale = new Vector3(GameConfig.ChunkSize, 0.05f, GameConfig.ChunkSize) * 0.96f;
                    TileIndicators[i, j] = tile;
                    tile.GetComponentInChildren<TMP_Text>().text = $"{i} . {j}";
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

        private void OnFixedUpdateNetwork()
        {
            SendEntityUpdateToClients();
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