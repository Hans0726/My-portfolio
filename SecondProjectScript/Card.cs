using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Rank { Fluorite, Bronze, Silver, Gold, Platinum };
[System.Serializable]
public class Card
{
    public int id;
    public Rank rank;
    public string cardTitle;
    public string cardDescription;

    public Sprite cardFlame;
    public Sprite artwork;
}