using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Battle,     // 普通战斗
    Boss,       // Boss战
    Shop,       // 商店
    Rest        // 休息
}
public class LevelNodeInfo
{
    public int nodeId;
    public string nodeType;        // Battle / Boss / Shop / Rest
    public int monsterGroupId;       // 敌人组ID
    public int shopId;             // 商店ID
    public int nextNodeId;         // 下一个节点
}
