using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static public GameManager instance = null;

    // game system status that should be maintained
    [System.NonSerialized]
    public int gatehpMax = 200;
    [System.NonSerialized]
    public int gateHp = 200;
    [System.NonSerialized]
    public int stage = 0;
    [System.NonSerialized]
    public bool bossStage = false;
    [System.NonSerialized]
    public int placeableNum = 1;
    public List<float> playerPositionX;
    [System.NonSerialized]
    public float revivalTime = 8;

    [HideInInspector]
    public bool startPlace = false;
    [HideInInspector]
    public bool playerPlaced = false;
    [HideInInspector]
    public bool stageStart = false;
    public bool spawnStart = false;
    public float stageReinforcement = 1.3f;
    public float expReinforcement = 1.5f;

    // stage 바뀔 때 초기화되는 변수들
    public float spawnRate = 2f;
    public float snailRate = 1f;
    public float DemoralizingRate = 1f;
    public float tetkaiRate = 1f;

    public GameObject playerFactory;
    public UpdatePosition upos;
    public PlayerStatus status;
    public List<Player> players;

    public DeckInfo[] playerDeckInfo;

    [System.NonSerialized]
    public float currentTime = 2f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(this);
        status.Awake();
        playerDeckInfo = new DeckInfo[4];
        for (int i = 0; i < 4; i++)
        {
            playerDeckInfo[i] = new DeckInfo();
        }
    }

    void Start()
    {
        playerPositionX = new List<float>();
    }

    void Update()
    {
        if (placeableNum > 0 && startPlace == true)
        {
            --placeableNum;
            GameObject player = Instantiate(playerFactory);
            player.GetComponent<BoxCollider2D>().enabled = true;
            player.transform.position = new Vector3(upos.GetPlacePoint().x, playerFactory.transform.position.y, 0f);
            players.Add(player.GetComponent<Player>());
            playerPositionX.Add(player.transform.position.x);
            playerPlaced = true;
            UIManager.instance.PlacePlayer();
            UIManager.instance.SetPlayerStatusTexts(status);
            startPlace = false;    
        }
        if (spawnStart)
        {
            if (bossStage == false)
            {
                currentTime -= Time.deltaTime;
                if (currentTime <= 0f)
                {
                    spawnStart = false;
                    currentTime = 2f;
                }
            }
            else
                currentTime += Time.deltaTime;

        }
        UIManager.instance.gatehpSlider.value = (float)gateHp / (float)gatehpMax;
        UIManager.instance.gatehpText.text = gateHp + " / " + gatehpMax;
        UIManager.instance.expSlider.value = status.currentExp / status.totalExp;
        UIManager.instance.expText.text = "경험치: " + status.currentExp.ToString("N2") + " / " + status.totalExp.ToString("N2");
        UIManager.instance.rankProbability[0].text = status.rankProbability[0].ToString("N0") + "%";
        UIManager.instance.rankProbability[1].text = status.rankProbability[1].ToString("N0") + "%";
        UIManager.instance.rankProbability[2].text = status.rankProbability[2].ToString("N0") + "%";
        UIManager.instance.rankProbability[3].text = status.rankProbability[3].ToString("N0") + "%";
        UIManager.instance.rankProbability[4].text = status.rankProbability[4].ToString("N0") + "%";

        if (gateHp <= 0)
        {
            PauseGame();
            SoundManager.instance.audioSourceEFX.PlayOneShot(SoundManager.instance.defeat);
            UIManager.instance.menuPanel.SetActive(true);
            gateHp = gatehpMax;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            GameObject enemy = collision.gameObject;
            Enemy enemyStatus = enemy.GetComponent<Enemy>();
            gateHp -= enemyStatus.attackDamage;

            if (gateHp <= 0)
                gateHp = 0;

            enemyStatus.InitEnemy();
            
            FindObjectOfType<EnemySpawn>().enemyObjectPool.Add(enemy);
            enemy.SetActive(false);

            if (bossStage)
            {
                spawnStart = false;
                stageStart = false;
            }
                
        }
    }

    public void StageStart()
    {
        if (stageStart == false)
        {
            stage++;

            if (stage % 5 == 0)
                bossStage = true;
            else
                bossStage = false;

            if (bossStage == true)
            {
                SoundManager.instance.audioSourceBGM.clip = SoundManager.instance.boss;
            }
            else
                SoundManager.instance.audioSourceBGM.clip = SoundManager.instance.dungeon;

            SaveLoadSystem.SaveSystem(status, playerDeckInfo);
            CalculateRankProbability(status.rankProbability);
            spawnRate = 1f;
            snailRate = 1f;
            DemoralizingRate = 1f;
            tetkaiRate = 1f;
            stageStart = true;
            spawnStart = true;
        }
    }
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }
    float forwardRate = 2f;
    public void FastForward()
    {
        if (forwardRate > 4f)
            forwardRate = 2f;
        Time.timeScale = forwardRate;
        forwardRate++;
    }

    public void RestartGame()
    {
        StopAllCoroutines();
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
        status.Awake();
    }

    public void StatusMenu()
    {

        bool isActive = UIManager.instance.statusMenu.activeSelf;

        if (isActive == true)
        {
            ResumeGame();
            UIManager.instance.statusMenu.SetActive(false);
        }
        else
        {
            PauseGame();
            UIManager.instance.statusMenu.SetActive(true);
            if (status.skillPoints > 0)
            {
                if (playerPlaced)
                {
                    UIManager.instance.buttonImage.SetActive(true);
                    if (status.bloodsucking)
                    {
                        UIManager.instance.attackDamageButton.SetActive(false);
                        UIManager.instance.attackDamageImage.SetActive(false);
                    }
                }
            }
            else
                UIManager.instance.buttonImage.SetActive(false);
        }

    }
    public void CalculateRankProbability(float[] rankProbability)
    {
        ref float fluorite = ref rankProbability[0], 
            bronze = ref rankProbability[1], 
            silver = ref rankProbability [2], 
            gold = ref rankProbability[3], 
            platinum = ref rankProbability[4];

        if (stage < 3)
        {
            bronze = 100;
        }
        else if (stage < 5)
        {
            silver = 30 * status.luck;
            bronze = (100 - silver) * 4 / 7;
            fluorite = 100 - silver - bronze;
        }
        else if (stage < 8)
        {
            silver = 50 * status.luck;
            bronze = (100 - silver) * 3 / 5;
            fluorite = 100 - silver - bronze;
        }
        else if (stage < 10)
        {
            gold = 30 * status.luck;
            silver = (100 - gold) / 2;
            bronze = (100 - silver - gold) * 2 / 3;
            fluorite = (100 - gold - silver - bronze);
        }
        else
        {
            platinum = 15 * status.luck;
            gold = (100 - platinum) / 6;
            silver = (100 - gold - platinum) / 2;
            bronze = (100 - silver - gold - platinum) * 2 / 3;
            fluorite = (100 - platinum - gold - silver - bronze);
        }
    }
    public void LoadData()
    {
        if (stageStart)
            return;

        GameObject clone = GameObject.FindGameObjectWithTag("Player");

        if (clone != null)
           Destroy(clone);

        SaveData data = SaveLoadSystem.LoadSystem();
        if (data == null)
            return;
        stage = data.stage - 1;

        gateHp          = data.gateHp;
        placeableNum    = data.placeableNum;
        playerPositionX = data.playerPositionX;

        status.hpMax            = data.hpMax;
        status.hp               = data.hp;
        status.attackableNum    = data.attackableNum;
        status.attackDamage     = data.attackDamage;
        status.attackSpeed      = data.attackSpeed;
        status.criticalRate     = data.criticalRate;
        status.defense          = data.defense;
        status.avoidability     = data.avoidbility;
        status.expRate          = data.expRate;
        status.luck             = data.luck;
        status.rankProbability  = data.rankProbability;
        status.totalExp         = data.totalExp;
        status.currentExp       = data.currentExp;
        status.skillPoints      = data.skillPoints;

        status.level            = data.level;
        status.healValue        = data.healValue;

        playerDeckInfo          = data.playerDeckInfo;
        CardEvent decks = UIManager.instance.cardPanel.GetComponent<CardEvent>();

        for (int i = 0; i < 4; i++)
        {
            playerDeckInfo[i].cardInfoList.Sort(); 
            playerDeckInfo[i].cardInfoList.Reverse();  // 선택된 id 내림차순 정렬하여 list의 끝부터 삭제되도록함
            if (playerDeckInfo[i].cardInfoList.Count > 0)
            {
                if (i == 0) // 브론즈 덱 정보
                    for (int j = 0; j < playerDeckInfo[i].cardInfoList.Count; j++)  // i번째 덱의 요소들은 제거된 카드의 id로 구성돼 있음
                        decks.deckBronze.cardList.RemoveAt(playerDeckInfo[i].cardInfoList[j]);
                else if (i == 1)    // 실버 덱 정보
                    for (int j = 0; j < playerDeckInfo[i].cardInfoList.Count; j++)
                        decks.deckSilver.cardList.RemoveAt(playerDeckInfo[i].cardInfoList[j]);
                else if (i == 2)    // 골드 덱 정보
                    for (int j = 0; j < playerDeckInfo[i].cardInfoList.Count; j++)
                        decks.deckGold.cardList.RemoveAt(playerDeckInfo[i].cardInfoList[j]);
                else              // 플레티넘 덱 정보
                    for (int j = 0; j < playerDeckInfo[i].cardInfoList.Count; j++)
                        decks.deckPlatinum.cardList.RemoveAt(playerDeckInfo[i].cardInfoList[j]);
            }
        }

        status.fastSwordTechnique = data.fastSwordTechnique;
        status.bloodsucking     = data.bloodsucking;
        status.hedgehog         = data.hedgehog;

        UIManager.instance.SetPlayerStatusTexts(status);;
        UIManager.instance.buttonImage.SetActive(true);

        FindObjectOfType<EnemySpawn>().StopAllCoroutines();
        StartCoroutine(FindObjectOfType<EnemySpawn>().SpawnProcess());

        UIManager.instance.StopCoroutine(UIManager.instance.StageStart(currentTime));
        StartCoroutine(UIManager.instance.StageStart(currentTime));

        int count = playerPositionX.Count;

        if (playerPositionX.Count != 0)
        {
            GameObject player = Instantiate(playerFactory);
            player.transform.position = new Vector3(playerPositionX[0], playerFactory.transform.position.y, 0f);
            playerPlaced = true;

            for (int i = 1; i < count; i++)
            {
                GameObject playerX = Instantiate(playerFactory);
                playerX = player;
                playerX.transform.position = new Vector3(playerPositionX[i], playerFactory.transform.position.y, 0f);
            }
        }
    }
}
