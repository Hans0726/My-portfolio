using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatePosition : MonoBehaviour
{
    [HideInInspector]
    public RectTransform imagePosition;

    Camera cam;
    Vector3 screenPoint;

    // Start is called before the first frame update
    void Awake()
    {
        imagePosition = transform.GetComponent<RectTransform>();
        cam = Camera.main;

        screenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100f);
        imagePosition.position = cam.ScreenToWorldPoint(screenPoint);
    }

    // Update is called once per frame
    void Update()
    {
        if (this.gameObject.activeSelf == true)
        {
            screenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100f);
            imagePosition.position = cam.ScreenToWorldPoint(screenPoint);
        }
    }

    public Vector3 GetPlacePoint()
    {
        return imagePosition.position;
    }
}
