using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour // RigidBody -> Constraints -> Freeze Rotate = 관성에 의한 회전 방지
{
    public float moveSpeed;
    public float jumpPower;
    public GameObject[] weapons;
    public bool[] hasWeapons;

    float hAxis;
    float vAxis;
    bool wDown;
    bool jDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;

    bool isJump = false;
    bool isDodge = false;
    bool isSwap = false;

    Vector3 moveVec;
    Vector3 dodgeVec;
    Rigidbody rigid;
    Animator anim;

    GameObject nearObject;
    GameObject equipWeapon;
    int equipWeaponIndex = -1;

    void Awake() {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Dodge();
        Swap();
        Interaction();
    }

    void GetInput(){
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
    }

    void Move(){
        moveVec = new Vector3(hAxis, 0, vAxis).normalized; // 어느 방향이든 같은 속도를 가지도록 normalized

        if(isDodge) // 회피 시 이동 방향 변경 불가
            moveVec = dodgeVec;

        if (isSwap) // 무기 스왑 시 이동 불가
            moveVec = Vector3.zero;

        if (wDown)
            transform.position += moveVec * moveSpeed * Time.deltaTime * 0.3f;
        else
            transform.position += moveVec * moveSpeed * Time.deltaTime;
        // transform.position += moveVec * moveSpeed * Time.deltaTime * (wDown ? 0.3f : 1f); // 삼항 연산자로 if문 처리

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn(){
        transform.LookAt(transform.position + moveVec); // 나아가는 방향으로 플레이어 회전
    }

    void Jump(){ // jump = 움직이지 않을 때, 점프 (회피랑 연속 사용 불가)
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap){
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse); // 즉발적인 힘 = ForceMode.Impulse
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Dodge(){ // dodge = 움직일 땐, 회피 (점프랑 연속 사용 불가)
        if (jDown && moveVec != Vector3.zero && !isDodge && !isJump && !isSwap){
            dodgeVec = moveVec;
            moveSpeed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.6f);
        }
    }

    void DodgeOut(){
        moveSpeed *= 0.5f;
        isDodge = false;
    }

    void Swap(){
        // (같은 무기로 변경 || 무기 소지하고 있지 않을 때) 무기 변경 시 -> 스왑 액션 하지 않음
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0)) 
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1)) 
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2)) 
            return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if((sDown1 || sDown2 || sDown3) && !isJump && !isDodge){
            if (equipWeapon != null)
                equipWeapon.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[equipWeaponIndex];
            equipWeapon.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.5f);
        }
    }

    void SwapOut(){
        isSwap = false;
    }

    void Interaction(){
        if(iDown && nearObject != null && !isJump && !isDodge){
            if(nearObject.tag == "Weapon"){
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }
    }

    void OnCollisionEnter(Collision other) {
        if(other.gameObject.tag == "Floor"){
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerStay(Collider other) {
        if(other.tag == "Weapon"){
            nearObject = other.gameObject;
        }

    }

    void OnTriggerExit(Collider other) {
        if(other.tag == "Weapon"){
            nearObject = null;
        }
    }
}
