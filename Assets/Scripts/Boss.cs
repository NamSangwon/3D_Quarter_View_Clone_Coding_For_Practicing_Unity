using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    public GameObject missiles;
    public Transform missilePortA;
    public Transform missilePortB;
    // Enemy의 bullet = Boss Rock

    Vector3 lookVec;
    Vector3 tauntVec;
    
    bool isLook = true;

    // Start is called before the first frame update
    void Awake(){ // Awake() 함수는 자식 스크립트만 단독 실행!!!
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshes = GetComponentsInChildren<MeshRenderer>(); // 해당 오브젝트의 하위 mesh matrial 받아 오기
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        nav.isStopped = true;
        StartCoroutine(Think());
    }

    // Update is called once per frame
    void Update()
    {
        if(isDead){
            StopAllCoroutines();
            return;
        }

        if (isLook){
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            lookVec = new Vector3(h, 0, v) * 5f; // 플레이어 움직임에 맞춰 보스 타겟팅하기 위한 벡터
            transform.LookAt(target.position + lookVec); // 플레이어 움직임에 맞춰 보스 타겟팅
        }
        else{
            nav.SetDestination(tauntVec);
        }
    }

    IEnumerator Think(){
        yield return new WaitForSeconds(0.1f);

        int randomAction = Random.Range(0, 5); // 0 ~ 4 까지의 랜덤 패턴

        switch(randomAction){
            // shoot missiles
            case 0:
            case 1:
                StartCoroutine(MissileShot());
                break;

            // shoot Rock
            case 2:
            case 3:
                StartCoroutine(RockShot());
                break;

            // Taunt attack
            case 4:
                StartCoroutine(Taunt());
                break;
        }
    }

    // Boss Attack Pattern
     IEnumerator MissileShot(){
        anim.SetTrigger("doShot");

        yield return new WaitForSeconds(0.2f);
        GameObject instantMissileA = Instantiate(missiles, missilePortA.position, missilePortA.rotation);
        BossMissile bossMissileA = instantMissileA.GetComponent<BossMissile>();
        bossMissileA.target = target;

        yield return new WaitForSeconds(0.3f);
        GameObject instantMissileB = Instantiate(missiles, missilePortB.position, missilePortB.rotation);
        BossMissile bossMissileB = instantMissileB.GetComponent<BossMissile>();
        bossMissileB.target = target;

        yield return new WaitForSeconds(2f);

        StartCoroutine(Think());
     }
     IEnumerator RockShot(){
        isLook = false;
        anim.SetTrigger("doBigShot");

        Instantiate(bullet, transform.position, transform.rotation);
        yield return new WaitForSeconds(3f);

        isLook = true;
        StartCoroutine(Think());
     }
     IEnumerator Taunt(){
        tauntVec = target.position + lookVec;

        isLook = false;
        nav.isStopped = false;
        boxCollider.enabled = false;
        anim.SetTrigger("doTaunt");

        yield return new WaitForSeconds(1.5f);
        meleeArea.enabled = true;

        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(1f);
        isLook = true;
        nav.isStopped = true;
        boxCollider.enabled = true;
        StartCoroutine(Think());
     }
}
