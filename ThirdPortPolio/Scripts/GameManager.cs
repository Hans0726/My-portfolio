using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // �κ񿡼� �ΰ������� ������ �� ����
    private List<short> _playerDeckToCarryOver;
    public List<short> PlayerDeckToCarryOver => _playerDeckToCarryOver;


    [Header("In-Game Start Animation UI")]
    public GameObject startSequencePanel;           // ������ ������ �г� (�ν����Ϳ��� �Ҵ�)
    public TextMeshProUGUI startSequenceText;       // "���� �ð� ����..." �ؽ�Ʈ (�ν����Ϳ��� �Ҵ�)
                                                    // Canvas Group�� ����ϸ� �гΰ� �ؽ�Ʈ ���ĸ� �� ���� �����ϱ� ����
    public CanvasGroup startSequenceCanvasGroup;    // �г��� Canvas Group (���� �����)
    public float startSequenceFadeDuration = 1.0f;
    public float startSequenceDisplayDuration = 2.0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // �κ񿡼� �� ���� �Ϸ� �� ȣ��
    public void SetDeckForNextGame(List<short> deckCardIds)
    {
        _playerDeckToCarryOver = new List<short>(deckCardIds); // ����� ����
        Debug.Log($"GameManager: Deck set for next game with {_playerDeckToCarryOver.Count} cards.");
    }

    public void MatchingSuccess()
    {
        // �κ񿡼� ���� �� ������ �����ͼ� ���� (����: LobbyCardManager���� ȣ��)
        if (LobbyCardManager.Instance != null)
        {
            SetDeckForNextGame(LobbyCardManager.Instance.CurrentDeckCardIds);
        }
        else
        {
            Debug.LogError("LobbyCardManager instance not found when trying to set deck for game!");
        }

        UIPopup_Matching.MatchingEvent.TriggerMatchingStatusChanged("��Ī ����");
        StartCoroutine(LoadInGameSceneAndInitialize()); // �Լ� �̸� ����
    }

    public void MatchingReqOk()
    {
        UIPopup_Matching.MatchingEvent.TriggerMatchingStatusChanged("��Ī ����");
    }

    private IEnumerator LoadInGameSceneAndInitialize()
    {
        Debug.Log("Matching success. Moving to InGame scene in 3 seconds...");
        yield return new WaitForSeconds(3f); // ��Ī ���� UI ǥ�� �ð� ��

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("B_InGame"); // �� �̸� Ȯ��
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        Debug.Log("InGame scene loaded.");


        // 1. ������ �Ŵ���(CardManager) ���� �ʱ�ȭ
        if (InGameCardManager.Instance != null)
        {
            InGameCardManager.Instance.Initialize(_playerDeckToCarryOver);
        }
        else Debug.LogError("InGameCardManager instance not found!");


        // 2. UI �Ŵ��� �ʱ�ȭ (�̺�Ʈ ����)
        if (InGameUIManager.Instance != null)
        {
            InGameUIManager.Instance.Initialize();
        }
        else Debug.LogError("InGameUIManager instance not found!");


        // 3. ������ ������ ����
        // ������ �������� ������ �� OnComplete �ݹ鿡�� InGameCardManager.Instance.DrawInitialHand()�� ȣ��
        if (InGameUIManager.Instance != null)
        {
            InGameUIManager.Instance.ShowOpeningSequence(); // ShowOpeningSequence ���ο��� DrawInitialHand�� Ʈ�����ϵ��� ����
        }
    }
}