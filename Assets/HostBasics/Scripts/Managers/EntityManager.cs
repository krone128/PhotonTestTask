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

        private IObjectPool<IEntity> _creaturePool;
        
        private Dictionary<short, IEntity> _entities  = new();
        private NetworkRunner _runner;

        public IEnumerable<IEntity> Entities => _entities.Values;

        public void Init(NetworkRunner runner)
        {
            _runner = runner;
            _creaturePool = new ObjectPool<IEntity>(() => Instantiate(_entityPrefab, _entityParent), ActionOnGet, ActionOnRelease);
        }

        private void ActionOnGet(IEntity obj)
        {
            obj.SetActive(true);
        }
        
        private void ActionOnRelease(IEntity obj)
        {
            obj.SetActive(false);
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
            
            entity.Position = position;
            entity.Destination = destination;
            entity.StartMovement(_runner.Tick.Raw);
        }
        // Handle entities moving out of interest zone on client
        public void UpdateNotInterested(Vector3 position)
        {
            var chunk = InterestManager.GetChunk(position);
            
            var entities = _entities.Values.ToArray();
            
            foreach (var e in entities)
            {
                if(InterestManager.IsInRadiusChunks(e.Position, chunk, GameConfig.InterestRadius) || (_runner.Tick.Raw - e.LastUpdateTick) < GameConfig.EntityPoolTimeout) continue;

                PoolEntity(e);
            }
        }
        
        private void PoolEntity(IEntity entityView)
        {
            entityView.SetActive(false);
            _entities.Remove(entityView.Id);
            _creaturePool.Release(entityView);
        }

        // Creates Fully inited object
        private IEntity SpawnEntity()
        {
            var entity = _creaturePool.Get();
            return entity;
        }

        public void Clear()
        {
            foreach (var entity in _entities.Values.ToArray())
            {
                PoolEntity(entity);
                _entities.Clear();
            }
        }

        public IEntity GetEntity(short id)
        {
            return _entities.GetValueOrDefault(id);
        }

        public void UpdateEntityMovement(float networkRunnerDeltaTime)
        {
            foreach (var entity in _entities.Values.ToArray())
            {
                entity.UpdateMovement(networkRunnerDeltaTime);
            }
        }
    }
}