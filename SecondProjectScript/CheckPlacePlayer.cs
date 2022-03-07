using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlacePlayer : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("PlaceImage") == true && Input.GetMouseButtonDown(0) == true)
        {
            GameManager.instance.startPlace = true;
        }
    }
}
