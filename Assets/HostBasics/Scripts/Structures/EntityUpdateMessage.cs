using UnityEngine;

namespace HostBasics.Scripts.Entities
{
    public struct EntityUpdateMessage
    {
        public short Id;
        public Vector2Half Position;
        public Vector2Half Destination;

        public EntityUpdateMessage(short id, Vector3 position, Vector3 destination)
        {
            Id = id;
            Position = position;
            Destination = destination;
        }
    }
}