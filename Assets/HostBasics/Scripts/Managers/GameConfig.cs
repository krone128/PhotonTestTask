using UnityEngine;

namespace HostBasics.Scripts
{
    public class GameConfig
    {
        public static Vector2Int GridChunks = new(10, 10);
        public static int ChunkSize = 100;
        public static int EntityCount = 5000;
        public static Vector2 GridSize = GridChunks * ChunkSize;
        public static int InterestRadius = 1;
    }
}