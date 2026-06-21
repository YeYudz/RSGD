using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家数据
/// </summary>
[System.Serializable]
public class PlayerData
{
    //当前拥有多少金币
    public int haveMoney;
    //当前解锁了 哪些角色
    public List<int> haveCharacter=new List<int>();

    // 构造函数，确保默认值
    public PlayerData()
    {
        //haveMoney = 500;
        //haveCharacter = new List<int>() { 0 };
    }

}
