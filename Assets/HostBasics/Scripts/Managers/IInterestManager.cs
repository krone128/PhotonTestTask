using System.Collections;
using System.Collections.Generic;
using Fusion;
using HostBasics.Scripts.Entities;
using UnityEngine;

namespace HostBasics.Scripts
{
    public interface IInterestManager
    {
        void RegisterEntity(IEntity entity);
        void UnregisterEntity(IEntity entity);

        public void RegisterPlayer(PlayerRef player, Transform playerTransform);
        public void UnregisterPlayer(PlayerRef player);
    
        public IDictionary<PlayerRef, List<IEntity>> GetUpdatedEntities();
    }
}