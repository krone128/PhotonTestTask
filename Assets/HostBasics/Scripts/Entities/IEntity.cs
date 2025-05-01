using System;
using TMPro;
using UnityEngine;

namespace HostBasics.Scripts.Entities
{
    public interface IEntity
    {
        public short Id { get; }
        public bool IsDirty { get; }
        public bool IsChunkDirty { get; }
        
        public Vector3 Position { get; set; }
        public Vector3 Destination { get; set; }
        int LastUpdateTick { get; set; }
        bool IsMoving { get; }

        void SetDirty();
        void ResetDirty();
        
        void SetChunkDirty();
        void ResetChunkDirty();
        
        void Init();
        void Init(short id);
        void StartMovement(int updateTick = 0);
        void SetActive(bool isActive);
        void SelectNewDestination();
        void UpdateMovement(float networkRunnerDeltaTime);
    }
}