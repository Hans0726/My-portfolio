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
    int tempNumEnemies;

    void Start()
    {
        spawnPoint = transform;
        tempNumEnemies = numEnemies;
        StartCoroutine(SpawnProcess());
    }

    public IEnumerator SpawnProcess()
    {
    restart:
        int stage = (GameManager.instance.stage - 1) % 5;
        if (GameManager.instance.stage % 5 == 0)
            numEnemies = 1;
        else
            numEnemies = 20;

        if (enemyObjectPool.Count > 0) // �ε� �� �ʱ�ȭ
        {
            for (int i = 0; i < tempNumEnemies; i++)    // ���ο� �������� �� ������ ���� ���� ������Ʈ �Ҹ�� ������Ʈ Ǯ ��� ����
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
            if (GameManager.instance.bossStage == true)
            {
                SoundManager.instance.audioSourceBGM.clip = SoundManager.instance.boss;
                SoundManager.instance.audioSourceBGM.Play();
                break;
            }
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
            {   // ���� �ð� ���� ��, ������Ʈ Ǯ ȸ���� ��� �Ϸ�Ǹ�
                if (enemyObjectPool.Count == numEnemies)
                {
                    for (int i = 0; i < numEnemies; i++)    // ���ο� �������� �� ������ ���� ���� ������Ʈ �Ҹ�� ������Ʈ Ǯ ��� ����
                    {
                        GameObject enemy = enemyObjectPool[0];
                        enemyObjectPool.Remove(enemy);
                        Destroy(enemy);
                        if (GameManager.instance.bossStage == true)
                        {
                            SoundManager.instance.audioSourceBGM.clip = SoundManager.instance.dungeon;
                            SoundManager.instance.audioSourceBGM.Play();
                            break;
                        }
                            
                    }
                    GameManager.instance.stageStart = false;
                    GameManager.instance.stage++;
                    if (GameManager.instance.stage % 5 == 0)
                        GameManager.instance.bossStage = true;
                    else
                        GameManager.instance.bossStage = false;
                    goto restart;
                }
            }
            yield return null;
        }
    }
}
