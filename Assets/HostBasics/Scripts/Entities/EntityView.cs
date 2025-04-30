using TMPro;
using UnityEngine;

namespace HostBasics.Scripts.Entities
{
    public partial class Entity : MonoBehaviour
    {
        private static short ENTITY_ID_POOL = 0;
        public static bool NetworkUpdateEnabled = true;
        
        public float Speed = 10f;
        
        public MeshRenderer meshRenderer;
        
        public Material RedMaterial;
        public Material GreenMaterial;

        private short _id;
        private bool _isMoving;

        Vector3 TargetDirection { get; set; }

        private void Update()
        {
            if(NetworkUpdateEnabled)
                UpdateMovement();
        }

        private void UpdateMovement()
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
        
        
        private void OnMouseUpAsButton()
        {
            GameObject.Find("EntityDebugText").GetComponent<TMP_Text>().text = $"Entity {_id}";
            OnClick?.Invoke(_id);
        }

        private void OnDrawGizmos()
        {
            //Debug.DrawLine(transform.position, Destination, Color.green);
        }
    }
}