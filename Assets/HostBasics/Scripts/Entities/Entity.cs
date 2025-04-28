using TMPro;
using UnityEngine;

namespace HostBasics.Scripts.Entities
{
    public partial class Entity : IEntity
    {
        private static short ENTITY_ID_POOL = 0;
        
        public float Speed = 10f;
        
        private short _id;
        
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
        
        public void SetDirty() => IsDirty = true;
        public void ResetDirty() => IsDirty = false;
        
        public void SetChunkDirty() => IsChunkDirty = true;
        public void ResetChunkDirty() => IsChunkDirty = false;

        private void OnMouseUpAsButton()
        {
            GameObject.Find("DebugText").GetComponent<TMP_Text>().text = $"Entity {_id}";
        }
    }
}