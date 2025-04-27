using UnityEngine;

namespace HostBasics.Scripts.Entities
{
    public interface IEntity
    {
        public short Id { get; }
        public bool IsDirty { get; }
        public bool IsChunkDirty { get; }
        
        public Vector3 Position { get; set;}
        public Vector3 Destination { get; set; }

        void Init();
        void Init(short id);
        void StartMovement();
        void ResetDirty();
        void SetDirty();
        void ResetChunkDirty();
        void SetChunkDirty();
    }
}