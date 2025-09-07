using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

public class UIPopup_Deck : UIPopup
{
    public const int MAX_NUM_CARDS = 15;

    [Header("[CardList Display]")]
    [SerializeField] private Transform _ownedCardContainer;
    [SerializeField] private Transform _deckCardContainer;

    [Header("[Card Prefab]")]
    [SerializeField] private GameObject _cardUIRootPrefab; // CardUI�� ��Ʈ ������

    [Header("[Filter Buttons]")]
    [SerializeField] private Button _btnShowAll;
    [SerializeField] private Button _btnShowAttackCard;
    [SerializeField] private Button _btnShowDefenseCard;

    [Header("[Pagination]")]
    [SerializeField] private Button _leftArrowButton;
    [SerializeField] private Button _rightArrowButton;
    private const int CARDS_PER_PAGE_OWNED = 8;
    private int _currentPageOwned = 0;

    [Header("[Deck Info]")]
    [SerializeField] private TextMeshProUGUI _txtNumCurrentCard;
    [SerializeField] private TextMeshProUGUI _txtNumTotalCard;

    private List<GameObject> _cardUIRootPool = new List<GameObject>();

    // Ȱ��ȭ�� CardUI�� ��Ʈ ������Ʈ�� CardData�� ���� (���� ���� �� ���� ������)
    private Dictionary<CardData, GameObject> _activeCardRootsMap = new Dictionary<CardData, GameObject>();

    private CardType _currentFilterType = CardType.UnDefined;
    protected override void Start()
    {
        base.Start();
        // InitializeObjectPool�� LobbyCardManager�� �����Ͱ� �غ�� �� ȣ��ǵ��� ���� ���
        // �Ǵ� LobbyCardManager���� ������ �ε� �Ϸ� �̺�Ʈ�� �߻����� �׶� �ʱ�ȭ
        // ���⼭�� OnEnable���� InitialDisplay�� ȣ��� �� Ǯ�� ��������� ä��� ������� ����

        _btnShowAll.onClick.AddListener(() => { _currentFilterType = CardType.UnDefined; _currentPageOwned = 0; RefreshAllCardDisplays(); });
        _btnShowAttackCard.onClick.AddListener(() => { _currentFilterType = CardType.Attack; _currentPageOwned = 0; RefreshAllCardDisplays(); });
        _btnShowDefenseCard.onClick.AddListener(() => { _currentFilterType = CardType.Defense; _currentPageOwned = 0; RefreshAllCardDisplays(); });
        _btnClose.onClick.AddListener(LobbyCardManager.Instance.SendUpdatedDeckToServer);

        _leftArrowButton.onClick.AddListener(OnLeftArrowClick);
        _rightArrowButton.onClick.AddListener(OnRightArrowClick);

        _txtNumTotalCard.text = $"/ {LobbyCardManager.Instance.OwnedPlayerCards.Count}";

        if (LobbyCardManager.Instance != null)
        {
            LobbyCardManager.Instance.OnDeckCompositionChanged += HandleDeckCompositionChanged;
        }
    }

    private void OnDestroy()
    {
        if (LobbyCardManager.Instance != null)
        {
            LobbyCardManager.Instance.OnDeckCompositionChanged -= HandleDeckCompositionChanged;
        }
        foreach (var cardUIRoot in _cardUIRootPool)
        {
            if (cardUIRoot != null) Destroy(cardUIRoot);
        }
        _cardUIRootPool.Clear();
        _activeCardRootsMap.Clear();
    }

    private void OnEnable()
    {
        _currentPageOwned = 0;
        ObjectPoolInitialized(); // Ǯ �ʱ�ȭ ����
        RefreshAllCardDisplays();    // �˾� Ȱ��ȭ �� ��ü UI ���� ����
    }

