using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossMissile : Bullet // 상속 (for 보스 미사일 만이 유도되도록 하기 위해서)
{
    public Transform target;
    NavMeshAgent nav;

    // Start is called before the first frame update
    void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        nav.SetDestination(target.position);
    }
}
