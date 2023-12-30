using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon };
    public Type itemType;
    public int value;
    
    float rotateSpped = 20f;

    void Update(){
        transform.Rotate(Vector3.up * rotateSpped * Time.deltaTime); // 회전축 * 회전 속도 * deltaTime
    }
}