    private void ObjectPoolInitialized()
    {
        if (LobbyCardManager.Instance == null || LobbyCardManager.Instance.OwnedPlayerCards == null) // OwnedPlayerCards�� ��� ī�� ������ ����Ʈ
        {
            Debug.LogError("[ObjectPoolInitialized] LobbyCardManager or its master card list is null.");
            return;
        }

        // Ǯ ũ��� ���� �� ��� ī�� ������ ����ŭ �ʿ�
        int requiredPoolSize = LobbyCardManager.Instance.OwnedPlayerCards.Count;

        if (_cardUIRootPool.Count < requiredPoolSize)
        {
            Debug.Log($"[ObjectPoolInitialized] Current pool size: {_cardUIRootPool.Count}, Required: {requiredPoolSize}. Expanding pool.");
            for (int i = _cardUIRootPool.Count; i < requiredPoolSize; i++)
            {
                GameObject cardRootGO = Instantiate(_cardUIRootPrefab, this.transform);
                cardRootGO.name = $"Pooled_CardUI_{_cardUIRootPool.Count}";
                cardRootGO.SetActive(false);
                _cardUIRootPool.Add(cardRootGO);
            }
        }

        // _activeCardRootsMap ä��� ���� (��� ī�� �����Ϳ� ���� UI ��Ʈ �̸� ���� �� ����)
        if (_activeCardRootsMap.Count != requiredPoolSize)
        {
            foreach (var go in _activeCardRootsMap.Values) go.SetActive(false); // ���� �� ��ü ��Ȱ��ȭ
            _activeCardRootsMap.Clear();
            foreach (var pooledGO in _cardUIRootPool) pooledGO.SetActive(false);

            for (int i = 0; i < LobbyCardManager.Instance.OwnedPlayerCards.Count; i++)
            {
                CardData cardData = LobbyCardManager.Instance.OwnedPlayerCards[i]; // ��� ī�� ������ ��ȸ
                if (i < _cardUIRootPool.Count)
                {
                    GameObject cardRootGO = _cardUIRootPool[i];
                    CardUI cardUI = cardRootGO.GetComponentInChildren<CardUI>();
                    if (cardUI != null)
                    {
                        cardUI.InitializeDisplay(cardData);
                        cardUI.OnOwnedCardClicked = HandleOwnedCardClicked; // ���� ��Ͽ��� Ŭ��
                        cardUI.OnDeckCardClicked = HandleDeckCardClicked;   // ������ ��Ͽ��� Ŭ��
                        _activeCardRootsMap[cardData] = cardRootGO; // ��� ī�忡 ���� ����
                    }
                    else Debug.LogError($"CardUI component not found on pooled object for {cardData.cardName}");
                }
                else
                {
                    Debug.LogError("Not enough objects in pool during EnsureObjectPoolInitialized. This shouldn't happen if pool size is correct.");
                }
            }
        }
    }


    // UI�� �� ���� ��� �����ϴ� �Լ� (�ʱ�ȭ, ����/������ ���� ��)
    private void RefreshAllCardDisplays()
    {
        if (LobbyCardManager.Instance == null) return;

        // ��� _activeCardRootsMap�� UI���� �ϴ� ��Ȱ��ȭ�ϰ� Ǯ �����̳ʷ� (����)
        foreach (var kvp in _activeCardRootsMap)
        {
            if (kvp.Value != null)
            {
                kvp.Value.SetActive(false);
                kvp.Value.transform.SetParent(this.transform, false);
            }
        }

        // === ���� ī�� ��� �׸��� ���� ===
        List<CardData> cardsForOwnedDisplay = GetFilteredOwnedCardsForDisplay();
        int startIndex = _currentPageOwned * CARDS_PER_PAGE_OWNED;
        int endIndex = Mathf.Min(startIndex + CARDS_PER_PAGE_OWNED, cardsForOwnedDisplay.Count);

        for (int i = 0; i < (endIndex - startIndex); i++) // ���� ǥ���� ������ŭ�� ����
        {
            CardData cardData = cardsForOwnedDisplay[startIndex + i];
            if (_activeCardRootsMap.TryGetValue(cardData, out GameObject cardRootGO) && cardRootGO != null)
            {
                cardRootGO.transform.SetParent(_ownedCardContainer, false);
                cardRootGO.transform.SetSiblingIndex(i); // ���� ī�� ��� �������� ����
                CardUI cardUI = cardRootGO.GetComponentInChildren<CardUI>();
                if (cardUI != null) cardUI.UpdateView(false);
                cardRootGO.SetActive(true);
            }
        }
        // === ���� ī�� ��� �׸��� �� ===

        // === �� ī�� ��� �׸��� ���� ===
        // `CurrentDeckCardIds`�� �̹� ID ������ ���ĵǾ� �ִٰ� �����ϰų�, ���⼭ `OrderBy` ���
        foreach (short cardId in LobbyCardManager.Instance.CurrentDeckCardIds.OrderBy(id => id))
        {
            CardData cardData = LobbyCardManager.Instance.OwnedPlayerCards.FirstOrDefault(c => c.cardId == cardId);
            if (cardData != null && _activeCardRootsMap.TryGetValue(cardData, out GameObject cardRootGO) && cardRootGO != null)
            {
                cardRootGO.transform.SetParent(_deckCardContainer, false);
                // SetSiblingIndex�� SortDeckContainerChildren���� ó���ϹǷ� ���⼭�� ���� ����
                CardUI cardUI = cardRootGO.GetComponentInChildren<CardUI>();
                if (cardUI != null) cardUI.UpdateView(true);
                cardRootGO.SetActive(true);
            }
        }
        SortDeckContainerChildren(); // �� ��� ���� ���� (�ʼ�)

        UpdateDeckCountText();
        UpdateArrowButtons(cardsForOwnedDisplay.Count);
    }


