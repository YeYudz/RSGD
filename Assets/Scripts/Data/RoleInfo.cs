using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleInfo
{
    public int id;
    public string name;

    public float atk;
    public float hp;
    public float def;

    public int type;
    public int price;

    public string hitEff;
    public string res;
    public string objRes;
    public string bulletRes;

    public float moveSpeed;
    public float atkRange;
    public float atkOffTime;

    public string weaponRes;
    public int bulletCounts;
    public int bulletId;
    public int shootCount;

    // 技能系统
    public SkillInfo skillE;  // E技能
    public SkillInfo skillQ;  // Q技能
}

// 技能信息类
[System.Serializable]
public class SkillInfo
{
    public string name;           // 技能名称
    public string description;    // 技能描述
    public float atkBonus;        // 攻击力加成百分比（1.0 = 100%）
    public float duration;        // 持续时间（秒）
    public float cooldown;        // 冷却时间（秒）
    public SkillType type;        // 技能类型
    public float hpCost;          // 扣除生命值百分比（仅角色2的Q技能）
    public float weaponScale;     // 武器大小倍数（仅角色2的Q技能）
}

// 技能类型枚举
public enum SkillType
{
    ATK_BOOST,        // 攻击力提升
    ATK_BOUST_HP_COST, // 攻击力提升+扣血+武器变大（角色2的Q）
    INFINITE_AMMO,    // 无限弹药（角色3的Q）
    MULTI_SHOT,       // 多重射击（角色4的Q）
}
