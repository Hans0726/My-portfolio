using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SaveData
{
    // game system data
    public int gateHp;
    public int stage;
    public int placeableNum;
    public List<float> playerPositionX;

    // player status data
    public int      hpMax;
    public int      hp;
    public int      attackableNum;
    public int      attackDamage;
    public float    attackSpeed;
    public int      criticalRate;
    public int      defense;
    public int      avoidbility;
    public float    expRate;
    public float    luck;
    public float[] rankProbability = new float[5];
    public float    totalExp;
    public float    currentExp;
    public int      skillPoints;

    public int      level;
    public int      healValue;

    public DeckInfo[] playerDeckInfo = new DeckInfo[4];
    public bool fastSwordTechnique;
    public bool bloodsucking;
    public bool hedgehog;

    public SaveData(PlayerStatus playerStatus, DeckInfo[] _playerDeckInfo)
    {
        gateHp          = GameManager.instance.gateHp;
        stage           = GameManager.instance.stage;
        placeableNum    = GameManager.instance.placeableNum;
        playerPositionX = GameManager.instance.playerPositionX;

        hpMax           = playerStatus.hpMax;
        hp              = playerStatus.hp;
        attackableNum   = playerStatus.attackableNum;
        attackDamage    = playerStatus.attackDamage;
        attackSpeed     = playerStatus.attackSpeed;
        criticalRate    = playerStatus.criticalRate;
        defense         = playerStatus.defense;
        avoidbility     = playerStatus.avoidability;
        expRate         = playerStatus.expRate;
        luck            = playerStatus.luck;
        rankProbability = playerStatus.rankProbability;
        totalExp        = playerStatus.totalExp;
        currentExp      = playerStatus.currentExp;
        skillPoints     = playerStatus.skillPoints;

        level           = playerStatus.level;
        healValue       = playerStatus.healValue;
        playerDeckInfo  = _playerDeckInfo;
        fastSwordTechnique = playerStatus.fastSwordTechnique;
        bloodsucking    = playerStatus.bloodsucking;
        hedgehog        = playerStatus.hedgehog;
    }
}
