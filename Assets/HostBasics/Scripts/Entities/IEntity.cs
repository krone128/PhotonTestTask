using System;
using UnityEngine;

namespace HostBasics.Scripts.Entities
{
    public interface IEntity
    {
        public event Action<IEntity, Vector3> OnDestinationUpdated;
        
        public short Id { get; }
        public bool Authoritative { get; }
        public bool IsDirty { get; }
        public bool IsChunkDirty { get; }
        
        public Vector3 Position { get; set;}
        public Vector3 Destination { get; set; }

        void Init();
        void Init(short id);
        void SelectNewDestination();
        void StartMovement();
        void ResetDirty();
        void SetDirty();
        void ResetChunkDirty();
        void SetChunkDirty();
    }
}