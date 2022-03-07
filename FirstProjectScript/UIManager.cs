using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;
    public GameObject heart;
    GameObject[] hearts;
    public Sprite heartVoid;
    public static bool playerDamaged;

    public GameObject uiBox;
    public Text uiBoxText;
    public Text explainText;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        playerDamaged = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FadeTextToZero());
        hearts = new GameObject[PlayerMove.hp - 1];
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i] = Instantiate(heart,
                new Vector3(heart.transform.position.x + 105 * (i + 1), heart.transform.position.y, heart.transform.position.z),
                Quaternion.identity,
                transform) ;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (playerDamaged == true && PlayerMove.hp >= 1)
        {
            hearts[PlayerMove.hp - 1].GetComponent<Image>().sprite = heartVoid;
        }
        if (playerDamaged == true && PlayerMove.hp == 0)
            heart.GetComponent<Image>().sprite = heartVoid;
    }

    public IEnumerator FadeTextToZero()  // 알파값 1에서 0으로 전환
    {
        explainText.color = new Color(explainText.color.r, explainText.color.g, explainText.color.b, 1);
        while (explainText.color.a > 0.0f)
        {
            explainText.color = new Color(explainText.color.r, explainText.color.g, explainText.color.b, explainText.color.a - (Time.deltaTime * 0.5f));
            yield return null;
        }
    }
}
