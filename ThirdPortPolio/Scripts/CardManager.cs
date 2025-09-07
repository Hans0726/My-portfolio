using System.Collections.Generic;

public class CardManager
{
    private List<CardData> _deck = new List<CardData>(); // ���� ���ӿ� ����� ��
    private List<CardData> _hand = new List<CardData>(); // ���� �տ� ��� �ִ� ī��
    public List<CardData> Hand { get { return _hand; } } // �ٸ� ��ũ��Ʈ(UI ��)���� �ڵ� ������ �б� ���� ������Ƽ
    private List<CardData> _discardPile = new List<CardData>();


    /// <summary>
    /// ���� ���� �� �����κ��� ���� �� ī�� ID ����Ʈ�� ������� _deck ����Ʈ�� ä��� �Լ�
    /// </summary>
    /// <param name="deckCardIds"></param>
    public void InitializeDeck(List<short> deckCardIds)
    {

    }

    /// <summary>
    /// _deck ����Ʈ�� �����ϰ� �����ϴ�.System.Random�̳� UnityEngine.Random�� ����Ͽ� ����(Fisher-Yates �˰��� ��)
    /// </summary>
    public void ShuffleDeck()
    {

    }

    /// <summary>
    /// _deck���� ������ ����ŭ ī�带 �̾� _hand ����Ʈ�� �߰��ϰ�, _deck������ �����մϴ�.���� ��� _discardPile�� ���� _deck���� �������� ������ �߰��� �� �ֽ��ϴ�.
    /// </summary>
    public void DrawCards(int count)
    {

    }

    /// <summary>
    ///  �ڵ忡�� ī�带 ����ϴ� ����(�ڵ忡�� ����, �ʿ�� _discardPile�� �߰� ��)
    /// </summary>
    public void PlayCard(CardData card)
    {

    }
}