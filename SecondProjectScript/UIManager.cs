using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection; 

public class UIManager : MonoBehaviour
{
    static public UIManager instance = null;

    // place UI
    public Text placeText;
    GameObject placeImage;
    GameObject blinkImage;

    // status UI
    public Text[] statusTexts;
    public GameObject buttonImage;
    public GameObject attackDamageButton;
    public GameObject attackDamageImage;
    public GameObject statusMenu;
    public Text[] rankProbability;

    // stage UI
    public Slider gatehpSlider;
    public Text gatehpText;
    public Slider expSlider;
    public Text expText;
    public Text stage;
    public Text clock;
    public Text clockRevival;

    public GameObject cardPanel;
    public GameObject[] cardButtons;
    public GameObject menuPanel;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(this);
    }
    void Start()
    {
        placeImage = transform.Find("PlaceImage").gameObject;
        blinkImage = transform.Find("PlaceSpot").transform.Find("Image").gameObject;
        StartCoroutine(BlinkPlaceImage());
        StartCoroutine(StageStart(GameManager.instance.currentTime));
    }

    public void PlacePlayer()
    {
        bool isActive = placeImage.activeSelf;

        if (isActive == false)
        {
            placeImage.SetActive(true);
            placeText.text = "��ġ����";
        }

        if (isActive == true)
        {
            placeImage.SetActive(false);
            placeText.text = "��ġ�ϱ�";
        }
    }
    public IEnumerator StageStart(float time)
    {
        int checkStage = GameManager.instance.stage;
    restart:
        yield return new WaitUntil(() => GameManager.instance.stageStart && checkStage == GameManager.instance.stage);
        checkStage++;
        stage.text = "Stage " + GameManager.instance.stage.ToString();
        yield return new WaitUntil(() => GameManager.instance.spawnStart == true);

        if (GameManager.instance.bossStage == false)
            while (time >= 0f)
            {
                time -= Time.deltaTime;
                clock.text = "00:" + (time % 60).ToString("00");

                if (time <= 0)
                {
                    time = GameManager.instance.currentTime;
                    goto restart;
                }
                yield return null;
            }
        else
        {
            time = 0f;
            while (true)
            {
                time += Time.deltaTime;
                clock.text = (time / 60).ToString("00") + ":" + (time % 60).ToString("00");

                if (GameManager.instance.spawnStart == false)
                {
                    GameManager.instance.currentTime = 30f;
                    time = GameManager.instance.currentTime;
                    goto restart;
                }
                yield return null;
            }
        }
    }

    IEnumerator BlinkPlaceImage()
    {
    restart:
        yield return new WaitUntil(() => placeImage.activeSelf == true);

        while (placeImage.activeSelf == true)
        {
            blinkImage.SetActive(true);
            yield return new WaitForSeconds(1f);
            blinkImage.SetActive(false);
            yield return new WaitForSeconds(1f);

            if (placeImage.activeSelf == false)
            {
                blinkImage.SetActive(false);
                goto restart;
            }    
        }
    }

    public void SetPlayerStatusTexts(PlayerStatus player)
    {
        // text ������ UI������Ʈ���� Text_�������� Substring �Լ��� ���ϵ� ���������� PlayerStatus ��ũ��Ʈ�� �����Ͽ� �� ���� �Ҵ�(���÷���)
        foreach (Text status in statusTexts)
        {
            string textObjectName = status.name.Substring(5);
            string inputText = player.GetType().GetField(textObjectName).GetValue(player).ToString();

            switch (textObjectName)
            {
                case "criticalRate": 
                case "avoidability":
                    status.text = inputText + "%";
                    break;
                case "attackSpeed":
                case "expRate":
                case "luck":
                    status.text = inputText + " ��";
                    break;
                case "skillPoints":
                    status.text = "����Ʈ: " + inputText;
                    break;
                case "level":
                    status.text = "����: " + inputText;
                    break;
                default:
                    status.text = inputText;
                    break;
            }
        }       
    }
}
