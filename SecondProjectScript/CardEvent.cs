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

    // 레벨업하여 카드 이벤트가 발생 시, 세 장의 카드에 랜덤으로 정해진 카드가 할당됨, 만약 중복으로 뽑혔다면 다시 정하여 할당
    void OnEnable()
    {
        
        GameManager.instance.PauseGame();

        for (int i = 0; i < 3; i++)
        {
            UIManager.instance.cardButtons[i].SetActive(true);
            card[i] = SetCard(playStatus);

            for (int j = 0; j < i; j++) // 중복 체크
            {
                if (card[i].id == -1)
                    break;

                if (card[i].rank == card[j].rank && card[i].id == card[j].id)
                {
                    switch (card[i].rank)   // 중복이라면 덱에서 꺼내온 카드를 다시 넣고
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
                    card[i] = SetCard(playStatus);  // 다시 세트
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

    // 선택된 카드의 효과 발동과 선택 이미지 효과
    public void OnCardSelected()
    {
        for (int i = 0;i < 3;i++)
            UIManager.instance.cardButtons[i].SetActive(false);
        SoundManager.instance.audioSourceEFX.PlayOneShot(SoundManager.instance.cardSelect);
        Transform selectedCard = EventSystem.current.currentSelectedGameObject.transform.parent;
        int selectedCardNum = int.Parse(selectedCard.name.Substring(4));
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i != selectedCardNum)   // 선택된 카드가 아니면 다시 덱에 넣어주고 비활성화
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
        if (card.cardTitle.Contains("카드 없음"))
            return;
        switch (card.rank)
        {
            case Rank.Fluorite:
                switch (card.id)
                {
                    case 0:// 내부 분열
                        GameManager.instance.spawnRate *= 1.5f;
                        break;
                    case 1:// 도발
                        GameManager.instance.spawnRate *= 0.5f;
                        break;
                    case 2:// 달팽이
                        GameManager.instance.snailRate = 0.7f;
                        break;
                    case 3:// 사기저하
                        GameManager.instance.DemoralizingRate = 0.7f;
                        break;
                    case 4:// 철괴
                        GameManager.instance.tetkaiRate = 0.5f;
                        break;
                    case 5:// 새로 고침
                        OnDisable();
                        OnEnable();
                        break;
                }
                break;

            case Rank.Bronze:
                switch (card.id)
                {
                    case 0:// 동검
                        playStatus.criticalRate = 10;
                        break;
                    case 1:// 동방패
                        playStatus.defense = 1;
                        break;
                    case 2:// 초심자의 열정
                        playStatus.expRate *= 10;
                        playStatus.expRate *= 15;
                        playStatus.expRate /= 100;
                        break;
                    case 3:// 빠른 검기
                        playStatus.fastSwordTechnique = true;
                        break;
                }
                break;

            case Rank.Silver:
                switch (card.id)
                {
                    case 0:// 다중 공격
                        playStatus.attackableNum++;
                        break;
                    case 1:// 힐링 팩터
                        playStatus.healValue = 1;
                        break;
                    case 2:// 포인트 적립
                        playStatus.skillPoints += 2;
                        break;
                    case 3:// 긴급 치료
                        foreach (Player player in GameManager.instance.players)
                            player.hp += 5;
                        break;
                    case 4:// 은검
                        playStatus.criticalRate = 15;
                        break;
                    case 5:// 은방패
                        playStatus.defense = 3;
                        break;
                    case 6:// 흡혈
                        playStatus.bloodsucking = true;
                        break;
                }
                break;

            case Rank.Gold:
                switch (card.id)
                {
                    case 0:// 다중 공격
                        playStatus.attackableNum += 2;
                        break;
                    case 1:// 힐링 팩터
                        playStatus.healValue = 2;
                        break;
                    case 2:// 포인트 적립
                        playStatus.skillPoints += 3;
                        break;
                    case 3:// 긴급 치료
                        foreach (Player player in GameManager.instance.players)
                            player.hp += 10;
                        break;
                    case 4:// 금검
                        playStatus.criticalRate = 20;
                        break;
                    case 5:// 금방패
                        playStatus.defense = 5;
                        break;
                    case 6:// 고슴도치
                        playStatus.hedgehog = true;
                        break;
                }
                break;

            case Rank.Platinum:
                switch (card.id)
                {
                    case 0:// 다중 공격
                        playStatus.attackableNum += 3;
                        break;
                    case 1:// 힐링 팩터
                        playStatus.healValue = 3;
                        break;
                    case 2:// 포인트 적립
                        playStatus.skillPoints += 5;
                        break;
                    case 3:// 긴급 치료
                        foreach (Player player in GameManager.instance.players)
                            player.hp += 20;
                        break;
                    case 4:// 백금검
                        playStatus.criticalRate = 30;
                        break;
                    case 5:// 백금방패
                        playStatus.defense = 10;
                        break;
                    case 6:// 분신술
                        GameManager.instance.placeableNum++;
                        break;
                }
                break;

        }
    }

    // 일정 확률에 따라 카드가 정해짐
    public Card SetCard(PlayerStatus status)
    {
        float fluorite = status.rankProbability[0],
            bronze = status.rankProbability[1],
            silver = status.rankProbability[2],
            gold = status.rankProbability[3];

        int choose = Random.Range(1, 101);

        Card card;

        if (choose <= fluorite) // 플루오라이트
            card = deckFluorite.cardList[Random.Range(0, deckFluorite.cardList.Count)];

        else if (choose <= fluorite + bronze)   // 브론즈
        {
            if (deckBronze.cardList.Count == 0)
                card = emptyCard;
            else
            {
                card = deckBronze.cardList[Random.Range(0, deckBronze.cardList.Count)];
                deckBronze.cardList.Remove(card);
            }

        }

        else if (choose <= fluorite + bronze + silver)  // 실버
        {
            if (deckSilver.cardList.Count == 0)
                card = emptyCard;
            else
            {
                card = deckSilver.cardList[Random.Range(0, deckSilver.cardList.Count)];
                deckSilver.cardList.Remove(card);
            }
        }

        else if (choose <= fluorite + bronze + silver + gold)   // 골드
        {
            if (deckGold.cardList.Count == 0)
                card = emptyCard;
            else
            {
                card = deckGold.cardList[Random.Range(0, deckGold.cardList.Count)];
                deckGold.cardList.Remove(card);
            } 
        }

        else // 플래티넘
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
