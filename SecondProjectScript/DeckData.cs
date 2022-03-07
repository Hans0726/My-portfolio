using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Deck", menuName = "Deck", order = 2)]
public class DeckData : ScriptableObject
{
    // Start is called before the first frame update
    public Deck deck;
}
