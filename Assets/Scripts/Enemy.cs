using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    public int maxHealth;
    public int curHealth;

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;

    void Awake() {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponent<MeshRenderer>().material;
    }

    void OnTriggerEnter(Collider other) {
        if(other.tag == "Melee"){
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec, false));
        }
        else if(other.tag == "Bullet"){
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject); // 총알 없애기

            StartCoroutine(OnDamage(reactVec, false));
        }
    }

    public void HitByGrenade(Vector3 explosionPos){
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec, true));
    }

    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade){
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0){
            mat.color = Color.white;
        }
        else{
            mat.color = Color.gray;
            gameObject.layer = 14; // layer 14 = EnemyDead

            if (isGrenade){ // 수류탄으로 적 처치 후 처리
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;
                
                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);

            }
            else { // 총알로 적 처치 후 처리
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
            }

            Destroy(gameObject, 4);
        }
    }
}
