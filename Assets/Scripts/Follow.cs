using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform target; // 플레이어
    public Vector3 offset; // 카메라 고정 위치

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + offset;
    }
}
