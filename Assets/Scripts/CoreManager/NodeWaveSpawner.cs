using System.Collections;
using UnityEngine;

public class NodeWaveSpawner : BaseManager<NodeWaveSpawner>
{
    private MonsterGroupInfo curGroup;
    private int totalCount;   // 本节点应生成怪物总数
    private int nowCount;     // 当前剩余存活怪物数

    /// <summary>
    /// 开始节点战斗生成
    /// </summary>
    public void StartSpawn(int groupId)
    {
        curGroup = GameDataMgr.Instance.monsterGroupInfoList.Find(g => g.groupId == groupId);

        if (curGroup == null)
        {
            Debug.LogError($"NodeWaveSpawner: 找不到怪物组 {groupId}");
            return;
        }

        // 计算总怪物数
        totalCount = 0;
        foreach (var wave in curGroup.spawnList)
        {
            totalCount += wave.count;
        }

        nowCount = totalCount;

        Debug.Log($"节点战斗开始，总怪物数 = {totalCount}");

        // 按波次生成
        SpawnAllWaves();
    }
    /// <summary>
    /// 生成 BOSS
    /// </summary>
    public void SpawnBossGroup(int groupId)
    {
        var group = GameDataMgr.Instance.monsterGroupInfoList.Find(g => g.groupId == groupId);

        if (group == null)
        {
            Debug.LogError($"找不到 Boss 怪物组 {groupId}");
            return;
        }

        foreach (var spawn in group.spawnList)
        {
            EventCenter.GetInstance().EventTrigger<SpawnEnemyParam>(
                "SPAWN_MONSTER_WAVE",
                new SpawnEnemyParam
                {
                    enemyId = spawn.monsterId,
                    count = spawn.count
                }
            );
        }
    }
    /// <summary>
    /// 按 spawnList 分波生成（带间隔）
    /// </summary>
    private void SpawnAllWaves()
    {
        MonoMgr.GetInstance().StartCoroutine(SpawnWavesWithDelay());
    }

    private IEnumerator SpawnWavesWithDelay()
    {
        foreach (var wave in curGroup.spawnList)
        {
            EventCenter.GetInstance().EventTrigger<SpawnEnemyParam>(
                "SPAWN_MONSTER_WAVE",
                new SpawnEnemyParam
                {
                    enemyId = wave.monsterId,
                    count = wave.count
                }
            );

            // 波次间隔时间（可配置，默认2秒）
            yield return new WaitForSeconds(2f);
        }
    }

    /// <summary>
    /// 怪物死亡时调用
    /// </summary>
    public void OnMonsterDead()
    {
        nowCount--;
        Debug.Log($"怪物死亡，剩余 {nowCount}");

        if (nowCount <= 0)
        {
            Debug.Log("节点怪物全部清空，准备进入下一节点");
            EventCenter.GetInstance().EventTrigger("NODE_MONSTER_ALL_DEAD");
        }
    }

    public void Clear()
    {
        totalCount = 0;
        nowCount = 0;
        curGroup = null;
    }
}
