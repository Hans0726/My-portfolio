using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCardManager : MonoBehaviour
{
    private static LobbyCardManager _instance = null;
    public static LobbyCardManager Instance { get { return _instance; } }

    [SerializeField] private CardDatabase _cardDatabase;

    // �÷��̾ ������ ��� ī���� CardData SO ���� ����Ʈ
    private List<CardData> _ownedPlayerCards = new List<CardData>();
    public List<CardData> OwnedPlayerCards { get { return _ownedPlayerCards; } }

    // �÷��̾��� ���� ���� ���Ե� ī�� ID ����Ʈ (IngameCardManager�� ���޵� ����)
    private List<short> _currentDeckCardIds = new List<short>();
    public List<short> CurrentDeckCardIds { get { return _currentDeckCardIds; } }
    public int NumCardInDeck => _currentDeckCardIds.Count;

    [SerializeField] private GameObject _cardPrefab;

    // �� ���� ���� �� UIPopup_Deck�� �˸� �̺�Ʈ
    public event Action OnDeckCompositionChanged;

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    // �����κ��� ���� S_PlayerDeckInfo ��Ŷ���� ���� ī�� ���� �ʱ�ȭ
    public void InitializePlayerDeck(S_PlayerDeckInfo packet)
    {
        _ownedPlayerCards.Clear();
        _currentDeckCardIds.Clear();

        if (packet == null || packet.cards == null)
        {
            Debug.LogError("Received null packet or null cards list.");
            return;
        }

        // CardDatabase���� cardId�� CardData SO�� ã�ƿ�
        _cardDatabase.Initialize();

        foreach (S_PlayerDeckInfo.Card cardInfoFromServer in packet.cards) // ��Ŷ �ʵ���� ���� ���ǿ� �°�
        {

            CardData cardDataSO = _cardDatabase.GetCardDataById(cardInfoFromServer.cardId);

            if (cardDataSO != null)
            {
                _ownedPlayerCards.Add(cardDataSO); // ������ ī�� ��Ͽ� SO ���� �߰�

                if (cardInfoFromServer.isInDeck)
                {
                    _currentDeckCardIds.Add(cardDataSO.cardId); // ���� ���Ե� ī�� ID �߰�
                }
            }
            else
            {
                Debug.LogWarning($"CardData for cardId {cardInfoFromServer.cardId} not found in database. Skipping.");
            }
        }

        Debug.Log($"Player cards initialized. Owned: {_ownedPlayerCards.Count}, In Deck: {NumCardInDeck}");

        // �� �������� _currentDeckCardIds ����Ʈ�� IngameCardManager���� ������ �غ� ��
    }

    public bool TryAddCardToDeck(short cardId)
    {
        if (_currentDeckCardIds.Count >= UIPopup_Deck.MAX_NUM_CARDS)
        {
            Debug.Log("Deck is full.");
            return false;
        }
        if (!_currentDeckCardIds.Contains(cardId))
        {
            _currentDeckCardIds.Add(cardId);
            OnDeckCompositionChanged?.Invoke();
            return true;
        }
        return false;
    }

    public bool TryRemoveCardFromDeck(short cardId)
    {
        if (_currentDeckCardIds.Contains(cardId))
        {
            _currentDeckCardIds.Remove(cardId);
            OnDeckCompositionChanged?.Invoke(); // UI ���� �˸�
            return true;
        }
        return false; // ���� ���� ī��
    }

    public void SendUpdatedDeckToServer()
    {
        C_PlayerDeckInfo deckPacket = new C_PlayerDeckInfo();
        foreach (CardData cardData in _ownedPlayerCards)
        {
            bool isInCurrentDeck = _currentDeckCardIds.Contains(cardData.cardId);
            deckPacket.cards.Add(new C_PlayerDeckInfo.Card { cardId = cardData.cardId, isInDeck = isInCurrentDeck });
        }
        Debug.Log($"Sending updated deck to server. Card count: {deckPacket.cards.Count}");
        NetworkMananger.Instance.Send(deckPacket.Serialize());
    }
}