using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeetCollision : MonoBehaviour
{
    SphereCollider feetColl;
    CapsuleCollider bodyColl;

    public bool _grounded = false;
    public Vector3 _terrainNormal;

    void Start()
    {
        feetColl = GetComponent<SphereCollider>();
        bodyColl = GetComponentInParent<CapsuleCollider>();
    }

    void Update()
    {      
    }

    private void OnCollisionStay(Collision collision) {
        Debug.Log("b");
        _terrainNormal = collision.GetContact(0).normal;
        _grounded = true;
    }

    private void OnCollisionExit(Collision collision) {
        _grounded = false;
    }
}
