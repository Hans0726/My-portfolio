using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeEvent : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && GameManager.instance.hasKey == true)
        {
            GameManager.instance.WinGame();
        }
    }
}
