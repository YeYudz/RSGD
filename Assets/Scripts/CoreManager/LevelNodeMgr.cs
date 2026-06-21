using System.Collections.Generic;
using UnityEngine;

public class LevelNodeMgr : BaseManager<LevelNodeMgr>
{
    // 当前节点索引（从1开始，与nodeId一致）
    public int curNodeId;

    private List<LevelNodeInfo> nodeList;

    public LevelNodeMgr()
    {
        LoadNodes();
        curNodeId = 0;
    }
   

    private void LoadNodes()
    {
        nodeList = GameDataMgr.Instance.levelNodeInfoList;
        nodeList.Sort((a, b) => a.nodeId.CompareTo(b.nodeId));
        Debug.Log("加载节点数据成功");
    }

    /// <summary>
    /// 开始游戏，进入第一个节点
    /// </summary>
    public void StartGame()
    {
        Debug.Log("开始游戏，进入第一个节点");
        EnterNode(1);
    }

    /// <summary>
    /// 进入指定节点
    /// </summary>
    public void EnterNode(int nodeId)
    {
        curNodeId = nodeId;
        LevelNodeInfo node = nodeList.Find(n => n.nodeId == nodeId);

        if (node == null)
        {
            Debug.LogError($"LevelNodeMgr: 找不到节点 {nodeId}");
            return;
        }

        HandleNode(node);
    }

    /// <summary>
    /// 节点完成（战斗胜利 / 商店离开 / 休息完成）
    /// </summary>
    public void FinishCurNode()
    {
        LevelNodeInfo node = nodeList.Find(n => n.nodeId == curNodeId);

        if (node == null || node.nextNodeId == 0)
        {
            // 通关
            EventCenter.GetInstance().EventTrigger<bool>("GAME_OVER", false);
            return;
        }

        EnterNode(node.nextNodeId);
    }

    private void HandleNode(LevelNodeInfo node)
    {
        Debug.Log("进入节点"+node.nodeId+"类型"+node.nodeType);
        switch (node.nodeType)
        {
            case "Battle":
                EventCenter.GetInstance().EventTrigger<int>("ENTER_BATTLE_NODE",node.monsterGroupId);
                break;

            case "Shop":
                EventCenter.GetInstance().EventTrigger("ENTER_SHOP_NODE");
                ////暂停游戏
                //GamingPauseMgr.SetPause(true);
                break;

            case "Rest":
                EventCenter.GetInstance().EventTrigger("ENTER_REST_NODE");
                break; 

            case "Boss":
                EventCenter.GetInstance().EventTrigger<int>("ENTER_BOSS_NODE",node.monsterGroupId);
                break;
        }
    }

    /// <summary>
    /// 游戏失败 / 重开时清理
    /// </summary>
    public void Clear()
    {
        curNodeId = 0;
    }
}