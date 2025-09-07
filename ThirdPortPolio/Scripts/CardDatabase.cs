using UnityEngine;
using System.Collections.Generic;
using System.Linq;
[CreateAssetMenu(fileName = "CardDatabase", menuName = "Card Database", order = 0)]
public class CardDatabase : ScriptableObject
{
    public List<CardData> _allCards = new List<CardData>();
    private Dictionary<short, CardData> _cardDictionary;

    // ���� ���� �� �Ǵ� �ʿ��� �� ȣ���Ͽ� ��ųʸ��� �ʱ�ȭ�մϴ�.
    public void Initialize()
    {
        if (_cardDictionary == null)
        {
            _cardDictionary = new Dictionary<short, CardData>();
            foreach (CardData card in _allCards)
            {
                if (card != null && !_cardDictionary.ContainsKey(card.cardId))
                {
                    _cardDictionary.Add(card.cardId, card);
                }
                else
                {
                    Debug.LogWarning($"CardDatabase: Duplicate cardId {card?.cardId} or null card found in allCards list.");
                }
            }
            Debug.Log($"CardDatabase Initialized with {_cardDictionary.Count} cards.");
        }
    }

    // ID�� CardData�� �������� �Լ�
    public CardData GetCardDataById(short id)
    {
        if (_cardDictionary == null)
        {
            Debug.LogError("CardDatabase not initialized! Call Initialize() first.");
            return null;
        }

        if (_cardDictionary.TryGetValue(id, out CardData cardData))
        {
            return cardData;
        }
        Debug.LogWarning($"CardData with id {id} not found in database.");
        return null;
    }
}