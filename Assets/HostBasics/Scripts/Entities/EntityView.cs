
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HostBasics.Scripts.Entities
{
    public partial class Entity : MonoBehaviour
    {
        public MeshRenderer meshRenderer;
        
        public Material RedMaterial;
        public Material GreenMaterial;

        private bool _isMoving;
        
        public bool IsMoving
        {
            get => _isMoving;
            private set
            {
                _isMoving = value;
                meshRenderer.material = value ? GreenMaterial : RedMaterial;
            }
        }

        Vector3 TargetDirection { get; set; }
    
        public bool IsActive => gameObject.activeSelf;
    
        public void StartMovement()
        {
            TargetDirection = Destination - transform.position;
            TargetDirection = TargetDirection.normalized;
            transform.LookAt(Destination);
            IsMoving = true;
        }

        public void Update()
        {
            UpdateMovement();
        }

        public void UpdateMovement()
        {
            if (!IsMoving) return;
        
            var posDelta = TargetDirection * (Speed * Time.deltaTime);
            
            if (Vector3.Magnitude(Destination - transform.position) > posDelta.magnitude)
            {
                transform.position += posDelta;
                return;
            }
            
            IsMoving = false;
            transform.position = Destination;
            
            SelectNewDestination();
        }
    
        public void SelectNewDestination()
        {
            if(!Authoritative) return;
            var destination = new Vector3(Random.Range(0, GameConfig.GridSize.x), 0f, Random.Range(0, GameConfig.GridSize.y));
            Destination = destination;
            StartMovement();
            IsDirty = true;
            OnDestinationUpdated?.Invoke(this, Destination);
            
        }

        private void OnDrawGizmos()
        {
            //Debug.DrawLine(transform.position, Destination, Color.green);
        }
    }
}