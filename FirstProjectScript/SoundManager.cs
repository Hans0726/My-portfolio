using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance = null;

    [HideInInspector]
    public AudioSource audioSourceBGM;
    [HideInInspector]
    public AudioSource audioSourceEFX;

    public AudioClip audioChase;
    public AudioClip audioDamaged;
    public AudioClip audioAttack;
    public AudioClip[] audioKick;
    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start()
    {
        audioSourceBGM = transform.GetChild(0).GetComponent<AudioSource>();
        audioSourceEFX = transform.GetChild(1).GetComponent<AudioSource>();
    }
 
}
