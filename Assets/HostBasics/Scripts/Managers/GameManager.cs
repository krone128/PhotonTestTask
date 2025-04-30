using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ExitGames.Client.Photon;
using Fusion;
using Fusion.Sockets;
using HostBasics.Scripts.Entities;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HostBasics.Scripts
{
    public class GameManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        private Dictionary<PlayerRef, (long, long)> TotalMsgBytesSent = new();
        private (long, long) TotalMsgBytesReceived = (0, 0);
        
        [SerializeField] private EntityManager _entityManager;
        private InterestManager _interestManager;
        
        public Transform playerTransform;
        
        private NetworkRunner _networkRunner;

        private void Awake()
        {
            SpawnDebugTiles();
            NetworkUpdateProxy.OnEntityPause += OnEntityPause;
        }

        public void Init(NetworkRunner runner, BasicSpawner basicSpawner)
        {
            _basicSpawner = basicSpawner;
            _networkRunner = runner;
            _entityManager.Init(runner);
            _networkRunner.AddCallbacks(this);
        }
        
        public void SetupWorld()
        {
            _entityManager.SpawnRandomBatched(GameConfig.EntityCount, GameConfig.GridSize);
            SetupInterestManagement();
            
            NetworkUpdateProxy.OnFixedUpdateNetwork += OnFixedUpdateNetwork;
        }

        private void OnEntityPause(short id)
        {
            if (_networkRunner.IsServer)
            {
                var entity = _entityManager.GetEntity(id);
                _basicSpawner.TeleportAll(entity.Position);
                SendEntityUpdateToClients();
            }
            
            PrintDebugEntityId(id);
        }
        
        public void PrintDebugEntityId(short id)
        {
            GameObject.Find("EntityDebugText").GetComponent<TMP_Text>().text = $"Entity {id}";
        }

        private void SetupInterestManagement()
        {
            _interestManager = new InterestManager();
            
            foreach (var e in _entityManager.Entities)
            {
                _interestManager.RegisterEntity(e);
            }
        }
        
        StringBuilder sb = new StringBuilder();
        StringBuilder uiSb = new StringBuilder();
        
        public ReliableKey EntityUpdateEvent = ReliableKey.FromInts(0,0,0,777);
        public ReliableKey PlayerChunkUpdateEvent = ReliableKey.FromInts(0,0,0,778);

        public void SendEntityUpdateToClients()
        {
            var mappedEntities = _interestManager.GetUpdatedEntities();

            if (mappedEntities.Count == 0)
            {
                return;
            }

            sb.Clear();
            uiSb.Clear();

            foreach (var playerRef in _networkRunner.ActivePlayers)
            {
                if (playerRef.PlayerId <= 1) continue;

                if (mappedEntities.TryGetValue(playerRef, out var mapping) && mapping.Any())
                {
                    var filteredMapping = mapping
                        .Select(e => new EntityUpdateMessage(e.Id, e.Position, e.Destination)).ToList();
                    
                    filteredMapping.Insert(0, new EntityUpdateMessage
                    {
                        Id = -1,
                        Position = _interestManager.GetPlayerPosition(playerRef),
                        Destination = default
                    });

                    var bytes = MemoryMarshal.Cast<EntityUpdateMessage, byte>(filteredMapping.ToArray());

                    sb.Append(
                        $"Sending batched to player [{playerRef.PlayerId}]:\n{filteredMapping.Count} entities, {bytes.Length} bytes");

                    var stats = TotalMsgBytesSent[playerRef];
                    stats.Item1++;
                    stats.Item2 += bytes.Length;

                    uiSb.AppendLine(
                        $"Clint [{playerRef.PlayerId}]:\n{TotalMsgBytesSent[playerRef].Item1} messages\n{TotalMsgBytesSent[playerRef].Item2} bytes");

                    TotalMsgBytesSent[playerRef] = stats;
                    _networkRunner.SendReliableDataToPlayer(playerRef, EntityUpdateEvent, bytes.ToArray());
                }

                if (sb.Length > 0)
                {
                    Debug.Log(sb.ToString());
                }
                else
                {
                    Debug.Log("No network updates sent at this frame");
                }

                if (uiSb.Length > 0)
                {
                    GameObject.Find("PlayerNetworkDataText").GetComponent<TMP_Text>().text =
                        uiSb.ToString();
                }
            }
        }

        private const int BatchSize = 50;

        public void ProcessReliableData(ArraySegment<byte> data)
        {
            TotalMsgBytesReceived.Item1++;
            TotalMsgBytesReceived.Item2+=data.Count;
            
            
            GameObject.Find("PlayerNetworkDataText").GetComponent<TMP_Text>().text =
                $"Client [{_networkRunner.LocalPlayer.PlayerId}] received:\n{TotalMsgBytesReceived.Item1} messages\n{TotalMsgBytesReceived.Item2} bytes\n";
            
            var span = MemoryMarshal.Cast<byte, EntityUpdateMessage>(data);

            if (playerTransform)
            {
                _entityManager.UpdateNotInterested(span[0].Position);
            }
            
            Debug.Log($"Received update message:\n{span.Length} entities, {data.Count} bytes");

            foreach (var e in span.ToArray().Skip(1))
            {
                _entityManager.UpdateEntityClient(e.Id, e.Position, e.Destination);
            }
        }
        
        GameObject[,] TileIndicators = new GameObject[GameConfig.GridChunks.x, GameConfig.GridChunks.y];

        public GameObject _debugTile;
        private BasicSpawner _basicSpawner;

        private void OnFixedUpdateNetwork()
        {
            SendEntityUpdateToClients();
        }
        
        public void PlayerJoined(PlayerRef player, Transform transform1)
        {
            TotalMsgBytesSent.Add(player, (0,0));
            _interestManager.RegisterPlayer(player, transform1);
        }

        public void PlayerLeft(PlayerRef player)
        {
            TotalMsgBytesSent.Remove(player);
            _interestManager.UnregisterPlayer(player);
        }

        #region Debug
        
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
        
        

        #endregion
        
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
            if(key.Equals(EntityUpdateEvent)) 
                ProcessReliableData(data);
        }


        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
   
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            _entityManager.Clear();
            NetworkUpdateProxy.OnFixedUpdateNetwork -= OnFixedUpdateNetwork;
            NetworkUpdateProxy.OnEntityPause -= OnEntityPause;
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {

        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
            
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
            
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            
        }
    }
}