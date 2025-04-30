using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HostBasics.Scripts.Entities
{
    public partial class Entity : IEntity
    {
        public static event Action<short> OnClick;

        public short Id
        {
            get => _id;
            private set
            {
                _id = value;
                gameObject.name = $"Entity {Id}";
            }
        }

        private bool Authoritative { get; set; }
        public bool IsDirty { get; private set; }
        public bool IsChunkDirty { get; private set; }

        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }
        public Vector3 Destination { get; set; }
        public int LastUpdateTick { get; set; }
        
        public bool IsMoving
        {
            get => _isMoving;
            private set
            {
                _isMoving = value;
                meshRenderer.material = value ? GreenMaterial : RedMaterial;
            }
        }

        public void Init()
        {
            Id = ++ENTITY_ID_POOL;
            Authoritative = true;
            SetDirty();
        }

        public void Init(short id)
        {
            Id = id;
            Authoritative = false;
            SetDirty();
        }
                
        public void SetActive(bool isActive) => gameObject.SetActive(isActive);
    
        public void SelectNewDestination()
        {
            if(!Authoritative) return;

            var destination =
                new Vector3(Random.Range(0, GameConfig.GridSize.x),
                    0f,
                    Random.Range(0, GameConfig.GridSize.y));
            
            Destination = destination;
            StartMovement();
            IsDirty = true;
        }
        
        public void StartMovement(bool compensate = false)
        {
            TargetDirection = Destination - transform.position;
            TargetDirection = TargetDirection.normalized;
            transform.LookAt(Destination);
            IsMoving = true;

            if (compensate)
            {
                transform.position += TargetDirection * (Speed * 0.1f);
            }
        }

        public void SetDirty() => IsDirty = true;
        public void ResetDirty() => IsDirty = false;
        
        public void SetChunkDirty() => IsChunkDirty = true;
        public void ResetChunkDirty() => IsChunkDirty = false;


    }
}