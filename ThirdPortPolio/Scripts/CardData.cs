using System;
using UnityEngine;


[Serializable]
public class Card
{
    public short cardId;
    public bool isInDeck;
    public string cardName;
    public float moveSpeed;
    public int cost;
    public string specialEffect;
}

[Serializable]
public class AttackCard : Card
{
    public int health;
    public int defense;
}

[Serializable]
public class DefenseCard : Card
{
    public int attack;
    public float attackSpeed;
}


// �� �Ӽ��� �߰��ؾ� ����Ƽ �����Ϳ��� ���� ���� ������ ���������ϴ�!
[CreateAssetMenu(fileName = "New CardData", menuName = "Card Data", order = 1)]
public class CardData : ScriptableObject
{
    [Header("�⺻ ����")]
    public short cardId;                // ī�� ���� ID (������ ���� �� �߿�)
    public string cardName = "ī�� �̸�";
    public float moveSpeed = 1.0f;      // �̵� �ӵ� (����/��� ���� ����)
    public int cost = 1;

    public Sprite cardImage;            // ī�� �̹���
    public CardType cardType;

    [Header("���� ī�� ����")]
    public int health = 10;             // �⺻ ü�� (���� ī���)
    public int defense = 0;             // �⺻ ���� (���� ī���)

    [Header("��� ī�� ����")]
    public int attack = 5;              // �⺻ ���ݷ� (��� ī���)
    public float attackSpeed = 1.0f;    // �⺻ ���� �ӵ� (��� ī���)

    [Header("Ư�� ȿ��")]
    public string specialEffect = ""; // Ư�� ȿ�� (���ڿ� �Ǵ� enum ��)

    private void OnEnable()
    {
        if (cardType == CardType.Attack)
        {
            attack = 0;
            attackSpeed = 0;
        }
        else
        {
            health = 0;
            defense = 0;
        }
    }
}

public enum CardType { UnDefined, Attack, Defense }
