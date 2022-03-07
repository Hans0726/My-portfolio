using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public class Deck
{
    public List<Card> cardList = new List<Card>();
}

[Serializable]
public class DeckInfo
{
    public List<int> cardInfoList = new List<int>();
}