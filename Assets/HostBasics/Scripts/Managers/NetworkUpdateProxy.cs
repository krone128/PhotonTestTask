using System;
using Fusion;

namespace HostBasics.Scripts
{
    public class NetworkUpdateProxy : NetworkBehaviour
    {
        public static event Action OnFixedUpdateNetwork;
        
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            OnFixedUpdateNetwork?.Invoke();
        }
    }
}