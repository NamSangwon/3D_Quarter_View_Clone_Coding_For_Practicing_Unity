using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Game Info
    public GameObject menuCamera;
    public GameObject gameCamera;
    public Player player;
    public Boss boss;
    public int stage;
    public float playTime;
    public bool isBattle;
    public int enemyCntA;
    public int enemyCntB;
    public int enemyCntC;

    // Game UI
    public GameObject menuPanel;
    public GameObject gamePanel;

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

    void Awake(){
        maxScoreTxt.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore")); // {0:n0} (3자리 수 마다 ',' 찍기)과 같은 형식으로 찍기 
    }

    public void GameStart(){
        menuCamera.SetActive(false);
        gameCamera.SetActive(true);
        
        menuPanel.SetActive(false);
        gamePanel.SetActive(true);
        
        player.gameObject.SetActive(true);
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
            playerAmmoTxt.text = "- / " + player.maxAmmo;
        else 
            playerAmmoTxt.text = player.equipWeapon.curAmmo + " / " + player.maxAmmo;

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
        bossHealthBar.localScale = new Vector3((float)boss.curHealth / boss.maxHealth, 1, 1); // 보스의 현재 체력 / 최대 체력의 비율로 체력바 조정
    }
}
