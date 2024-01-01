using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range }; // { 근거리, 원거리 }
    public Type attackType; // 공격 타입
    public int damage; // 공격 데미지
    public float rate; // 공격 속도
    public BoxCollider meleeArea; // 근거리 공격 범위
    public TrailRenderer trailEffect; // 공격 효과

    public Transform bulletPos; // 총알 발사 위치
    public GameObject bullet; // 총알 종류
    public Transform bulletCasePos; // 탄피 배출 위치
    public GameObject bulletCase; // 탄피 종류

    public int maxAmmo;
    public int curAmmo;

    public void Use(){
        if(attackType == Type.Melee){
            StopCoroutine("Swing"); // 코루틴 종료
            StartCoroutine("Swing"); // 코루틴 실행
        }
        else if(attackType == Type.Range && curAmmo > 0){
            curAmmo--;
            StartCoroutine("Shot");
        }
    }

    
    // 일반 함수 = Use() 메인 루틴 -> Swing() 서브 루틴 -> Use() 메인 루틴
    // 코루틴 = Use() + Swing() 루틴 같이 실행
    IEnumerator Swing(){
        // Colider & Effect 활성화 후 비활성화
        yield return new WaitForSeconds(0.1f); // == 0.1초 대기
        meleeArea.enabled = true;
        trailEffect.enabled = true; 
        
        yield return new WaitForSeconds(0.3f); // == 0.3초 대기
        meleeArea.enabled = false;
        
        yield return new WaitForSeconds(0.3f); // == 0.3초 대기
        trailEffect.enabled = false;

        // yield return null; // IEnumerator에서는 yield를 1개 이상 반드시 리턴 해야 함 (== 1 프레임 대기)
        // yield break; // 코루틴 탈출
    }

    IEnumerator Shot(){
        // 1. 총알 발사
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation); // 총알 생성
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>(); // 총알 물리 생성
        bulletRigid.velocity = bulletPos.forward * 50; // 총알 발사 (bulletPos의 앞으로 발사)

        yield return null;
        
        // 2. 탄피 배출
        GameObject instantBulletCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation); // 탄피 생성
        Rigidbody bulletCaseRigid = instantBulletCase.GetComponent<Rigidbody>(); // 탄피 물리 생성
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3); // 총의 바깥 방향으로 랜덤하게 배출
        bulletCaseRigid.AddForce(caseVec, ForceMode.Impulse); // 탄피 배출
        bulletCaseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse); // 탄피 배출 시 회전
    }
}