    // ���� ī�� ��Ͽ��� ī�� Ŭ�� ��
    private void HandleOwnedCardClicked(CardUI clickedCardUI)
    {
        CardData cardData = clickedCardUI.CurrentCardData;
        if (LobbyCardManager.Instance.TryAddCardToDeck(cardData.cardId))
        {
            // ���������� ���� �߰���
            if (_activeCardRootsMap.TryGetValue(cardData, out GameObject cardRootGO))
            {
                cardRootGO.transform.SetParent(_deckCardContainer, false);
                clickedCardUI.UpdateView(true); // �̴� ī�� ������� ����

                RefreshOwnedCardsDisplay();  // ���� ī�� ��Ͽ����� ���� ������ �ʾƾ� �ϹǷ�, �ش� �������� �ٽ� �׷��� ����
                SortDeckContainerChildren();
            }
            // �� ī��Ʈ ������Ʈ�� OnDeckCompositionChanged �ڵ鷯�� ���
        }
    }

    // �� ��Ͽ��� ī�� Ŭ�� ��
    private void HandleDeckCardClicked(CardUI clickedCardUI)
    {
        CardData cardDataToRemove = clickedCardUI.CurrentCardData;
        if (LobbyCardManager.Instance.TryRemoveCardFromDeck(cardDataToRemove.cardId))
        {
            // ���������� ������ ���ŵ�
            if (_activeCardRootsMap.TryGetValue(cardDataToRemove, out GameObject cardRootGO))
            {
                // �θ� _ownedCardContainer�� �ű�� ����,
                // �� ī�尡 ���� ���� ī�� ����/�������� �´��� Ȯ���ϰ� �׿� ���� ó��
                cardRootGO.transform.SetParent(_ownedCardContainer, false);
                clickedCardUI.UpdateView(false); // ū ī�� ������� ����

                RefreshOwnedCardsDisplay();
                SortDeckContainerChildren();
            }
            // �� ī��Ʈ ������Ʈ�� OnDeckCompositionChanged �ڵ鷯�� ���
        }
    }

