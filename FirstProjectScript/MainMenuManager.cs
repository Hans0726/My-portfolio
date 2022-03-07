using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    AudioSource audioSource;
    public AudioClip audioMouseCursor;
    public AudioClip audioMouseClick;

    public GameObject gameManual;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void OnMouseEnter()
    {
        audioSource.PlayOneShot(audioMouseCursor);
    }
    public void OnMouseDown()
    {
        audioSource.PlayOneShot(audioMouseClick);
    }
    public void StartGame()
    {
        Cursor.visible = false;
        SceneManager.LoadScene(1);
    }

    public void GameManual()
    {
        gameManual.SetActive(true);
    }
    public void CloseBox()
    {
        gameManual.SetActive(false);
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
