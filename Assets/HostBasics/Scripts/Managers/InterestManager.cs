using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fusion;
using HostBasics.Scripts.Entities;
using UnityEngine;

namespace HostBasics.Scripts
{
    public class InterestManager
    {
        List<IEntity> _entitiesList = new ();
        
        Dictionary<PlayerRef, Transform> _playerMap = new ();
        Dictionary<PlayerRef, List<IEntity>> _interestResults = new();
        Dictionary<PlayerRef, HashSet<Vector2Int>> _playerToDirtyChunkMap = new();
        Dictionary<PlayerRef, Vector2Int> _playerToChunkMap = new();
        
        Dictionary<int, Vector2Int> _entityToChunkMap = new();

        public void RegisterPlayer(PlayerRef player, Transform playerTransform)
        {
            _playerMap.Add(player, playerTransform);
            _playerToDirtyChunkMap[player] = new HashSet<Vector2Int>();
            _interestResults[player] = new List<IEntity>();
        }

        public void UnregisterPlayer(PlayerRef player)
        {
            _playerMap.Remove(player);
            _playerToDirtyChunkMap.Remove(player);
            _interestResults.Remove(player);
        }
        
        public void RegisterEntity(IEntity entity)
        {
            _entitiesList.Add(entity);
        }

        public void UnregisterEntity(IEntity entity)
        {
            _entitiesList.Remove(entity);
        }
        
        private void UpdatePlayerChunks()
        {
            foreach (var playerKvp in _playerMap)
            {
                var playerChunk = GetChunk(playerKvp.Value.position);
                _playerToDirtyChunkMap[playerKvp.Key].Clear();
                
                if (_playerToChunkMap.TryGetValue(playerKvp.Key, out var mappedChunk))
                {
                    if (mappedChunk == playerChunk)
                    {
                        continue;
                    }
                    
                    //chunkChanged, collect 
                    var chunkDelta = playerChunk - mappedChunk;
                    
                    //check
                    if (chunkDelta.x != 0 && playerChunk.x > 0 && playerChunk.x < GameConfig.GridChunks.x)
                    {
                        //if moved left/right and not at the edge
                        var colStart = Mathf.Max(0, playerChunk.y - GameConfig.InterestRadius);
                        var colEnd = Mathf.Min(GameConfig.GridChunks.y, playerChunk.y + GameConfig.InterestRadius);
                        
                        for (var i = colStart; i <= colEnd; i++)
                        {
                            var chunkToAdd = new Vector2Int(playerChunk.x + chunkDelta.x, i);
                        
                            _playerToDirtyChunkMap[playerKvp.Key].Add(chunkToAdd);
                        }
                    }
                    
                    if (chunkDelta.y != 0 && playerChunk.y > 0 && playerChunk.y < GameConfig.GridChunks.y)
                    {
                        //if moved up/down and not at the edge
                        var rowStart = Mathf.Max(0, playerChunk.x - GameConfig.InterestRadius);
                        var rowEnd = Mathf.Min(GameConfig.GridChunks.x, playerChunk.x + GameConfig.InterestRadius);
                        
                        for (var i = rowStart; i <= rowEnd; i++)
                        {
                            var chunkToAdd = new Vector2Int(i, playerChunk.y + chunkDelta.y);
                        
                            _playerToDirtyChunkMap[playerKvp.Key].Add(chunkToAdd);
                        }
                    }

                }
                else
                {
                    //player not registered in chunk, mark all his interest chunks as dirty

                    for (var i = Mathf.Max(0, playerChunk.x - GameConfig.InterestRadius); 
                         i <= Mathf.Min(GameConfig.GridChunks.x, playerChunk.x + GameConfig.InterestRadius);
                         i++)
                    {
                        for (var j = Mathf.Max(0, playerChunk.y - GameConfig.InterestRadius); 
                             j <= Mathf.Min(GameConfig.GridChunks.y, playerChunk.y + GameConfig.InterestRadius); 
                             j++)
                        {
                            _playerToDirtyChunkMap[playerKvp.Key].Add(new Vector2Int(i, j));
                        }
                    }
                }

                _playerToChunkMap[playerKvp.Key] = playerChunk;
            }
        }

        private void CheckEntityChangedChunks()
        {
            foreach (var entity in _entitiesList)
            {
                var chunk = GetChunk(entity.Position);

                if (_entityToChunkMap.TryGetValue(entity.Id, out var mappedChunk))
                {
                    if (mappedChunk == chunk) continue;
                }
                else
                {
                    _entityToChunkMap[entity.Id] = chunk;
                }
                entity.SetChunkDirty();
                //_entityToChunkMapping[entity.Id] = chunk;
            }
        }
        
        public IDictionary<PlayerRef, List<IEntity>> GetUpdatedEntities()
        {
            UpdatePlayerChunks();
            CheckEntityChangedChunks();
            
            foreach (var precacheValue in _interestResults.Values)
            {
                precacheValue.Clear();
            }
            
            foreach (var e in _entitiesList)
            {
                var lastEntityChunk = _entityToChunkMap[e.Id];
                var entityChunk = GetChunk(e.Position);
                _entityToChunkMap[e.Id] = entityChunk;
                
                foreach (var p in _playerMap)
                {
                    var playerChunk = _playerToChunkMap[p.Key];
                    var pcList = _interestResults[p.Key];
                    var dirtyChunksMap = _playerToDirtyChunkMap[p.Key];
                    var entityInRadius = IsInRadiusChunks(entityChunk, playerChunk, GameConfig.InterestRadius);
                    var lastEntityInRadius = IsInRadiusChunks(lastEntityChunk, playerChunk, GameConfig.InterestRadius);
                    
                    if (dirtyChunksMap.Contains(entityChunk)
                        || (entityInRadius && 
                            (e.IsDirty 
                             || (e.IsChunkDirty && !lastEntityInRadius))))
                    {
                        pcList.Add(e);
                    }
                }
                
                e.ResetDirty();
                e.ResetChunkDirty();
            }

            return _interestResults;
        }
        
        public static Vector2Int GetChunk(Vector3 position)
        {
            return new Vector2Int((int)(position.x / GameConfig.ChunkSize), (int)(position.z / GameConfig.ChunkSize));
        }

        public static bool IsInRadiusChunks(Vector3 position, Vector2Int chunk, int blockRadius)
        {
            var chunkDelta = GetChunk(position) - chunk;
                    
            return Mathf.Abs(chunkDelta.x) <= blockRadius && Mathf.Abs(chunkDelta.y) <= blockRadius;
        }

        private static bool IsInRadiusChunks(Vector2Int position, Vector2Int chunk, int blockRadius)
        {
            var chunkDelta = position - chunk;
                    
            return Mathf.Abs(chunkDelta.x) <= blockRadius && Mathf.Abs(chunkDelta.y) <= blockRadius;
        }
    }
}