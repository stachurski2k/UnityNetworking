using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetCollider : MonoBehaviour
{
    [SerializeField] bool useRigidBody=false;
    private void Start()
    {
        if(!NetworkManager.isServer()){
            if(TryGetComponent<Collider>(out Collider collider)){
                collider.enabled=false;
            }
            if(useRigidBody&&TryGetComponent<Rigidbody2D>(out Rigidbody2D body2D)){
                body2D.simulated=false;
            }
            if(useRigidBody&&TryGetComponent<Rigidbody>(out Rigidbody body)){
                body.detectCollisions=false;
            }
        }
    }
}
