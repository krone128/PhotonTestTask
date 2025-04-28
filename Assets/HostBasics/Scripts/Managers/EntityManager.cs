using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using HostBasics.Scripts.Entities;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace HostBasics.Scripts
{
    
    public class EntityManager : MonoBehaviour
    {
        [SerializeField] Transform _entityParent;
        [SerializeField] private Entity _entityPrefab;

        private IObjectPool<Entity> _creaturePool;
        
        private Dictionary<int, Entity> _entities  = new();
        private NetworkRunner _runner;

        public IEnumerable<IEntity> Entities => _entities.Values;

        public void Init(NetworkRunner runner)
        {
            _runner = runner;
            _creaturePool = new ObjectPool<Entity>(() => Instantiate(_entityPrefab, _entityParent), ActionOnGet, ActionOnRelease);
        }

        private void ActionOnGet(Entity obj)
        {
            obj.gameObject.SetActive(true);
        }
        
        private void ActionOnRelease(Entity obj)
        {
            obj.gameObject.SetActive(false);
        }

        public void SpawnRandomBatched(int count, Vector2 positionRange)
        {
            for (int i = 0; i < count; i++)
            {
                var position = new Vector3(Random.Range(0, positionRange.x), 0f, Random.Range(0, positionRange.y));
                var creature = SpawnEntity();
                creature.Init();
                creature.Position = position;
                creature.SelectNewDestination();
                _entities.Add(creature.Id, creature);
            }
        }
        
        public void SpawnGrid(int count, Vector2 positionRange)
        {
            var halfChunkSize = GameConfig.ChunkSize / 2f;
            
            for (int i = 0; i < GameConfig.GridChunks.x; i++)
            {
                for (int j = 0; j < GameConfig.GridChunks.y; j++)
                {
                    var pos =
                        new Vector3(
                            i * GameConfig.ChunkSize + halfChunkSize,
                            0,
                            j * GameConfig.ChunkSize + halfChunkSize);
                    
                    var creature = SpawnEntity();
                    creature.Init();
                    creature.Position = pos;
                    _entities.Add(creature.Id, creature);
                }
            }
        }

        public void UpdateEntityClient(short id, Vector3 position, Vector3 destination)
        {
            if (!_entities.TryGetValue(id, out var entity))
            {
                entity = SpawnEntity();
                entity.Init(id);
                _entities.Add(id, entity);
            }
            
            entity.LastUpdateTick = _runner.Tick.Raw;
            
            entity.Position = position;
            entity.Destination = destination;
            entity.StartMovement();
        }
        // Handle entities moving out of interest zone on client
        public void UpdateNotInterested(Vector3 position)
        {
            var chunk = InterestManager.GetChunk(position);
            
            var entities = _entities.Values.ToArray();
            
            foreach (var e in entities)
            {
                if(InterestManager.IsInRadiusChunks(e.Position, chunk, GameConfig.InterestRadius) || (_runner.Tick.Raw - e.LastUpdateTick < 5)) continue;

                PoolEntity(e);
            }
        }
        
        private void PoolEntity(Entity entityView)
        {
            entityView.gameObject.SetActive(false);
            _entities.Remove(entityView.Id);
            _creaturePool.Release(entityView);
        }

        // Creates Fully inited object
        public Entity SpawnEntity()
        {
            var entity = _creaturePool.Get();
            return entity;
        }

        public void Clear()
        {
            foreach (var entity in _entities.Values.ToArray())
            {
                PoolEntity(entity);
            }
        }
    }
}