using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.Timeline;

public class Player : MonoBehaviour // RigidBody -> Constraints -> Freeze Rotate = 관성에 의한 회전 방지
{
    public GameManager manager;

    public float moveSpeed;
    public float jumpPower;

    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;
    public Camera followCamera;
    public GameObject grenadeObj;

    float hAxis;
    float vAxis;
    bool wDown;
    bool jDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;
    bool fDown;
    bool rDown;
    bool gDown;

    public int ammo;
    public int coin;
    public int health;
    public int score;
    public int hasGrenades;
    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

    bool isJump = false;
    bool isDodge = false;
    bool isSwap = false;
    bool isFireReady = true;
    bool isReload = false;
    bool isBorder;
    bool isDamage;
    bool isShop;
    bool isDead;

    Vector3 moveVec;
    Vector3 dodgeVec;
    Rigidbody rigid;
    Animator anim;
    MeshRenderer[] meshes;

    GameObject nearObject;
    public Weapon equipWeapon;
    int equipWeaponIndex = -1;
    float fireDelay = 0;

    void Awake() {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        meshes = GetComponentsInChildren<MeshRenderer>();

        PlayerPrefs.SetInt("MaxScore", 0);
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
        Reload();
        Grenade();
        Attack();
        Dodge();
        Swap();
        Interaction();
    }

    void GetInput(){
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload");
        iDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
    }

    void Move(){
        moveVec = new Vector3(hAxis, 0, vAxis).normalized; // 어느 방향이든 같은 속도를 가지도록 normalized

        if(isDodge) // 회피 시 이동 방향 변경 불가
            moveVec = dodgeVec;

        if (isSwap || !isFireReady || isReload || isDead) // 무기 스왑 or 사용 or 재장전 시 이동 불가
            moveVec = Vector3.zero;

        if (!isBorder){        
            if (wDown)
                transform.position += moveVec * moveSpeed * Time.deltaTime * 0.3f;
            else
                transform.position += moveVec * moveSpeed * Time.deltaTime;
            // transform.position += moveVec * moveSpeed * Time.deltaTime * (wDown ? 0.3f : 1f); // 삼항 연산자로 if문 처리
        }

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn(){
        // 1. 키보드에 의한 플레이어 회전
        transform.LookAt(transform.position + moveVec); // 나아가는 방향으로 플레이어 회전

        // 2. 마우스 클릭에 의한 플레이어 회전 (RayCastHit의 마우스 클릭 위치를 활용하여 회전을 구현)
        if (fDown && !isDead){
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if(Physics.Raycast(ray, out rayHit, 100)){ // Raycast()를 통해 rayHit 값 업데이트
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0; // 플레이어 기울어짐 방지 (ex. 벽에 마우스 클릭 시 플레이어가 기울어짐)
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Jump(){ // jump = 움직이지 않을 때, 점프 (회피랑 연속 사용 불가)
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap && !isShop && !isDead){
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse); // 즉발적인 힘 = ForceMode.Impulse
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Reload(){
        if(equipWeapon == null)
            return;
        if (equipWeapon.attackType == Weapon.Type.Melee)
            return;
        if (ammo <= 0)
            return;

        if(rDown && !isJump && !isDodge && !isSwap && isFireReady && !isShop && !isDead){
            anim.SetTrigger("doReload");
            isReload = true;
            Invoke("ReloadOut", 3f);
        }
    }

    void ReloadOut(){
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }

    void Grenade(){
        if (hasGrenades <= 0)
            return;

        if(gDown && !isReload && !isSwap && !isShop && !isDead){
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if(Physics.Raycast(ray, out rayHit, 100)){ // Raycast()를 통해 rayHit 값 업데이트
                Vector3 throwVec = rayHit.point - transform.position;
                throwVec.y = 12; // 플레이어 기울어짐 방지 (ex. 벽에 마우스 클릭 시 플레이어가 기울어짐)

                // 수류탄 투척
                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rigidGrednade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrednade.AddForce(throwVec, ForceMode.Impulse);
                rigidGrednade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }

    void Attack(){
        if(equipWeapon == null) 
            return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (fDown && isFireReady && !isDodge && !isSwap && !isShop && !isDead){
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.attackType == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Dodge(){ // dodge = 움직일 땐, 회피 (점프랑 연속 사용 불가)
        if (jDown && moveVec != Vector3.zero && !isDodge && !isJump && !isSwap && !isShop && !isDead){
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

        if((sDown1 || sDown2 || sDown3) && !isJump && !isDodge && !isShop && !isDead){
            if (equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[equipWeaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.5f);
        }
    }

    void SwapOut(){
        isSwap = false;
    }

    void Interaction(){
        if(iDown && nearObject != null && !isJump && !isDodge && !isDead){
            if(nearObject.tag == "Weapon"){
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
            else if (nearObject.tag == "Shop") {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
            }
        }
    }

    void OnCollisionEnter(Collision other) {
        if(other.gameObject.tag == "Floor"){
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other) {
        if(other.tag == "Item"){
            Item item = other.GetComponent<Item>();

            switch(item.itemType){
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo)
                        ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Grenade:
                    if (hasGrenades == maxHasGrenades)
                        return;
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades)
                        hasGrenades = maxHasGrenades;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
            }

            Destroy(other.gameObject);
        }
        else if(other.tag == "EnemyBullet"){
            if(!isDamage){
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;
                
                bool isBossAtk = other.name == "BossMeleeArea";
                StartCoroutine(onDamage(isBossAtk));
            }
            
            if (other.GetComponent<Rigidbody>() != null) {// 근접 공격과 원거리 공격 구분
                Destroy(other.gameObject);
            }
        }
    }

    IEnumerator onDamage(bool isBossAtk){ // 피격 후 1초 간 무적 (무적 시 플레이어 노랑색)
        isDamage = true;

        foreach (MeshRenderer mesh in meshes){
            mesh.material.color = Color.yellow;
        }

        if (isBossAtk)
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);

        if(health <= 0 && !isDead)
            OnDie();

        yield return new WaitForSeconds(1f);

        isDamage = false;
        foreach (MeshRenderer mesh in meshes){
            mesh.material.color = Color.white;
        }

        if (isBossAtk)
            rigid.velocity = Vector3.zero;
    }

    void OnDie(){
        anim.SetTrigger("doDie");
        isDead = true;
        manager.GameOver();
    }

    void FreezeRotation(){ // 플레이어 물체 충돌 시 자동 회전 제어
        rigid.angularVelocity = Vector3.zero;
    }

    void StopToWall(){ // 플레이어 벽 뚫기 방지
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green); // Ray Debugging 하기
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall")); // 5만큼의 길이에서 Wall 감지
    }

    void FixedUpdate() {
        FreezeRotation();
        StopToWall();
    }

    void OnTriggerStay(Collider other) {
        if(other.tag == "Weapon" || other.tag == "Shop"){
            nearObject = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other) {
        if(other.tag == "Weapon"){
            nearObject = null;
        }
        else if (other.tag == "Shop"){
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Exit();
            isShop = false;
            nearObject = null;
        }
    }
}
