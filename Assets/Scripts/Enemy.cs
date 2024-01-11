using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    public GameManager manager;

    public enum Type { A, B, C, D };
    public Type enemyType;
    public int maxHealth;
    public int curHealth;
    public int score;
    public Transform target;
    public BoxCollider meleeArea;
    public GameObject bullet;
    public GameObject[] coins;
    
    protected bool isChase;
    protected bool isAttack;
    protected bool isDead;

    protected Rigidbody rigid;
    protected BoxCollider boxCollider;
    protected MeshRenderer[] meshes;
    protected NavMeshAgent nav;
    protected Animator anim;

    void Awake() {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshes = GetComponentsInChildren<MeshRenderer>(); // 해당 오브젝트의 하위 mesh matrial 받아 오기
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if(enemyType != Type.D) // Boss는 쫓아 가지 않음
            Invoke("ChaseStart", 2);
    }

    void Update(){
        if (nav.enabled && enemyType != Type.D){
            nav.SetDestination(target.position);
            // nav.destination = target.position; // target 위치 추적
            nav.isStopped = !isChase;
        }   
    }

    void ChaseStart(){
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    void FixedUpdate() {
        Targeting();
        FreezeVelocity();
    }

    void Targeting(){
        if (!isDead && enemyType != Type.D){
            float targetRadius = 0f;
            float targetRange = 0f;

            switch (enemyType){
                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;
                case Type.B:
                    targetRadius = 1f;
                    targetRange = 12f;
                    break;
                case Type.C:
                    targetRadius = 0.5f;
                    targetRange = 25f;
                    break;

            }

            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

            if (rayHits.Length > 0 && !isAttack){
                StartCoroutine(Attack());
            }
        }
    }

    IEnumerator Attack(){
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch (enemyType){
            case Type.A: // 일반 공격
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;
                
                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;

                yield return new WaitForSeconds(1f);
                break;
            case Type.B: // 돌격
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
                meleeArea.enabled = true;
                
                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;

                yield return new WaitForSeconds(2f);
                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;
                
                yield return new WaitForSeconds(2f);
                break;
        }
        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);
    }

    void FreezeVelocity(){
        if (isChase){
            rigid.angularVelocity = Vector3.zero;
            rigid.velocity = Vector3.zero;
        }
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
        foreach(MeshRenderer mesh in meshes)
            mesh.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0){
            foreach(MeshRenderer mesh in meshes)
                mesh.material.color = Color.white;
        }
        else{
            foreach(MeshRenderer mesh in meshes)
                mesh.material.color = Color.gray;
            gameObject.layer = 14; // layer 14 = EnemyDead
            isDead = true;
            isChase = false;
            nav.enabled = false;
            anim.SetTrigger("doDie"); 

            // 코인 드랍 
            Player player = target.GetComponent<Player>();
            player.score += score;
            int randomCoin = Random.Range(0, 3);
            Instantiate(coins[randomCoin], transform.position, Quaternion.identity);

            switch(enemyType){
                case Type.A:
                    manager.enemyCntA--;
                    break;
                case Type.B:
                    manager.enemyCntB--;
                    break;
                case Type.C:
                    manager.enemyCntC--;
                    break;
                case Type.D:
                    manager.enemyCntD--;
                    break;
            }

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
