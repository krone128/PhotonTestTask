using System;
using UnityEngine;

namespace HostBasics.Scripts.Entities
{
    public partial class Entity : IEntity
    {
        public event Action<IEntity, Vector3> OnDestinationUpdated;
        
        private static short ENTITY_ID_POOL = 0;
        private short _id;

        public float Speed = 10f;

        public bool IsChunkDirty { get; private set; }

        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }
        public Vector3 Destination { get; set; }

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

        public void ResetDirty()
        {
            IsDirty = false;
        }

        public void SetDirty()
        {
            IsDirty = true;
        }

        public void ResetChunkDirty()
        {
            IsChunkDirty = false;
        }

        public void SetChunkDirty()
        {
            IsChunkDirty = true;
        }

        public short Id
        {
            get => _id;
            private set
            {
                _id = value;
                gameObject.name = $"Entity {Id}";
            }
        }

        public bool Authoritative { get; private set; }

        public bool IsDirty { get; private set; }
    }
}