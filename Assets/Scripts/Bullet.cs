using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;

    private void OnCollisionEnter(Collision other) {
        if(other.gameObject.tag == "Floor"){ // 탄피가 땅에 떨어지면 3초 뒤 제거
            Destroy(gameObject, 3); 
        }
    }

    void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "Wall"){ // 총알이 벽에 부딪히면 제거
            Destroy(gameObject);
        }
    }
}