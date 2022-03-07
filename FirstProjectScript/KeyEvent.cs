using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyEvent : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform[] spawnPoints;
    private void Start()
    {
        int tempNum = Random.Range(0, 6);
        transform.position = spawnPoints[tempNum].position;
    }

    private void Update()
    {
        transform.Rotate(new Vector3(0, 100f * Time.deltaTime, 0));
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.instance.hasKey = true;
            Destroy(transform.gameObject);
        }
    }
}
