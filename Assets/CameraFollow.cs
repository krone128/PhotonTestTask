using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform playerTransform;
    
    [SerializeField] private Vector3 _offset;

    // Update is called once per frame
    void Update()
    {
        if(!playerTransform) return;
        
        transform.position = playerTransform.position + _offset;
        transform.LookAt(playerTransform);
    }
}
