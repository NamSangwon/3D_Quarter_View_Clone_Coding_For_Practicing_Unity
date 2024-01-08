using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.AI;

public class BossRock : Bullet // 불릿 상속 (기모으기)
{
    Rigidbody rigid;
    float angularPower = 1;
    float scaleValue = 0.1f;
    bool isShoot;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        
        StartCoroutine(GainPowerTimer());
        StartCoroutine(GainPower());
    }

    IEnumerator GainPower(){
        while(!isShoot){ // floor의 physical material을 따로 설정하여 회전 시 앞으로 갈 수 있도록 수정
            scaleValue += 0.5f * Time.deltaTime;
            angularPower += 10f * Time.deltaTime;
            transform.localScale = Vector3.one * scaleValue;
            rigid.AddTorque(transform.right * angularPower, ForceMode.Acceleration);
            yield return null;
        }
    }

    IEnumerator GainPowerTimer(){
        yield return new WaitForSeconds(2.2f);
        isShoot = true;
    }
}
