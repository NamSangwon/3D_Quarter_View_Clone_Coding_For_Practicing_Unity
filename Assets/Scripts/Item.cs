using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon };
    public Type itemType;
    public int value;

    Rigidbody rigid;
    SphereCollider sphereCollider;

    void Awake() {
        rigid = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }
    
    float rotateSpped = 20f;

    void Update(){
        transform.Rotate(Vector3.up * rotateSpped * Time.deltaTime); // 회전축 * 회전 속도 * deltaTime
    }

    void OnCollisionEnter(Collision other) {
        if(other.gameObject.tag == "Floor"){
            rigid.isKinematic = true;
            sphereCollider.enabled = false;
        }
    }
}
