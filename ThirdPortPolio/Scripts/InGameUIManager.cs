using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // ������ ���� �Ҹ� �ִϸ��̼� � ��� ����
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager Instance { get; private set; }

    // --- ���� ������ ���� enum ---
    public enum HandState
    {
        Idle,           // �ƹ��͵� �� �ϴ� ����
        InitialDrawing, // �ʱ� ��ο� �ִϸ��̼� ��
        InInteraction   // �Ϲ����� ��ȣ�ۿ� ���� (LateUpdate ����)
    }
    private HandState _currentHandState = HandState.Idle;

    [Header("Opening Sequence UI")]
    [SerializeField] private CanvasGroup _openingSequenceCanvasGroup;
    [SerializeField] private TextMeshProUGUI _openingSequenceText;
    [SerializeField] private float _openingFadeDuration = 1.0f;
    [SerializeField] private float _openingDisplayDuration = 2.0f;

    [Space(20)]
    [Header("References")]
    [SerializeField] private Transform _handContainer;
    [SerializeField] private GameObject _cardUIPrefab;
    [SerializeField] private RectTransform _deckPosition;
    [SerializeField] private RectTransform _dropZone; // �巡�� ��� �Ǵ� ����
    [SerializeField] private Canvas _mainCanvas;
    [SerializeField] private TextMeshProUGUI _costText;
    

    [Header("Layout Settings")]
    [SerializeField] private float _spreadAngle = 10f;
    [SerializeField] private float _cardSpacing = 100f;
    [SerializeField] private float _baseYPosition = 150f;
    [SerializeField] private float _collapsedYPosition = -50f;
    [SerializeField] private float _hoverScaleMultiplier = 1.2f;
    [SerializeField] private float _hoverYOffset = 50f;
    [SerializeField] private float _arcCorrectionFactor = 5f; // ��ġ ��� ���� ���

    [Header("Animation Settings")]
    [SerializeField] private Vector2 _expandedCardScale = Vector2.one;
    [SerializeField] private Vector2 _collapsedCardScale = new Vector2(0.8f, 0.8f);
    [SerializeField] private float _lerpSpeed = 10f;

    // --- ���� ���� ---
    private CardUI _hoveredCard = null;
    private CardUI _draggedCard = null;
    private bool _isHandExpanded = false;

    // ������Ʈ Ǯ��
    [SerializeField] private Transform _cardPoolContainer;
    private List<GameObject> _cardUIPool = new List<GameObject>();
    private List<GameObject> _activeHandCardRoots = new List<GameObject>();
    private const int INITIAL_POOL_SIZE = 15; // �ִ� �ڵ� �� + ������


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (GameTurnManager.Instance != null)
        {
            GameTurnManager.Instance.OnCostChanged += UpdateCardInteractableStates;
        }
    }

    void OnDestroy()
    {
        if (GameTurnManager.Instance != null)
        {
            GameTurnManager.Instance.OnCostChanged -= UpdateCardInteractableStates;
        }
    }

    void Start()
    {
        // GameManager�� ���ٴ� ���� �� ������ �ٷ� �����ߴٴ� �ǹ�
        if (GameManager.Instance == null)
        {
            // --- �׽�Ʈ ��� �ʱ�ȭ �帧 ---
            Debug.LogWarning("--- RUNNING IN TEST MODE ---");

            // 1. ������ �Ŵ���(CardManager) �׽�Ʈ �ʱ�ȭ
            if (InGameCardManager.Instance != null)
            {
                InGameCardManager.Instance.TestInitialize(); // �׽�Ʈ �� ���� �� ����
            }
            else { Debug.LogError("TEST MODE: InGameCardManager instance not found!"); }

            // 2. UI �Ŵ��� �ʱ�ȭ (�̺�Ʈ ����)
            Initialize(); // �ڽ��� �ʱ�ȭ �Լ� ȣ��

            // 3. ������ ������ ����
            // ������ �������� ������ DrawInitialHand�� ȣ��Ǿ�� ��
            ShowOpeningSequence();
        }
        // else: ���� ��忡���� GameManager�� LoadInGameSceneAndInitialize����
        // �� �Ŵ����� Initialize �Լ��� ������� ȣ���� �� ���̹Ƿ�, ���⼭�� �ƹ��͵� �� ��.
    }

    public void Initialize()
    {
        Debug.Log("[InGameUIManager] Initializing...");
        InitializeObjectPool();

        if (InGameCardManager.Instance != null)
        {
            InGameCardManager.Instance.OnInitialHandDrawn += HandleInitialHandDrawn;
        }
        else
        {
            Debug.LogError("[InGameUIManager] InGameCardManager.Instance is null during Initialize!");
        }
    }

    #region UI Initialization
    public void ShowOpeningSequence()
    {
        StartCoroutine(OpeningSequenceCoroutine());
    }

    private IEnumerator OpeningSequenceCoroutine()
    {
        if (_openingSequenceCanvasGroup == null || _openingSequenceText == null)
        {
            Debug.LogError("Opening sequence UI elements are not assigned in InGameUIManager!");
            yield break;
        }

        _openingSequenceCanvasGroup.alpha = 0f;
        _openingSequenceCanvasGroup.gameObject.SetActive(true);
        _openingSequenceText.text = "���� �ð� ���� ������ �غ��ϼ���!";

        Debug.Log("InGameUIManager: Opening Sequence Fading In");
        _openingSequenceCanvasGroup.DOFade(1f, _openingFadeDuration);
        yield return new WaitForSeconds(_openingFadeDuration);

        Debug.Log("InGameUIManager: Opening Sequence Displaying");
        yield return new WaitForSeconds(_openingDisplayDuration);

        Debug.Log("InGameUIManager: Opening Sequence Fading Out");
        _openingSequenceCanvasGroup.DOFade(0f, _openingFadeDuration).OnComplete(() =>
        {
            _openingSequenceCanvasGroup.gameObject.SetActive(false);
            Debug.Log("InGameUIManager: Opening Sequence Finished");
            if (InGameCardManager.Instance != null)
            {
                InGameCardManager.Instance.DrawInitialHand();
            }
        });
    }

    // �ڡڡ� �ʱ� ��ο� ó�� �Լ� (DOTween Sequence�� ���) �ڡڡ�
    private void HandleInitialHandDrawn()
    {
        _currentHandState = HandState.InitialDrawing; // ���� ����

        List<CardData> initialHandData = InGameCardManager.Instance.PlayerHand;

        // ������ Ȱ��ȭ�� ī�尡 �ִٸ� ����
        foreach (var go in _activeHandCardRoots) ReturnCardUIRootToPool(go);
        _activeHandCardRoots.Clear();

        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < initialHandData.Count; i++)
        {
            CardUI newCardUI = AddCardToHandView(initialHandData[i]);
            GameObject newCardGO = newCardUI.RootGameObject;
            Transform cardTransform = newCardGO.transform;

            cardTransform.SetParent(_handContainer, false);
            cardTransform.position = _deckPosition.position;
            cardTransform.localScale = Vector3.zero;
            newCardGO.SetActive(true);

            // �߾ӿ� ���̴� �ִϸ��̼�
            seq.Insert(i * 0.1f, cardTransform.DOMove(_handContainer.position, 0.5f).SetEase(Ease.OutQuad));
            seq.Insert(i * 0.1f, cardTransform.DOScale(_collapsedCardScale, 0.5f));
        }

        seq.OnComplete(() =>
        {
            Debug.Log("Initial draw animation (to center) finished.");
            _currentHandState = HandState.InInteraction; // ���� LateUpdate ���� ����
        });
    }
    #endregion

    #region Card Pooling
    private void InitializeObjectPool()
    {
        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
        {
            GameObject cardRootGO = Instantiate(_cardUIPrefab, _cardPoolContainer);
            cardRootGO.name = $"Pooled_CardUI_{i}";
            cardRootGO.SetActive(false);
            _cardUIPool.Add(cardRootGO);
        }
    }

    private GameObject GetCardUIRootFromPool()
    {
        GameObject cardRootInstance = _cardUIPool.FirstOrDefault(go => !go.activeSelf);
        if (cardRootInstance == null)
        {
            cardRootInstance = Instantiate(_cardUIPrefab, _cardPoolContainer);
            _cardUIPool.Add(cardRootInstance);
            Debug.LogWarning("CardUI Pool extended.");
        }

        return cardRootInstance;
    }

    private void ReturnCardUIRootToPool(GameObject cardRoot)
    {
        if (cardRoot != null)
        {
            cardRoot.SetActive(false);
            cardRoot.transform.SetParent(_cardPoolContainer, false);
        }
    }
    #endregion

    private CardUI AddCardToHandView(CardData drawnCard)
    {
        GameObject cardRootGO = GetCardUIRootFromPool();

        CardUI cardUI = cardRootGO.GetComponentInChildren<CardUI>();
        if (cardUI != null)
        {
            cardUI.InitializeDisplay(drawnCard); // uiManager ���� ���� ����
            _activeHandCardRoots.Add(cardRootGO);
        }
        else
        {
            ReturnCardUIRootToPool(cardRootGO);
            return null;
        }

        return cardUI;
    }

    // ī�� ���� ó��
    private void RemoveCardFromHandView(GameObject playedCard)
    {
        _activeHandCardRoots.Remove(playedCard);

        // ������� �ִϸ��̼� �� Ǯ�� ��ȯ
        playedCard.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
            .OnComplete(() => ReturnCardUIRootToPool(playedCard));
    }

    void LateUpdate()
    {
        if (_currentHandState != HandState.InInteraction) return;

        // --- 1. ���콺 �Ʒ��� �ִ� '�ڵ� ī��' ã�� ---
        _hoveredCard = null; // �� ������ �ʱ�ȭ
        PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            CardUI cardUI = result.gameObject.GetComponentInParent<CardUI>();
            // ����ĳ��Ʈ�� UI�� _activeHandCardRoots�� ���Ե� ������ Ȯ�� (�ڵ� ī������ �Ǻ�)
            if (cardUI != null && _activeHandCardRoots.Contains(cardUI.RootGameObject))
            {
                _hoveredCard = cardUI;
                break; // ���� ���� �ִ� ī�� �ϳ��� ����
            }
        }

        // --- 2. �ڵ��� ��ǥ ����(Ȯ��/���) ���� ---
        // �ڡڡ� �ٽ� ����: ȣ���� ī�尡 '�ϳ��� ������' �ڵ�� Ȯ��� ���°� ��ǥ �ڡڡ�
        bool targetExpandedState = (_hoveredCard != null || _draggedCard != null);

        // �ڡڡ� �ε巯�� ��ȯ�� ���� Lerp ��� (���� ���������� ��õ) �ڡڡ�
        // ���� ����(_isHandExpanded)�� ��ǥ ����(targetExpandedState)�� ���������� ������ �� ����
        // ������ ������ ��� �����ϴ� ���� �� ����
        _isHandExpanded = targetExpandedState;


        // --- 3. UI ������Ʈ ---
        AnimateHandToTargetState();
    }


    private void AnimateHandToTargetState()
    {
        int cardCount = _activeHandCardRoots.Count;

        // --- 1. ������ ����(Sibling Index) ���� ---
        // ��ο� ����(_activeHandCardRoots�� ����)�� �⺻ ������ ������ ���
        for (int i = 0; i < cardCount; i++)
        {
            _activeHandCardRoots[i].transform.SetSiblingIndex(i);
        }
        // ���� ȣ���� ī�尡 �ִٸ�, �� ī�常 �� ���� �ø�
        if (_isHandExpanded && _hoveredCard)
        {
            GameObject card = _hoveredCard.RootGameObject;
            card.transform.SetAsLastSibling();
        }

        // --- 2. ���̾ƿ� ��� �� �ִϸ��̼� ---
        float startAngle = -(cardCount - 1) / 2.0f * _spreadAngle;
        float startX = -(cardCount - 1) / 2.0f * _cardSpacing;

        for (int i = 0; i < cardCount; i++)
        {
            GameObject cardRootGO = _activeHandCardRoots[i]; // ��ο� ������� ������
            Transform cardTransform = cardRootGO.transform;
            CardUI cardUI = cardRootGO.GetComponentInChildren<CardUI>();

            // ��ǥ ��ġ/ȸ��/ũ�� ���
            float targetAngle = startAngle + i * _spreadAngle;
            float targetX = startX + i * _cardSpacing;

            float cardHeight = cardUI.RectTransform.rect.height;
            float rotationRadius = cardHeight * 0.5f; // ȸ�� ������
            float radianAngle = Mathf.Abs(targetAngle) * Mathf.Deg2Rad;
            float yRiseDueToRotation = (1 - Mathf.Cos(radianAngle)) * rotationRadius;

            float targetY;

            if (_isHandExpanded)
            {
                targetY = _baseYPosition - (yRiseDueToRotation * _arcCorrectionFactor);
                if (cardUI == _hoveredCard)
                {
                    targetY += _hoverYOffset;
                }
            }
            else // ��ҵ� ������ ��
            {
                // ��� �ÿ��� ��ä�� ����� �����ϵ�, ��ü���� ���� ���̸� ����
                targetY = _collapsedYPosition - (yRiseDueToRotation * _arcCorrectionFactor);
            }


            // ȸ�� �� ũ�� ���� (������ ����)
            Quaternion targetRotation = Quaternion.Euler(0, 0, -targetAngle);
            Vector2 targetScale = _isHandExpanded ? _expandedCardScale : _collapsedCardScale;
            if (_isHandExpanded && cardUI == _hoveredCard)
            {
                targetScale *= _hoverScaleMultiplier;
                targetRotation = Quaternion.identity;
            }

            Vector3 targetPosition = new Vector3(targetX, targetY, 0);

            // Lerp�� �ε巴�� �̵�
            cardTransform.localPosition = Vector3.Lerp(cardTransform.localPosition, targetPosition, Time.deltaTime * _lerpSpeed);
            cardTransform.localRotation = Quaternion.Slerp(cardTransform.localRotation, targetRotation, Time.deltaTime * _lerpSpeed);
            cardTransform.localScale = Vector3.Lerp(cardTransform.localScale, targetScale, Time.deltaTime * _lerpSpeed);
        }
    }

    public void OnCardBeginDrag(CardUI cardUI)
    {
        _activeHandCardRoots.Remove(cardUI.RootGameObject); // �ڵ忡�� ��� ����
        _draggedCard = cardUI;
        _draggedCard.RootGameObject.transform.SetParent(_mainCanvas.transform, true); // ������ �ֻ�����
        _draggedCard.RootGameObject.transform.rotation = Quaternion.identity; // ȸ�� �ʱ�ȭ

        _draggedCard.CanvasGroup.blocksRaycasts = false;
    }

    public void OnCardDrag(PointerEventData eventData)
    {
        if (_draggedCard != null)
        {
            _draggedCard.RootGameObject.transform.position = eventData.position;
        }
    }

    public void OnCardEndDrag(CardUI cardUI, PointerEventData eventData)
    {
        if (_draggedCard == null) return;
        _draggedCard.CanvasGroup.blocksRaycasts = true;

        // ��� ���� Ȯ��
        if (RectTransformUtility.RectangleContainsScreenPoint(_dropZone, eventData.position))
        {
            Debug.Log($"Card {_draggedCard.CurrentCardData.cardName} Played!");
            // ���� ī�� ��� ����
            InGameCardManager.Instance.PlayCardFromHand(_draggedCard.CurrentCardData);
            RemoveCardFromHandView(_draggedCard.RootGameObject);
        }
        else
        {
            // �ڵ�� ����
            _activeHandCardRoots.Add(cardUI.RootGameObject);
            _draggedCard.transform.SetParent(_handContainer, true);
            bool canAfford = GameTurnManager.Instance.CurrentCost >= _draggedCard.CurrentCardData.cost;
            _draggedCard.SetPlayableState(canAfford);
        }
        _draggedCard = null;
    }

    private void UpdateCardInteractableStates(int currentCost)
    {
        foreach (var cardRootGO in _activeHandCardRoots)
        {
            CardUI cardUI = cardRootGO.GetComponent<CardUI>();
            if (cardUI != null && cardUI.CurrentCardData != null)
            {
                bool canAfford = currentCost >= cardUI.CurrentCardData.cost;
                cardUI.SetPlayableState(canAfford);
            }
        }
    }
}