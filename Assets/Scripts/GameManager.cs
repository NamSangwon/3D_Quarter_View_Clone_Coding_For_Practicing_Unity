using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Game Info
    public GameObject menuCamera;
    public GameObject gameCamera;
    public Player player;
    public Boss boss;
    public GameObject itemShop;
    public GameObject weaponShop;
    public GameObject startZone;
    public int stage;
    public float playTime;
    public bool isBattle;
    public int enemyCntA;
    public int enemyCntB;
    public int enemyCntC;
    public int enemyCntD;

    public Transform[] enemyZones;
    public GameObject[] enemies;
    public List<int> enemyList;

    // Game UI
    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject overPanel;

    public TextMeshProUGUI  maxScoreTxt;
    public TextMeshProUGUI  scoreTxt;
    
    public TextMeshProUGUI  stageTxt;
    public TextMeshProUGUI  playTimeTxt;

    public TextMeshProUGUI  playerHealthTxt;
    public TextMeshProUGUI  playerAmmoTxt;
    public TextMeshProUGUI  playerCoinTxt;

    public Image weapon1Img;
    public Image weapon2Img;
    public Image weapon3Img;
    public Image weaponRImg;

    public TextMeshProUGUI  enemyATxt;
    public TextMeshProUGUI  enemyBTxt;
    public TextMeshProUGUI  enemyCTxt;

    public RectTransform bossHealthGroup;
    public RectTransform bossHealthBar;

    public TextMeshProUGUI curScoreText;
    public TextMeshProUGUI bestText;

    void Awake(){
        enemyList = new List<int>();
        maxScoreTxt.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore")); // {0:n0} (3자리 수 마다 ',' 찍기)과 같은 형식으로 찍기 
    }

    public void GameStart(){
        menuCamera.SetActive(false);
        gameCamera.SetActive(true);
        
        menuPanel.SetActive(false);
        gamePanel.SetActive(true);
        
        player.gameObject.SetActive(true);
    }
    public void GameOver(){
        gamePanel.SetActive(false);
        overPanel.SetActive(true);
        curScoreText.text = scoreTxt.text;

        int maxScore = PlayerPrefs.GetInt("MaxScore"); 
        if (player.score > maxScore){
            bestText.gameObject.SetActive(true);
            PlayerPrefs.SetInt("MaxScore", player.score);
        }
    }

    public void Restart(){
        SceneManager.LoadScene(0);
    }


    public void StageStart(){
        itemShop.SetActive(false);
        weaponShop.SetActive(false);
        startZone.SetActive(false);

        foreach(Transform zone in enemyZones)
            zone.gameObject.SetActive(true);

        isBattle = true;
        StartCoroutine(InBattle());
    }

    public void StageEnd(){
        player.transform.position = Vector3.up * 0.5f;

        itemShop.SetActive(true);
        weaponShop.SetActive(true);
        startZone.SetActive(true);

        foreach(Transform zone in enemyZones)
            zone.gameObject.SetActive(false);

        isBattle = false;
        stage++;
    }

    IEnumerator InBattle(){
        if (stage % 5 == 0){ // 5의 배수의 스테이지는 보스 생성
            enemyCntD++;
            GameObject instantEnemy = Instantiate(enemies[3], enemyZones[0].position, enemyZones[0].rotation); // Enemy Zone 4개 중 하나에 랜덤하게 적 생성
            Enemy enemy = instantEnemy.GetComponent<Enemy>();
            enemy.manager = this; // enemy 처치 시 enemy count 감소를 Enemy.cs에서 진행하기 위함
            enemy.target = player.transform;
            boss = instantEnemy.GetComponent<Boss>();
        }
        else{ // 5의 배수 외의 스테이지에는 일반 스테이지
            for (int i = 0; i < stage; i++){ // Enemy A ~ C 까지 랜덤하게 생성하도록 함
                int rand = Random.Range(0, 3);
                enemyList.Add(rand);

                switch(rand){
                    case 0:
                        enemyCntA++;
                        break;
                    case 1:
                        enemyCntB++;
                        break;
                    case 2:
                        enemyCntC++;
                        break;
                }
            }

            while(enemyList.Count > 0){ // Enemy Zone 4개 중 하나에 랜덤하게 적 생성
                int randZone = Random.Range(0, 4);
                GameObject instantEnemy = Instantiate(enemies[enemyList[0]], enemyZones[randZone].position, enemyZones[randZone].rotation); 
                Enemy enemy = instantEnemy.GetComponent<Enemy>();
                enemy.target = player.transform;
                enemy.manager = this; // enemy 처치 시 enemy count 감소를 Enemy.cs에서 진행하기 위함
                enemyList.RemoveAt(0);
                yield return new WaitForSeconds(4f);
            }
        }

        while(enemyCntA + enemyCntB + enemyCntC + enemyCntD > 0) {
            yield return null;
        }

        yield return new WaitForSeconds(4f);

        boss = null;
        StageEnd();
    }


    void Update(){
        if (isBattle)
            playTime += Time.deltaTime;
    }

    void LateUpdate() { // Update()가 끝난 후 호출되는 생명주기
        // 상단 UI
        scoreTxt.text = string.Format("{0:n0}", player.score);
        stageTxt.text = "STAGE " + stage;

        int hour = (int) (playTime / 3600);
        int min = (int) ((playTime - hour * 3600) / 60);
        int second = (int) (playTime % 3600);
        playTimeTxt.text = string.Format("{0:00}", hour) + ":" + string.Format("{0:00}", min) + ":" + string.Format("{0:00}", second);

        // 플레이어 UI
        playerHealthTxt.text = player.health + " / " + player.maxHealth;
        playerCoinTxt.text = string.Format("{0:n0}", player.coin);
        if (player.equipWeapon == null || player.equipWeapon.attackType == Weapon.Type.Melee)
            playerAmmoTxt.text = "- / " + player.ammo;
        else 
            playerAmmoTxt.text = player.equipWeapon.curAmmo + " / " + player.ammo;

        // 무기 UI
        weapon1Img.color = new Color(1, 1, 1, player.hasWeapons[0] ? 1 : 0);
        weapon2Img.color = new Color(1, 1, 1, player.hasWeapons[1] ? 1 : 0);
        weapon3Img.color = new Color(1, 1, 1, player.hasWeapons[2] ? 1 : 0);
        weaponRImg.color = new Color(1, 1, 1, player.hasGrenades > 0 ? 1 : 0);

        // 몬스터 개체 수 UI
        enemyATxt.text = enemyCntA.ToString();
        enemyBTxt.text = enemyCntB.ToString();
        enemyCTxt.text = enemyCntC.ToString();

        // 보스 체력 UI
        if (boss != null){
            bossHealthGroup.anchoredPosition = Vector3.down * 20f;
            bossHealthBar.localScale = new Vector3((float)boss.curHealth / boss.maxHealth, 1, 1); // 보스의 (현재 체력 / 최대 체력)의 비율로 체력바 조정
        }
        else{
            bossHealthGroup.anchoredPosition = Vector3.up * 200f;
        }
    }
}
