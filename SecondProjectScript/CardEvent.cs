using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[SerializeField]
public class CardEvent : MonoBehaviour
{
    public Deck deckFluorite;
    public Deck deckBronze;
    public Deck deckSilver;
    public Deck deckGold;
    public Deck deckPlatinum;
    public Card emptyCard;

    public PlayerStatus playStatus;
    public Image[] cardFlame;
    public Image[] cardArtwork;
    public Text[] cardTitle;
    public Text[] cardDescription;
    Card[] card;
    private void Awake()
    {
        card = new Card[3];
    }

    // �������Ͽ� ī�� �̺�Ʈ�� �߻� ��, �� ���� ī�忡 �������� ������ ī�尡 �Ҵ��, ���� �ߺ����� �����ٸ� �ٽ� ���Ͽ� �Ҵ�
    void OnEnable()
    {
        
        GameManager.instance.PauseGame();

        for (int i = 0; i < 3; i++)
        {
            UIManager.instance.cardButtons[i].SetActive(true);
            card[i] = SetCard(playStatus);

            for (int j = 0; j < i; j++) // �ߺ� üũ
            {
                if (card[i].id == -1)
                    break;

                if (card[i].rank == card[j].rank && card[i].id == card[j].id)
                {
                    switch (card[i].rank)   // �ߺ��̶�� ������ ������ ī�带 �ٽ� �ְ�
                    {
                        case Rank.Bronze:
                            deckBronze.cardList.Add(card[i]);
                            break;
                        case Rank.Silver:
                            deckSilver.cardList.Add(card[i]);
                            break;
                        case Rank.Gold:
                            deckGold.cardList.Add(card[i]);
                            break;
                        case Rank.Platinum:
                            deckPlatinum.cardList.Add(card[i]);
                            break;
                    }
                    card[i] = SetCard(playStatus);  // �ٽ� ��Ʈ
                    j = -1;
                }
            }

            cardArtwork[i].sprite = card[i].artwork;
            cardFlame[i].sprite = card[i].cardFlame;
            cardTitle[i].text = card[i].cardTitle;
            cardDescription[i].text = card[i].cardDescription;
        }
    }
    void OnDisable()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
           transform.GetChild(i).gameObject.SetActive(true);
    }

    // ���õ� ī���� ȿ�� �ߵ��� ���� �̹��� ȿ��
    public void OnCardSelected()
    {
        for (int i = 0;i < 3;i++)
            UIManager.instance.cardButtons[i].SetActive(false);
        SoundManager.instance.audioSourceEFX.PlayOneShot(SoundManager.instance.cardSelect);
        Transform selectedCard = EventSystem.current.currentSelectedGameObject.transform.parent;
        int selectedCardNum = int.Parse(selectedCard.name.Substring(4));
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i != selectedCardNum)   // ���õ� ī�尡 �ƴϸ� �ٽ� ���� �־��ְ� ��Ȱ��ȭ
            {
                switch(card[i].rank)
                {
                    case Rank.Bronze:
                        deckBronze.cardList.Add(card[i]);          
                        break;
                    case Rank.Silver:
                        deckSilver.cardList.Add(card[i]);
                        break;
                    case Rank.Gold:
                        deckGold.cardList.Add(card[i]);
                        break;
                    case Rank.Platinum:
                        deckPlatinum.cardList.Add(card[i]);
                        break;
                }
                transform.GetChild(i).gameObject.SetActive(false);
            }
                
        }

        switch (card[selectedCardNum].rank)
        {
            case Rank.Bronze:
                GameManager.instance.playerDeckInfo[0].cardInfoList.Add(card[selectedCardNum].id);
                deckBronze.cardList.Remove(card[selectedCardNum]);   
                break;
            case Rank.Silver:
                GameManager.instance.playerDeckInfo[1].cardInfoList.Add(card[selectedCardNum].id);
                deckSilver.cardList.Remove(card[selectedCardNum]);
                break;
            case Rank.Gold:
                GameManager.instance.playerDeckInfo[2].cardInfoList.Add(card[selectedCardNum].id);
                deckGold.cardList.Remove(card[selectedCardNum]);
                break;
            case Rank.Platinum:
                GameManager.instance.playerDeckInfo[3].cardInfoList.Add(card[selectedCardNum].id);
                deckPlatinum.cardList.Remove(card[selectedCardNum]);
                break;
        }
        GameManager.instance.ResumeGame();
        StartCoroutine(SetActiveFalseObject(selectedCard));
        CardEffect(card[selectedCardNum]);
        UIManager.instance.SetPlayerStatusTexts(playStatus);
    }
    IEnumerator SetActiveFalseObject(Transform obj)
    {
        yield return new WaitForSeconds(2f);
        obj.gameObject.SetActive(false);
        transform.gameObject.SetActive(false);
    }

    // card effect
    void CardEffect(Card card)
    {
        if (card.cardTitle.Contains("ī�� ����"))
            return;
        switch (card.rank)
        {
            case Rank.Fluorite:
                switch (card.id)
                {
                    case 0:// ���� �п�
                        GameManager.instance.spawnRate *= 1.5f;
                        break;
                    case 1:// ����
                        GameManager.instance.spawnRate *= 0.5f;
                        break;
                    case 2:// ������
                        GameManager.instance.snailRate = 0.7f;
                        break;
                    case 3:// �������
                        GameManager.instance.DemoralizingRate = 0.7f;
                        break;
                    case 4:// ö��
                        GameManager.instance.tetkaiRate = 0.5f;
                        break;
                    case 5:// ���� ��ħ
                        OnDisable();
                        OnEnable();
                        break;
                }
                break;

            case Rank.Bronze:
                switch (card.id)
                {
                    case 0:// ����
                        playStatus.criticalRate = 10;
                        break;
                    case 1:// ������
                        playStatus.defense = 1;
                        break;
                    case 2:// �ʽ����� ����
                        playStatus.expRate *= 10;
                        playStatus.expRate *= 15;
                        playStatus.expRate /= 100;
                        break;
                    case 3:// ���� �˱�
                        playStatus.fastSwordTechnique = true;
                        break;
                }
                break;

            case Rank.Silver:
                switch (card.id)
                {
                    case 0:// ���� ����
                        playStatus.attackableNum++;
                        break;
                    case 1:// ���� ����
                        playStatus.healValue = 1;
                        break;
                    case 2:// ����Ʈ ����
                        playStatus.skillPoints += 2;
                        break;
                    case 3:// ��� ġ��
                        foreach (Player player in GameManager.instance.players)
                            player.hp += 5;
                        break;
                    case 4:// ����
                        playStatus.criticalRate = 15;
                        break;
                    case 5:// ������
                        playStatus.defense = 3;
                        break;
                    case 6:// ����
                        playStatus.bloodsucking = true;
                        break;
                }
                break;

            case Rank.Gold:
                switch (card.id)
                {
                    case 0:// ���� ����
                        playStatus.attackableNum += 2;
                        break;
                    case 1:// ���� ����
                        playStatus.healValue = 2;
                        break;
                    case 2:// ����Ʈ ����
                        playStatus.skillPoints += 3;
                        break;
                    case 3:// ��� ġ��
                        foreach (Player player in GameManager.instance.players)
                            player.hp += 10;
                        break;
                    case 4:// �ݰ�
                        playStatus.criticalRate = 20;
                        break;
                    case 5:// �ݹ���
                        playStatus.defense = 5;
                        break;
                    case 6:// ����ġ
                        playStatus.hedgehog = true;
                        break;
                }
                break;

            case Rank.Platinum:
                switch (card.id)
                {
                    case 0:// ���� ����
                        playStatus.attackableNum += 3;
                        break;
                    case 1:// ���� ����
                        playStatus.healValue = 3;
                        break;
                    case 2:// ����Ʈ ����
                        playStatus.skillPoints += 5;
                        break;
                    case 3:// ��� ġ��
                        foreach (Player player in GameManager.instance.players)
                            player.hp += 20;
                        break;
                    case 4:// ��ݰ�
                        playStatus.criticalRate = 30;
                        break;
                    case 5:// ��ݹ���
                        playStatus.defense = 10;
                        break;
                    case 6:// �нż�
                        GameManager.instance.placeableNum++;
                        break;
                }
                break;

        }
    }

    // ���� Ȯ���� ���� ī�尡 ������
    public Card SetCard(PlayerStatus status)
    {
        float fluorite = status.rankProbability[0],
            bronze = status.rankProbability[1],
            silver = status.rankProbability[2],
            gold = status.rankProbability[3];

        int choose = Random.Range(1, 101);

        Card card;

        if (choose <= fluorite) // �÷������Ʈ
            card = deckFluorite.cardList[Random.Range(0, deckFluorite.cardList.Count)];

        else if (choose <= fluorite + bronze)   // �����
        {
            if (deckBronze.cardList.Count == 0)
                card = emptyCard;
            else
            {
                card = deckBronze.cardList[Random.Range(0, deckBronze.cardList.Count)];
                deckBronze.cardList.Remove(card);
            }

        }

        else if (choose <= fluorite + bronze + silver)  // �ǹ�
        {
            if (deckSilver.cardList.Count == 0)
                card = emptyCard;
            else
            {
                card = deckSilver.cardList[Random.Range(0, deckSilver.cardList.Count)];
                deckSilver.cardList.Remove(card);
            }
        }

        else if (choose <= fluorite + bronze + silver + gold)   // ���
        {
            if (deckGold.cardList.Count == 0)
                card = emptyCard;
            else
            {
                card = deckGold.cardList[Random.Range(0, deckGold.cardList.Count)];
                deckGold.cardList.Remove(card);
            } 
        }

        else // �÷�Ƽ��
        {
            if (deckPlatinum.cardList.Count == 0)
                card = emptyCard;
            else
            {
                card = deckPlatinum.cardList[Random.Range(0, deckPlatinum.cardList.Count)];
                deckPlatinum.cardList.Remove(card);
            }
        }
        return card;
    }
}