    private void SortDeckContainerChildren()
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in _deckCardContainer) // _deckCardContainer�� �������� �ڽĵ鸸 ������
        {
            children.Add(child);
        }

        // CardUI ������Ʈ �� CardData�� �������� ����
        children.Sort((t1, t2) => {
            CardUI cui1 = t1.GetComponentInChildren<CardUI>(true); // ��Ʈ�� �ڽĿ��� CardUI�� ã��
            CardUI cui2 = t2.GetComponentInChildren<CardUI>(true);

            if (cui1 != null && cui1.CurrentCardData != null && cui2 != null && cui2.CurrentCardData != null)
            {
                return cui1.CurrentCardData.cardId.CompareTo(cui2.CurrentCardData.cardId);
            }

            // CardUI�� CardData�� ���� ��� ���� ó�� (��: �ڷ� �����ų�, �α� ���)
            if (cui1 == null || cui1.CurrentCardData == null) return 1; // t1�� �ڷ�
            if (cui2 == null || cui2.CurrentCardData == null) return -1; // t2�� �ڷ�
            return 0;
        });

        // ���ĵ� ������� SiblingIndex �缳��
        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetSiblingIndex(i);
        }
    }

    private void HandleDeckCompositionChanged()
    {
        UpdateDeckCountText();
    }

    // ���� ī�� ��� ǥ�� ���� (����, ������ ���� ��)
    private void RefreshOwnedCardsDisplay()
    {
        if (LobbyCardManager.Instance == null) return;

        // ���� ȭ�鿡 ���̴� ���� ī�� UI�� ��Ȱ��ȭ (���� �ִ� ī��� �ǵ帮�� ����)
        foreach (CardData cardData in _activeCardRootsMap.Keys.ToList()) // ToList()�� ���纻 ��ȸ
        {
            if (!LobbyCardManager.Instance.CurrentDeckCardIds.Contains(cardData.cardId))
            {
                _activeCardRootsMap[cardData].SetActive(false);
                _activeCardRootsMap[cardData].transform.SetParent(this.transform); // Ǯ �����̳ʷ�
            }
        }

        List<CardData> filteredOwnedCardsToDisplay = GetFilteredOwnedCardsForDisplay();
        int startIndex = _currentPageOwned * CARDS_PER_PAGE_OWNED;
        int endIndex = Mathf.Min(startIndex + CARDS_PER_PAGE_OWNED, filteredOwnedCardsToDisplay.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            CardData cardData = filteredOwnedCardsToDisplay[i];
            if (_activeCardRootsMap.TryGetValue(cardData, out GameObject cardRootGO))
            {
                cardRootGO.transform.SetParent(_ownedCardContainer, false);
                
                CardUI cardUI = cardRootGO.GetComponentInChildren<CardUI>();
                cardUI.UpdateView(false); // ū ī�� ���
                cardRootGO.SetActive(true);
            }
        }
        UpdateArrowButtons(filteredOwnedCardsToDisplay.Count);
    }

    // GetFilteredOwnedCardsForDisplay: ���� ���� ���� ���� ī�� �� ���� ���Ϳ� �´� ī�常 ��ȯ
    private List<CardData> GetFilteredOwnedCardsForDisplay()
    {
        List<CardData> availableForDisplay = LobbyCardManager.Instance.OwnedPlayerCards
                                            .Where(c => !LobbyCardManager.Instance.CurrentDeckCardIds.Contains(c.cardId))
                                            .ToList();
        return _currentFilterType switch
        {
            CardType.Attack => availableForDisplay.Where(card => card.cardType == CardType.Attack).ToList(),
            CardType.Defense => availableForDisplay.Where(card => card.cardType == CardType.Defense).ToList(),
            _ => availableForDisplay
        };
    }

    private void UpdateDeckCountText()
    {
        _txtNumCurrentCard.text = LobbyCardManager.Instance.NumCardInDeck.ToString();
    }

    private void UpdateArrowButtons(int totalFilteredOwnedCards)
    {
        _leftArrowButton.gameObject.SetActive(_currentPageOwned > 0);
        _rightArrowButton.gameObject.SetActive((_currentPageOwned + 1) * CARDS_PER_PAGE_OWNED < totalFilteredOwnedCards);
    }

    void OnLeftArrowClick()
    {
        if (_currentPageOwned > 0)
        {
            _currentPageOwned--;
            RefreshOwnedCardsDisplay(); // ���� ī�� ��ϸ� ������ ����
        }
    }

    void OnRightArrowClick()
    {
        List<CardData> filteredOwnedCardsToDisplay = GetFilteredOwnedCardsForDisplay();
        if ((_currentPageOwned + 1) * CARDS_PER_PAGE_OWNED < filteredOwnedCardsToDisplay.Count)
        {
            _currentPageOwned++;
            RefreshOwnedCardsDisplay(); // ���� ī�� ��ϸ� ������ ����
        }
    }
}