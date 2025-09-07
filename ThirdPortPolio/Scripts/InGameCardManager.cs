using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;

public class InGameCardManager : MonoBehaviour
{
    public static InGameCardManager Instance { get; private set; }

    [Header("Card Database")]
    [SerializeField] private CardDatabase _cardDatabase; // �ν����Ϳ��� �Ҵ�

    private List<CardData> _playerDeck = new List<CardData>(); // ���� ���ӿ��� ����� ��
    public List<CardData> PlayerDeck => _playerDeck;

    private List<CardData> _playerHand = new List<CardData>(); // ���� �÷��̾��� �ڵ�
    public List<CardData> PlayerHand => _playerHand;

    private List<CardData> _playerDiscardPile = new List<CardData>(); // ������ ī�� ���� (���� ����)
    public List<CardData> PlayerDiscardPile => _playerDiscardPile;

    public int InitialHandSize = 3; // �ʱ� �ڵ� ũ��

    // �̺�Ʈ (UI �� �ٸ� ������ �ڵ� ������ ������ �� �ֵ���)
    public event Action<CardData> OnCardDrawn;  // ī�带 �̾��� ��
    public event Action<CardData> OnCardPlayed; // ī�带 ������� ��
    public event Action OnInitialHandDrawn;     // �ʱ� �ڵ� ��ο찡 '�Ϸ�'�Ǿ��� ��

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _cardDatabase.Initialize(); // �����ͺ��̽� �ʱ�ȭ   
    }

    public void Initialize(List<short> deckCardIds)
    {
        Debug.Log("[InGameCardManager] Initializing...");
        _cardDatabase.Initialize();
        InitializeDeck(deckCardIds);
        ShuffleDeck();
    }

    // �׽�Ʈ�� �ʱ�ȭ �Լ� (GameManager���� ȣ��)
    public void TestInitialize()
    {
        Debug.Log("[InGameCardManager] Test Initializing...");
        _cardDatabase.Initialize();
        for (int i = 0; i < 10; i++)
        {
            // ����ī�� 0~7 / ����ī�� 100~101���� �ۿ� �����Ƿ�
            short randomAttackCardId = (short)UnityEngine.Random.Range(0, 8);
            short randomDefenseCardId = (short)UnityEngine.Random.Range(100, 102);
            short cointoss = (short)UnityEngine.Random.Range(0, 2);
            short resId = cointoss == 0 ? randomAttackCardId : randomDefenseCardId;

            CardData cardData = _cardDatabase.GetCardDataById(resId);
            if (cardData != null)
            {
                _playerDeck.Add(cardData);
            }
        }
        ShuffleDeck();
    }

    /// <summary>
    /// �κ񿡼� ���޹��� �� ī�� ID ����Ʈ�� ���� ���� �ʱ�ȭ�մϴ�.
    /// GameManager ���� ���� ȣ��˴ϴ�.
    /// </summary>
    public void InitializeDeck(List<short> deckCardIds)
    {
        _playerDeck.Clear();
        _playerHand.Clear();
        _playerDiscardPile.Clear();

        if (_cardDatabase == null)
        {
            Debug.LogError("Cannot initialize deck, CardDatabase is missing.");
            return;
        }
        if (deckCardIds == null || deckCardIds.Count == 0)
        {
            Debug.LogError("Cannot initialize deck, received empty or null deckCardIds list.");
            return;
        }

        Debug.Log($"Initializing InGame Deck with {deckCardIds.Count} cards.");
        foreach (short cardId in deckCardIds)
        {
            CardData cardData = _cardDatabase.GetCardDataById(cardId);
            if (cardData != null)
            {
                _playerDeck.Add(cardData);
                // Debug.Log($"Added to deck: {cardData.cardName} (ID: {cardData.cardId})");
            }
            else
            {
                Debug.LogWarning($"CardData for ID {cardId} not found in database. Skipping.");
            }
        }
    }

    /// <summary>
    /// ���� ���� �����ϴ�.
    /// </summary>
    public void ShuffleDeck()
    {
        if (_playerDeck == null || _playerDeck.Count == 0)
        {
            Debug.LogWarning("Deck is empty or null, cannot shuffle.");
            return;
        }

        System.Random rng = new System.Random();
        _playerDeck = _playerDeck.OrderBy(a => rng.Next()).ToList();
        Debug.Log("Player deck shuffled.");
    }

    /// <summary>
    /// ������ ����ŭ ������ ī�带 �̾� �ڵ�� �����ɴϴ�.
    /// ���� ��� ������ ī�带 ���� ������ ������ �� �ֽ��ϴ� (������).
    /// </summary>
    public void DrawCards(int amountToDraw)
    {
        if (_playerDeck == null)
        {
            Debug.LogError("PlayerDeck is null. Cannot draw cards.");
            return;
        }

        for (int i = 0; i < amountToDraw; i++)
        {
            if (_playerDeck.Count == 0)
            {
                // ���� ����� �� ó�� (��: ���� ī�� ���̸� ��� ������)
                if (_playerDiscardPile.Count > 0)
                {
                    Debug.Log("Deck is empty. Shuffling discard pile into deck.");
                    _playerDeck.AddRange(_playerDiscardPile);
                    _playerDiscardPile.Clear();
                    ShuffleDeck(); // �� �� ����
                    // OnDeckChanged �̺�Ʈ�� ShuffleDeck ���ο��� ȣ���
                }
                else
                {
                    Debug.LogWarning("Deck is empty and discard pile is also empty. Cannot draw more cards.");
                    break; // �� �̻� ���� ī�� ����
                }
            }

            // ���� ������ ����ִٸ� (���� ī�嵵 �����ٸ�) �ߴ�
            if (_playerDeck.Count == 0) break;


            CardData drawnCard = _playerDeck[0];
            _playerDeck.RemoveAt(0);
            _playerHand.Add(drawnCard); 
        }
        Debug.Log($"Drew {amountToDraw} cards (or less if deck empty). Hand size: {_playerHand.Count}");
    }

    /// <summary>
    /// ���� ���� �� �ʱ� �ڵ带 �̽��ϴ�.
    /// </summary>
    public void DrawInitialHand()
    {
        Debug.Log($"Drawing initial hand of {InitialHandSize} cards.");
        DrawCards(InitialHandSize); // ���������� OnCardDrawn�� ȣ������ �ʵ��� DrawCards ����

        // �ʱ� �ڵ� ��ο찡 ��� �������� �˸�
        OnInitialHandDrawn?.Invoke();
    }

    // ���� �� ī�� �� �� �̴� �Լ�
    public void DrawOneCard()
    {
        if (_playerDeck.Count == 0)
        {
            // TODO �� ���� ó�� ...
            return;
        }
        CardData drawnCard = _playerDeck[0];
        _playerDeck.RemoveAt(0);
        _playerHand.Add(drawnCard);
        OnCardDrawn?.Invoke(drawnCard); // ���� �߿��� �� �徿 �̺�Ʈ �߻�
    }

    // �ڵ忡�� ī�� ��� �Լ�
    public bool PlayCardFromHand(CardData cardToPlay)
    {
        if (_playerHand.Contains(cardToPlay))
        {
            // ���� ī�� ��� ���� (�ڿ� �Ҹ�, ȿ�� �ߵ� ��)�� GameManager�� �ٸ� ������ ó��
            _playerHand.Remove(cardToPlay);
            _playerDiscardPile.Add(cardToPlay);
            OnCardPlayed?.Invoke(cardToPlay); // ����� ī�� ������ �̺�Ʈ�� ����
            return true;
        }
        Debug.LogWarning($"Card {cardToPlay.cardName} not found in hand.");
        return false;
    }
}