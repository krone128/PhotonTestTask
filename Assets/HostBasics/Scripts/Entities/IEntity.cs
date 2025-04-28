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
        
        public int LastUpdateTick { get; set; }

        void Init();
        void Init(short id);
        void StartMovement();
        
        void SetDirty();
        void ResetDirty();
        
        void SetChunkDirty();
        void ResetChunkDirty();
    }
}