using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "New Status", menuName = "Status/player", order = 1)]
public class PlayerStatus : ScriptableObject
{
    public int hpMax;
    public int hp;
    public int attackableNum;
    public int attackDamage;
    public float attackSpeed;
    public int criticalRate;
    public int defense;
    public int avoidability;
    public float expRate;
    public float luck;
    public float[] rankProbability = new float[5];

    public float totalExp;
    public float currentExp;
    public int skillPoints;

    public int level;
    public int healValue;

    public bool fastSwordTechnique = false;
    public bool bloodsucking = false;
    public bool hedgehog = false;
    public void Awake()
    {
        hpMax = 40;
        hp = 40;
        attackableNum = 1;
        attackDamage = 1;
        attackSpeed = 1f;
        criticalRate = 0;
        defense = 0;
        avoidability = 10;
        expRate = 1f;
        luck = 1f;
        for (int i = 0; i < 5; i++)
            rankProbability[i] = 0;

        totalExp = 90f;
        currentExp = 0f;
        skillPoints = 5;

        level = 1;
        healValue = 0;

        fastSwordTechnique = false;
        bloodsucking = false;
        hedgehog = false;
    }

    public void InvestPoints()
    {
        string buttonName = EventSystem.current.currentSelectedGameObject.name.Substring(7);
        if (GameManager.instance.playerPlaced == true && skillPoints > 0)
        {
            switch (buttonName)
            {
                case "attackDamage":
                    attackDamage++;
                    break;
                case "attackSpeed":
                    attackSpeed *= 10;
                    attackSpeed++;
                    attackSpeed /= 10;
                    break;
                case "avoidability":
                    if (avoidability >= 100)
                        return;
                    avoidability+= 5;
                    break;
                case "expRate":
                    expRate *= 10;
                    expRate++;
                    expRate /= 10;
                    break;
                case "luck":
                    luck *= 10;
                    luck++;
                    luck /= 10;
                    break;
            }
            skillPoints--;
            UIManager.instance.SetPlayerStatusTexts(this);
            GameManager.instance.CalculateRankProbability(rankProbability);
        }
        if (skillPoints == 0)
            UIManager.instance.buttonImage.SetActive(false);
    }
}
