using UnityEngine;

namespace HostBasics.Scripts.Entities
{
    public struct EntityUpdateMessage
    {
        public int Id;
        public Vector3 Position;
        public Vector3 Destination;

        public EntityUpdateMessage(int id, Vector3 position, Vector3 destination)
        {
            Id = id;
            Position = position;
            Destination = destination;
        }
    }
}