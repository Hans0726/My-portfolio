using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TransitionScene());
    }

    IEnumerator TransitionScene()
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(2);
        ao.allowSceneActivation = false;

        while (ao.isDone == false)
        {
            if (ao.progress >= 0.9f)
                ao.allowSceneActivation = true;
            yield return null;
        }
        
    }
}
