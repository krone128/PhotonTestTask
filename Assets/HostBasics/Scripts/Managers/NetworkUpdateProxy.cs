using System;
using Fusion;
using HostBasics.Scripts.Entities;
using TMPro;
using UnityEngine;

namespace HostBasics.Scripts
{
    public class NetworkUpdateProxy : NetworkBehaviour
    {
        public static event Action<short> OnEntityPause;
        
        public static event Action OnFixedUpdateNetwork;

        private void Awake()
        {
            //Entity.OnClick += SendEntityPause;
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            
            if(_networkUpdateEnabled)
                OnFixedUpdateNetwork?.Invoke();
        }
        
        private bool _networkUpdateEnabled = true;
        
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                _networkUpdateEnabled = !_networkUpdateEnabled;
                RPC_SendEnableUpdatesOnEntities(_networkUpdateEnabled);
            }
        }

        public void SendEntityPause(short id)
        {
            _networkUpdateEnabled = false;
            RPC_SendEnableUpdatesOnEntities(_networkUpdateEnabled, id);
        }

        [Rpc(RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
        public void RPC_SendEnableUpdatesOnEntities(bool updateEnabled, short entityId = -1, RpcInfo info = default)
        {
            Debug.Log("RPC_SendEnableUpdatesOnEntities");
            Entity.NetworkUpdateEnabled = updateEnabled;

            if (entityId > -1)
            {
                OnEntityPause?.Invoke(entityId);
            }
        }
    }
}