using System;
using UnityEngine;

namespace HostBasics.Scripts.Entities
{
    public struct Vector2Half
    {
        public Half x;
        public Half y;

        private Vector2Half(Half x, Half y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator Vector2Half(Vector3 value)
        {
            return new Vector2Half(new Half(value.x), new Half(value.z));
        }

        public static implicit operator Vector3(Vector2Half value)
        {
            return new Vector3(value.x, 0, value.y);
        }
    }
}