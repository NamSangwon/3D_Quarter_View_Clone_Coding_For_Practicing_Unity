using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Transform target;
    public float orbitSpeed;
    public Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - target.position; // 수류탄 그룹과 플레이어와의 위치 차이
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + offset; // 수류탄 그룹 위치 업데이트
        transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime); // params = (위치, 회전축, 속도)
        offset = transform.position - target.position; // offset 업데이트
    }
}
