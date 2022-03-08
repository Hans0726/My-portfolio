using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public GameObject[] enemyObjects;
    [System.NonSerialized]
    public List<GameObject> enemyObjectPool = new List<GameObject>();
    Transform spawnPoint;
    int numEnemies = 20;

    void Start()
    {
        spawnPoint = transform;
        StartCoroutine(SpawnProcess());
    }

    public IEnumerator SpawnProcess()
    {
    restart:
        int stage = GameManager.instance.stage % 5;
        if (GameManager.instance.stage % 5 == 4)    // 미리 오브젝트를 생성하기 위해 해당 스테이지의 이 전 스테이지를 기준으로 에네미 오브젝트의 수를 정함
        {
            numEnemies = 1;
            SoundManager.instance.audioSourceBGM.clip = SoundManager.instance.boss;
            SoundManager.instance.audioSourceBGM.Play();
        }
        else
            numEnemies = 20;

        if (enemyObjectPool.Count != 0)
        {
            int objectNumBeforeLoad = enemyObjectPool.Count;

            for (int i = 0;i< objectNumBeforeLoad; i++)
            {
                GameObject enemy = enemyObjectPool[0];
                enemyObjectPool.Remove(enemy);
                Destroy(enemy);
            }
        }

        for (int i = 0; i < numEnemies; i++)
        {
            GameObject enemy = Instantiate(enemyObjects[stage], spawnPoint);
            enemyObjectPool.Add(enemy);
            enemy.SetActive(false);
        }

        yield return new WaitUntil(() => GameManager.instance.stageStart == true);

        while (true)
        {
            if (GameManager.instance.spawnStart == true)
            {
                if (enemyObjectPool.Count > 0)
                {
                    GameObject enemy = enemyObjectPool[0];
                    enemy.transform.position = spawnPoint.position;
                    enemy.SetActive(true);
                    enemy.GetComponent<Enemy>().isAlive = true;
                    enemyObjectPool.Remove(enemy);
                    yield return new WaitForSeconds(GameManager.instance.spawnRate);
                }
            }
            else
            {   // 스폰 시간 종료 시, 오브젝트 풀 회수가 모두 완료되면
                if (enemyObjectPool.Count == numEnemies)
                {
                    for (int i = 0; i < numEnemies; i++)    // 새로운 스테이지 적 생성을 위한 기존 오브젝트 소멸과 오브젝트 풀 요소 제거
                    {
                        GameObject enemy = enemyObjectPool[0];
                        enemyObjectPool.Remove(enemy);
                        Destroy(enemy);
                    }
                    if (GameManager.instance.bossStage == true)
                    {
                        SoundManager.instance.audioSourceBGM.clip = SoundManager.instance.dungeon;
                        SoundManager.instance.audioSourceBGM.Play();
                    }
                        
                    GameManager.instance.stageStart = false;
                    goto restart;
                }
            }
            yield return null;
        }
    }
}
