using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    GameObject player;
    public GameObject enemy;
    GameObject[] enemyClone;
    const int numEnemies = 3;
    bool playerInSightFlag;
    Transform center;
    NavMeshAgent nav;
    public bool hasKey = false;

    public bool isPause = false;
    bool isEnd = false;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        Cursor.lockState = CursorLockMode.Locked;
        PlayerMove.hp = 3;

        player = GameObject.FindGameObjectWithTag("Player");
        enemyClone = new GameObject[numEnemies];
        center = GameObject.FindGameObjectWithTag("Respawn").transform;
        nav = enemy.GetComponent<NavMeshAgent>();

        player.transform.position = RandomSpawnNearPoint(center.position, 100f);
    }

    private void Update()
    {
        playerInSightFlag = true;
        for (int i = 0; i < numEnemies; i++)
        {
            if (enemyClone[i] == null)
            {
                enemyClone[i] = Instantiate(enemy);
                enemyClone[i].transform.position = RandomSpawnNearPoint(player.transform.position, 100f);
                enemyClone[i].SetActive(true);
            }

            playerInSightFlag &= !(enemyClone[i].GetComponent<EnemyFSM>().playerInSight);  
        } // playerInSIght가 하나라도 true면 false가 되어 AND연산 결과가 false가 됨, 모두 false일 경우에만 true가 됨
        if (playerInSightFlag == true)
            SoundManager.instance.audioSourceBGM.Pause();

        if (isEnd == false && Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPause == false)
            {
                PauseGame();   
                return;
            }

            if (isPause == true)
            {
                ContinueGame();
                return;
            }
        }
    }
    public Vector3 RandomSpawnNearPoint(Vector3 nearPoint, float range)
    {
    restart:
        Vector3 point = nearPoint + (Random.insideUnitSphere * range);
        point.y = 3f;
        if (Vector3.Distance(point, nearPoint) <= range * 0.75f)
            goto restart;

        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(point, out hit, 10f, 1))
            finalPosition = hit.position + Vector3.up;
        else
            goto restart;
        if (finalPosition.y >= 5f)
            goto restart;

        return finalPosition;
    }

    public void PauseGame()
    {
        isPause = true;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SoundManager.instance.audioSourceBGM.Pause();
        SoundManager.instance.audioSourceEFX.Pause();
        UIManager.instance.uiBox.SetActive(true);
        UIManager.instance.uiBoxText.text = "Pause";
    }
    public void ContinueGame()
    {
        UIManager.instance.uiBox.SetActive(false);
        SoundManager.instance.audioSourceBGM.Play();
        SoundManager.instance.audioSourceEFX.Play();
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        isPause = false;
    }

    public void WinGame()
    {
        PauseGame();
        isEnd = true;
        UIManager.instance.uiBoxText.text = "Win";
    }
    public void LoseGame()
    {
        PauseGame();
        isEnd = true;
        UIManager.instance.uiBoxText.text = "Lose";
    }

    public void RestartGame()
    {
        Cursor.visible = false;
        UIManager.instance.uiBox.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
