using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    static public SoundManager instance = null;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        audioSourceBGM = transform.GetChild(0).GetComponent<AudioSource>();
        audioSourceEFX = transform.GetChild(1).GetComponent<AudioSource>();
    }
    [HideInInspector]
    public AudioSource audioSourceBGM;
    [HideInInspector]
    public AudioSource audioSourceEFX;
    public AudioClip[] attack;
    public AudioClip defeat;
    public AudioClip cardSelect;
    public AudioClip levelUP;
    public AudioClip boss;
    public AudioClip dungeon;
    public AudioClip death;
}
