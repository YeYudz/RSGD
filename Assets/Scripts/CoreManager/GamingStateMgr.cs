using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public struct BulletCountsMaxAndNow//弹匣
{
    public int nowCounts;
    public int maxCounts;
}
public enum BulletOwner//子弹父对象
{
    Player,
    Monster
}
public struct SpawnEnemyParam//生成怪物参数
{
    public int enemyId;
    public int count;
}
public struct SpawnBulletParam//生成子弹参数
{
    public int bulletId;
    public int count;          // 原始消耗数量
    public int extraCount;     // 额外生成的子弹数量（不消耗弹药）
    public Transform shootPoint;
    public Vector3 shootDir;
    public BulletOwner owner;
    public float atk;
}
public class GamingStateMgr : MonoBehaviour
{
    public List<Monster> activeMonsters = new List<Monster>();
    //引入「升级奖励队列」
    private Queue<int> rewardLevelQueue = new Queue<int>();
    private bool isRewardPanelOpen = false;
    private bool shouldOpenRewardWhenAvailable = false; // 新增标志位
    //private int pendingShopLevels = 0;// 缓存商店升级等级
    // 玩家预设体
    private GameObject playerObj;
    //射击协程
    private Coroutine shootCoroutine;
    //结算协程
    private Coroutine gameSettlementCoroutine;
    //游戏状态
    private bool isActive;
    // 射击状态控制
    private bool isShootingActive = false;  // 是否有射击协程正在运行
    // 玩家状态
    public float curHp;
    public float maxHp;
    public float atk;
    public float def;
    public float atkRange;
    public string bulletRes;
    public int nowBulletCounts;

    // 攻击力分离：基础攻击力（奖励加成）+ 技能加成倍率
    private float baseAtk;                    // 基础攻击力（角色配置 + 奖励加成），常驻
    private float currentSkillBonus = 0f;     // 当前技能攻击力加成倍率（限时）
    //局内金币
    private int battleGold;
    //局内经验
    int gainExp;
    // 当前节点怪物总数
    private int curNodeMonsterTotal;
    // 无限弹药状态
    private bool isInfiniteAmmo = false;
    //初始化玩家等级
    public PlayerGamingData playerLevel = new PlayerGamingData
    {
        level = 1,
        currentExp = 0
    };
    //强制冻结（暂停对象行为活动）
    public static bool IsFrozen =>
    !GamingLifecycleMgr.IsRunning || GamingLifecycleMgr.IsPaused;
    private void Start()
    {
        
    }
    void Update()
    {
        if (!isActive) return;

    }
    private void Awake()
    {
        //订阅生命周期
        GamingLifecycleMgr.OnGameStart += OnGameStarted;
        GamingLifecycleMgr.OnGamePause += OnGamePaused;
        GamingLifecycleMgr.OnGameResume += OnGameResumed;
        GamingLifecycleMgr.OnGameShutdown += OnGameShutdown;
        // 监听选角事件
        EventCenter.GetInstance().AddEventListener<int>("CHOOSED_HERO_ID", OnInitPlayer);
        // 监听伤害/治疗事件    调试用  后续可做成功能 再说
        EventCenter.GetInstance().AddEventListener<float>("PLAYER_HURT", OnPlayerHurt);
        EventCenter.GetInstance().AddEventListener<float>("PLAYER_HEAL", OnPlayerHeal);
        EventCenter.GetInstance().AddEventListener<float>("PLAYER_HEAL_PERCENT", OnPlayerHealPercent);
        // 监听玩家攻击事件      调试用  后续可做成功能 再说
        EventCenter.GetInstance().AddEventListener("PLAYER_KNIFE_ATTACK", OnPlayerKnifeAttack);
        //// 监听敌人 生成/死亡 事件
        EventCenter.GetInstance().AddEventListener<SpawnEnemyParam>("SPAWN_Z1_MONSTER", OnSpawnMonster);//新添加的Monster
        EventCenter.GetInstance().AddEventListener<SpawnEnemyParam>("SPAWN_Z2_MONSTER", OnSpawnMonster);//新添加的Monster
        EventCenter.GetInstance().AddEventListener<SpawnEnemyParam>("SPAWN_MONSTER_WAVE", OnSpawnMonster);//节点波次生怪
        EventCenter.GetInstance().AddEventListener<int>("MONSTER_DIE", OnMonsterDie);
        //监听 进入不同节点
        EventCenter.GetInstance().AddEventListener<int>("ENTER_BATTLE_NODE",OnEnterBattleNode);
        //EventCenter.GetInstance().AddEventListener("ENTER_SHOP_NODE", OnEnterShopNode);
        EventCenter.GetInstance().AddEventListener("ENTER_REST_NODE", OnEnterRestNode);
        EventCenter.GetInstance().AddEventListener<int>("ENTER_BOSS_NODE", OnEnterBOSSNode);
        //监听进入下一节点
        EventCenter.GetInstance().AddEventListener("NODE_MONSTER_ALL_DEAD",OnFinishCurNode);
        //监听离开商店节点
        EventCenter.GetInstance().AddEventListener("SHOP_LEAVE",OnLeaveShop);
        //Monster
        EventCenter.GetInstance().AddEventListener<Monster>("MONSTER_SPAWNED", OnMonsterSpawned);//新添加的Monster
        EventCenter.GetInstance().AddEventListener<Monster>("MONSTER_DIED", OnMonsterDied);//新添加的Monster
        //监听子弹生成事件
        //玩家
        EventCenter.GetInstance().AddEventListener<SpawnBulletParam>("SPAWN_PLAYER_BULLET",OnSpawnBullet);
        //怪物
        EventCenter.GetInstance().AddEventListener<SpawnBulletParam>("SPAWN_MONSTER_BULLET",OnSpawnMonsterBullet);
        //监听 换弹事件
        EventCenter.GetInstance().AddEventListener<RoleInfo>("FINISH_RELOAD",OnFinishReload);
        //监听升级事件
        EventCenter.GetInstance().AddEventListener<int>("PLAYER_LEVEL_UP", OnPlayerLevelUp);
        //监听buff获取
        EventCenter.GetInstance().AddEventListener<int>("GET_BUFF",AddBuffToPlayer);
        // 监听奖励面板关闭
        EventCenter.GetInstance().AddEventListener("REWARD_PANEL_CLOSED", OnRewardPanelClosed);      //关闭
        //监听游戏结束事件
        EventCenter.GetInstance().AddEventListener<bool>("GAME_OVER",OnGameOverDo);
        //监听攻击范围变化（角色2的Q技能）
        EventCenter.GetInstance().AddEventListener<float>("ATK_RANGE_FRESH", OnAtkRangeFresh);
        // 监听技能攻击力加成变化
        EventCenter.GetInstance().AddEventListener<float>("SKILL_ATK_BONUS", OnSkillAtkBonus);
        // 监听无限弹药状态变化
        EventCenter.GetInstance().AddEventListener<bool>("INFINITE_AMMO_STATE", OnInfiniteAmmoState);
        //监听游戏退出事件
        EventCenter.GetInstance().AddEventListener("GAME_OVER_CONFIRMED",OnGameShutdown);
        EventCenter.GetInstance().AddEventListener("SETTING_CLOSE_TO_MENU", OnGameShutdown);

    }
    private void OnDestroy()
    {
        // 取消订阅生命周期事件
        GamingLifecycleMgr.OnGameStart -= OnGameStarted;
        GamingLifecycleMgr.OnGamePause -= OnGamePaused;
        GamingLifecycleMgr.OnGameResume -= OnGameResumed;
        GamingLifecycleMgr.OnGameShutdown -= OnGameShutdown;

        // 监听选角事件
        EventCenter.GetInstance().RemoveEventListener<int>("CHOOSED_HERO_ID", OnInitPlayer);
        // 监听伤害/治疗事件    调试用  后续可做成功能 再说
        EventCenter.GetInstance().RemoveEventListener<float>("PLAYER_HURT", OnPlayerHurt);
        EventCenter.GetInstance().RemoveEventListener<float>("PLAYER_HEAL", OnPlayerHeal);
        EventCenter.GetInstance().RemoveEventListener<float>("PLAYER_HEAL_PERCENT", OnPlayerHealPercent);
        // 监听玩家攻击事件      调试用  后续可做成功能 再说
        EventCenter.GetInstance().RemoveEventListener("PLAYER_KNIFE_ATTACK", OnPlayerKnifeAttack);
        //// 监听敌人 生成/死亡 事件
        EventCenter.GetInstance().RemoveEventListener<SpawnEnemyParam>("SPAWN_Z1_MONSTER", OnSpawnMonster);//新添加的Monster
        EventCenter.GetInstance().RemoveEventListener<SpawnEnemyParam>("SPAWN_Z2_MONSTER", OnSpawnMonster);//新添加的Monster
        EventCenter.GetInstance().RemoveEventListener<SpawnEnemyParam>("SPAWN_MONSTER_WAVE", OnSpawnMonster);//节点波次生怪
        EventCenter.GetInstance().RemoveEventListener<int>("MONSTER_DIE", OnMonsterDie);
        //监听 进入不同节点
        EventCenter.GetInstance().RemoveEventListener<int>("ENTER_BATTLE_NODE", OnEnterBattleNode);
        //EventCenter.GetInstance().RemoveEventListener("ENTER_SHOP_NODE", OnEnterShopNode);
        EventCenter.GetInstance().RemoveEventListener("ENTER_REST_NODE", OnEnterRestNode);
        EventCenter.GetInstance().RemoveEventListener<int>("ENTER_BOSS_NODE", OnEnterBOSSNode);
        //监听进入下一节点
        EventCenter.GetInstance().RemoveEventListener("NODE_MONSTER_ALL_DEAD", OnFinishCurNode);
        //监听离开商店节点
        EventCenter.GetInstance().RemoveEventListener("SHOP_LEAVE", OnLeaveShop);
        //Monster
        EventCenter.GetInstance().RemoveEventListener<Monster>("MONSTER_SPAWNED", OnMonsterSpawned);//新添加的Monster
        EventCenter.GetInstance().RemoveEventListener<Monster>("MONSTER_DIED", OnMonsterDied);//新添加的Monster
        //监听子弹生成事件
        //玩家
        EventCenter.GetInstance().RemoveEventListener<SpawnBulletParam>("SPAWN_PLAYER_BULLET", OnSpawnBullet);
        //怪物
        EventCenter.GetInstance().RemoveEventListener<SpawnBulletParam>("SPAWN_MONSTER_BULLET", OnSpawnMonsterBullet);
        //监听 换弹事件
        EventCenter.GetInstance().RemoveEventListener<RoleInfo>("FINISH_RELOAD", OnFinishReload);
        //监听升级事件
        EventCenter.GetInstance().RemoveEventListener<int>("PLAYER_LEVEL_UP", OnPlayerLevelUp);
        //监听buff获取
        EventCenter.GetInstance().RemoveEventListener<int>("GET_BUFF", AddBuffToPlayer);
        //监听奖励面板关闭
        EventCenter.GetInstance().RemoveEventListener("REWARD_PANEL_CLOSED", OnRewardPanelClosed);      //关闭
        //监听游戏结束事件
        EventCenter.GetInstance().RemoveEventListener<bool>("GAME_OVER", OnGameOverDo);
        // 监听技能攻击力加成变化
        EventCenter.GetInstance().RemoveEventListener<float>("SKILL_ATK_BONUS", OnSkillAtkBonus);
        // 监听无限弹药状态变化
        EventCenter.GetInstance().RemoveEventListener<bool>("INFINITE_AMMO_STATE", OnInfiniteAmmoState);
        //监听游戏退出事件
        EventCenter.GetInstance().RemoveEventListener("GAME_OVER_CONFIRMED", OnGameShutdown);
        EventCenter.GetInstance().RemoveEventListener("SETTING_CLOSE_TO_MENU", OnGameShutdown);
    }
    private void OnGameStarted()
    {
        isActive = true;
    }
    private void OnGamePaused()
    {
        isActive = false;
    }
    private void OnGameResumed()
    {
        isActive = true;
    }
    private void OnGameShutdown()//退出游戏======返回主菜单时调用
    {
        isActive = false;
        isShootingActive = false;  // 重置射击状态
        activeMonsters.Clear();
        rewardLevelQueue.Clear();
        StopAllCoroutines();
        LevelNodeMgr.GetInstance().Clear();
        NodeWaveSpawner.GetInstance().Clear();
        UIManager.Instance.HidePanel<GamingPanel>();
        UIManager.Instance.HidePanel<ShopPanel>();
        UIManager.Instance.HidePanel<RewardPanel>();
        UIManager.Instance.HidePanel<GameOverPanel>();
    }
    private void OnInitPlayer(int roleId)
    {
        // 从GameDataMgr获取角色配置
        RoleInfo data = GameDataMgr.Instance.roleInfoList[roleId];
        if (data == null)
        {
            Debug.LogError($"未找到角色ID: {roleId}");
            return;
        }
        maxHp = data.hp;
        curHp = maxHp;
        atk = data.atk;
        baseAtk = data.atk;                    // 初始化基础攻击力
        currentSkillBonus = 0f;                // 初始化技能加成
        def = data.def;
        atkRange = data.atkRange;
        nowBulletCounts = data.bulletCounts;
        if (!string.IsNullOrEmpty(data.objRes))
        {
            playerObj = Resources.Load<GameObject>(data.objRes);
            if (playerObj != null)
            {
                playerObj = Instantiate(
                    playerObj,
                    Vector3.zero,
                    Quaternion.identity
                );

                Debug.Log($"角色模型实例化完成：{data.objRes}");
            }
            else
            {
                Debug.LogError($"角色模型加载失败：{data.objRes}");
            }
        }
        EventCenter.GetInstance().EventTrigger<RoleInfo>("PLAYER_SPAWN", data);

        Debug.Log($"玩家初始化完成：maxHp={maxHp}, curHp={curHp}"); // 确认maxHp不为0
        Debug.Log($"玩家初始化完成：atk={atk}, baseAtk={baseAtk}, atkRange={atkRange}"); // 确认不为0
        // 触发UI更新
        EventCenter.GetInstance().EventTrigger<float>("UI_HP_UPDATE", (float)curHp / maxHp);
        EventCenter.GetInstance().EventTrigger<float>("ATK_FRESH", CalculateFinalAtk());
        GamingLifecycleMgr.StartGame();
    }
    private void OnPlayerHurt(float damage)
    {
        EventCenter.GetInstance().EventTrigger("PLAYER_IS_WOUNDED");
        curHp -= damage;
        curHp = Mathf.Max(curHp, 0);
        Debug.Log($"玩家受伤：剩余HP={curHp}");
        EventCenter.GetInstance().EventTrigger<float>("UI_HP_UPDATE", (float)curHp / maxHp);
        if (curHp <= 0)
        {
            EventCenter.GetInstance().EventTrigger<bool>("GAME_OVER",true);
        }
    }

    // 攻击范围变化（角色2的Q技能）
    private void OnAtkRangeFresh(float newAtkRange)
    {
        atkRange = newAtkRange;
        Debug.Log($"攻击范围更新：{atkRange}");
    }

    // 技能攻击力加成变化处理
    private void OnSkillAtkBonus(float bonus)
    {
        currentSkillBonus = bonus;
        float finalAtk = CalculateFinalAtk();
        EventCenter.GetInstance().EventTrigger<float>("ATK_FRESH", finalAtk);
        Debug.Log($"技能攻击力加成更新：bonus={bonus}, 最终攻击力={finalAtk}");
    }

    private void OnInfiniteAmmoState(bool isActive)
    {
        isInfiniteAmmo = isActive;
        Debug.Log($"无限弹药状态: {(isActive ? "开启" : "关闭")}");
    }

    // 计算最终攻击力（基础攻击力 × (1 + 技能加成倍率)）
    private float CalculateFinalAtk()
    {
        return baseAtk * (1f + currentSkillBonus);
    }

    private void OnPlayerHeal(float heal)
    {
        curHp += heal;
        curHp = Mathf.Min(curHp, maxHp);
        Debug.Log($"玩家治疗：剩余HP={curHp}");
        EventCenter.GetInstance().EventTrigger<float>("UI_HP_UPDATE", (float)curHp / maxHp);
    }
    private void OnPlayerHealPercent(float heal)
    {
        curHp += heal*maxHp;
        curHp = Mathf.Min(curHp, maxHp);
        Debug.Log($"玩家治疗：剩余HP={curHp}");
        EventCenter.GetInstance().EventTrigger<float>("UI_HP_UPDATE", (float)curHp / maxHp);
    }
    private void OnMonsterDie(int monsterId)
    {
        MonsterInfo monsterCfg = GameDataMgr.Instance.monsterInfoList.Find(item => item.id == monsterId);
        if (monsterCfg == null)
        {
            Debug.LogError($"找不到 ID 为 {monsterId} 的怪物配置！");
            return;
        }
        Debug.Log("触发了刷新金币事件");
        EventCenter.GetInstance().EventTrigger<int>("UI_GOLD_UPDATE", (int)monsterCfg.giveMoney);
        battleGold += (int)monsterCfg.giveMoney;
        gainExp = (int)monsterCfg.giveEXP;
        AddExp(gainExp);
        activeMonsters.RemoveAll(e => e.monsterCfg.id == monsterId);
        Debug.Log($"GamingStateMgr: 怪物死亡，剩余 {activeMonsters.Count}");
        NodeWaveSpawner.GetInstance().OnMonsterDead();
    }
    private void OnPlayerKnifeAttack()
    {
        Transform playerT = playerObj.transform;
        Vector3 origin = playerObj.transform.position +
                         playerObj.transform.forward * 1f +
                         Vector3.up * 1f;

        float radius = atkRange; // 当前使用的是角色攻击距离

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer == -1)
        {
            Debug.LogError("场景中不存在 Enemy Layer！");
            return;
        }

        Collider[] hits = Physics.OverlapSphere(
            origin,
            radius,
            1 << enemyLayer
        );

        foreach (var hit in hits)
        {
            Transform targetT = hit.transform;

            Vector3 toTarget = (targetT.position - playerT.position).normalized;
            float dot = Vector3.Dot(playerT.forward, toTarget);

            if (dot < 0.7f)
                continue;

            Monster monster = hit.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(atk);
            }
        }
    }
    private void SpawnBulletsByType(int bulletId, int count,Transform shootPoint, Vector3 shootDir, BulletOwner owner,float atk)
    {
        if (bulletId == 0) return; 
        if (!isActive) return;

        BulletInfo bulletCfg = GameDataMgr.Instance.bulletInfoList.Find(g => g.id == bulletId);
        if (bulletCfg == null)
        {
            Debug.LogError($"GamingStateMgr: 找不到子弹配置 ID={bulletId}");
            return;
        }

        // 防止重复开火
        if (shootCoroutine != null)
            StopCoroutine(shootCoroutine);

        shootCoroutine = StartCoroutine(ShootRoutine(bulletCfg, count, shootPoint, shootDir, owner,atk));
    }
    private void OnSpawnBullet(SpawnBulletParam param)
    {
        // 如果当前有射击正在进行，忽略这次射击请求
        if (isShootingActive)
        {
            Debug.Log("射击正在执行中，忽略本次射击请求");
            return;
        }

        if (nowBulletCounts <= 0)
        {
            //没子弹了 触发换弹事件
            EventCenter.GetInstance().EventTrigger("PLAYER_RELOAD");
            return;
        }

        // 标记为正在射击
        isShootingActive = true;

        // 实际生成的子弹数量 = 原始消耗数量 + 额外生成数量
        int actualCount = param.count + param.extraCount;

        SpawnBulletsByType(param.bulletId, actualCount, param.shootPoint, param.shootDir, param.owner, param.atk);

        // 只消耗原始数量（额外子弹不消耗弹药）
        // 如果无限弹药技能激活，则不消耗子弹
        if (!isInfiniteAmmo)
        {
            nowBulletCounts -= param.count;

            if (nowBulletCounts <= 0)
                EventCenter.GetInstance().EventTrigger("NO_SOUND");
        }
        //每进行一次攻击，刷新一次子弹数量UI
        EventCenter.GetInstance().EventTrigger<int>("WEAPON_BULLET_COUNTS_UPDATE", nowBulletCounts);
    }
    private IEnumerator ShootRoutine(BulletInfo bulletCfg,int count,Transform shootPoint,Vector3 shootDir,BulletOwner owner, float atk)
    {
        for (int i = 0; i < count; i++)
        {
            // 等待游戏恢复（如果暂停了）
            while (GamingStateMgr.IsFrozen || GamingLifecycleMgr.IsPaused)
            {
                yield return null;
            }

            // 如果游戏已停止，中断射击
            if (!GamingLifecycleMgr.IsRunning)
            {
                isShootingActive = false;
                yield break;
            }

            PoolMgr.GetInstance().GetObj(bulletCfg.res, (bulletObj) =>
            {
                if (bulletObj == null)
                {
                    Debug.LogError($"加载子弹失败→{bulletCfg.res}");
                    return;
                }
                if (shootPoint == null)
                    return;
                Vector3 shootDir =owner == BulletOwner.Player? -shootPoint.right: shootPoint.forward;
                bulletObj.transform.position = shootPoint.position;
                bulletObj.transform.rotation = shootPoint.rotation;

                Bullet bullet = bulletObj.GetComponent<Bullet>();
                bullet?.Init(bulletCfg, atk, shootDir, owner);
            });
            // 每发子弹间隔
            yield return new WaitForSecondsRealtime(0.1f); // 可调
        }

        // 射击完成，清除射击状态
        isShootingActive = false;
    }
    private void OnSpawnMonsterBullet(SpawnBulletParam param)
    {
        SpawnBulletsByType(param.bulletId,param.count,param.shootPoint,param.shootDir, param.owner, param.atk);
    }
    private void OnMonsterSpawned(Monster monster)
    {
        activeMonsters.Add(monster);
    }
    private void OnMonsterDied(Monster monster)
    {
        activeMonsters.Remove(monster);

    }
    private void SpawnMonstersByType(int enemyId, int count)
    {
        for (int i = 0; i < count; i++)
        {
            MonsterInfo cfg = GameDataMgr.Instance.monsterInfoList[enemyId];
            if (cfg == null)
            {
                Debug.LogError($"GamingStateMgr: 找不到怪物配置 ID={enemyId}");
                continue;
            }

            // 从对象池获取敌人
            PoolMgr.GetInstance().GetObj(cfg.res, (monster) =>
            {
                if (monster == null)
                {
                    Debug.LogError($"GamingStateMgr: 加载怪物预制体失败→{cfg.res}");
                    return;
                }

                // 设置随机出生位置
                Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(-5, 5), 0, UnityEngine.Random.Range(-5, 5));
                monster.transform.position = spawnPos;
                monster.transform.rotation = Quaternion.identity;

                // 初始化怪物
                Monster monsterAI = monster.GetComponent<Monster>();
                if (monsterAI != null)
                {
                    monsterAI.Init(cfg);
                    monsterAI.ResetMonster();
                    activeMonsters.Add(monsterAI);
                    EventCenter.GetInstance().EventTrigger<Monster>("MONSTER_SPAWNED", monsterAI);
                }
                else
                {
                    Debug.LogError("GamingStateMgr: 怪物对象缺失Monster组件！");
                }
            });
        }
    }
    private void OnSpawnMonster(SpawnEnemyParam param)
    {
        SpawnMonstersByType(param.enemyId, param.count);
    }
    private void OnFinishReload(RoleInfo info)//换弹动作结束  做什么
    {
        //更新当前子弹数量
        nowBulletCounts=info.bulletCounts;
        Debug.Log("更新了子弹数量");
        //让GamingUIMgr更新
        EventCenter.GetInstance().EventTrigger<int>("WEAPON_BULLET_COUNTS_UPDATE", nowBulletCounts);
        Debug.Log("更新了UI显示");
    }
    private void AddExp(int amount)
    {
        playerLevel.currentExp += amount;

        while (true)
        {
            LevelInfo nextLevel = GameDataMgr.Instance.levelInfoList.Find(l => l.level == playerLevel.level + 1);

            if (nextLevel == null)
                break; // 已满级

            if (playerLevel.currentExp >= nextLevel.expRequired)
            {
                playerLevel.currentExp -= nextLevel.expRequired;

                playerLevel.level++;  // 等级提升
                int levelIncrease = 1;

                // 通知升级事件，触发奖励面板
                EventCenter.GetInstance().EventTrigger<int>("PLAYER_LEVEL_UP", levelIncrease);
            }
            else
            {
                break;
            }
        }

        // 通知 UI刷新
        EventCenter.GetInstance().EventTrigger<float>("UI_EXP_UPDATE", GetExpProgress());
    }
    private float GetExpProgress()
    {
        LevelInfo nextLevel = GameDataMgr.Instance.levelInfoList.Find(l => l.level == playerLevel.level + 1);

        if (nextLevel == null)
            return 1f;

        return (float)playerLevel.currentExp / nextLevel.expRequired;
    }
    private void AddBuffToPlayer(int id)
    {
        switch(id)
        {
            case 1:
                EventCenter.GetInstance().EventTrigger<float>("PLAYER_HEAL_PERCENT", 0.2f);
                break;
            case 2:
                EventCenter.GetInstance().EventTrigger<float>("PLAYER_HEAL_PERCENT", 0.5f);
                break;
            case 3:
                EventCenter.GetInstance().EventTrigger<float>("PLAYER_HEAL_PERCENT", 1f);
                break;
            case 4:
                baseAtk += 5;
                EventCenter.GetInstance().EventTrigger<float>("ATK_FRESH", CalculateFinalAtk());
                break;
            case 5:
                baseAtk += 10;
                EventCenter.GetInstance().EventTrigger<float>("ATK_FRESH", CalculateFinalAtk());
                break;
            case 6:
                baseAtk *= 2f;
                EventCenter.GetInstance().EventTrigger<float>("ATK_FRESH", CalculateFinalAtk());
                break;
            case 7:
                baseAtk *= 2.5f;
                EventCenter.GetInstance().EventTrigger<float>("ATK_FRESH", CalculateFinalAtk());
                break;
            case 8:
                playerLevel.level += 1;
                playerLevel.currentExp = 0;  // 重置经验
                EventCenter.GetInstance().EventTrigger<int>("PLAYER_LEVEL_UP", 1);  // 触发奖励面板
                break;
            case 9:
                playerLevel.level += 3;
                playerLevel.currentExp = 0;  // 重置经验
                EventCenter.GetInstance().EventTrigger<int>("PLAYER_LEVEL_UP", 3);  // 触发奖励面板
                break;
            case 1001:
                EventCenter.GetInstance().EventTrigger<float>("PLAYER_HEAL_PERCENT", 0.05f);
                break;
            case 1002:
                EventCenter.GetInstance().EventTrigger<float>("PLAYER_HEAL_PERCENT", 0.1f);
                break;
            case 1003:
                EventCenter.GetInstance().EventTrigger<float>("PLAYER_HEAL_PERCENT", 0.2f);
                break;
            case 1004:
                baseAtk += 2;
                EventCenter.GetInstance().EventTrigger<float>("ATK_FRESH", CalculateFinalAtk());
                break;
            case 1005:
                baseAtk += 5;
                EventCenter.GetInstance().EventTrigger<float>("ATK_FRESH", CalculateFinalAtk());
                break;
            case 1006:
                baseAtk += 10;
                EventCenter.GetInstance().EventTrigger<float>("ATK_FRESH", CalculateFinalAtk());
                break;
            case 1007:
                baseAtk *= 1.1f;
                EventCenter.GetInstance().EventTrigger<float>("ATK_FRESH", CalculateFinalAtk());
                break;
            case 1008:
                baseAtk *= 1.3f;
                EventCenter.GetInstance().EventTrigger<float>("ATK_FRESH", CalculateFinalAtk());
                break;
            case 1009:
                baseAtk *= 1.5f;
                EventCenter.GetInstance().EventTrigger<float>("ATK_FRESH", CalculateFinalAtk());
                break;
        }
    }
    private void OnPlayerLevelUp(int levels)
    {
        // 将升级层数加入队列
        for (int i = 0; i < levels; i++)
        {
            rewardLevelQueue.Enqueue(1);
        }

        Debug.Log($"加入升级奖励队列：+{levels} 级，当前队列数：{rewardLevelQueue.Count}");

        // 尝试打开奖励面板
        TryOpenRewardPanel();
    }
    private void TryOpenRewardPanel()
    {
        // 确保游戏正在运行
        if (!GamingLifecycleMgr.IsRunning)
        {
            shouldOpenRewardWhenAvailable = true; // 标记等待时机
            return;
        }

        // 如果奖励面板已打开，退出
        if (isRewardPanelOpen)
        {
            Debug.Log("奖励面板已在打开状态，等待关闭后再处理");
            return;
        }

        // 如果没有奖励可领取，退出
        if (rewardLevelQueue.Count == 0)
        {
            Debug.Log("没有奖励可领取");
            shouldOpenRewardWhenAvailable = false;
            return;
        }

        // 如果游戏暂停了，先恢复游戏（处理商店等暂停状态）
        if (GamingLifecycleMgr.IsPaused)
        {
            Debug.Log("游戏处于暂停状态，恢复游戏");
            GamingLifecycleMgr.ResumeGame();
        }

        // 设置奖励面板状态
        isRewardPanelOpen = true;
        shouldOpenRewardWhenAvailable = false; // 清除等待标记

        Debug.Log("正在打开奖励面板");

        // 显示奖励面板
        UIManager.Instance.ShowPanel<RewardPanel>();
        // 打开成了 发送信息更新UI显示
        EventCenter.GetInstance().EventTrigger("UPDATE_LEVEL_SHOW");
        // 发送第一个奖励信息
        SendNextRewardInfo();
    }
    private void SendNextRewardInfo()
    {
        if (rewardLevelQueue.Count > 0)
        {
            int level = rewardLevelQueue.Dequeue();
            EventCenter.GetInstance().EventTrigger<int>("GET_LEVEL_INFO", level);
        }
    }
    private void OnRewardPanelClosed()
    {
        Debug.Log("奖励面板已关闭，当前队列数：" + rewardLevelQueue.Count);
        isRewardPanelOpen = false;

        // 检查是否有更多奖励需要处理
        if (rewardLevelQueue.Count > 0)
        {
            // 短暂延迟后尝试打开下一个奖励面板
            StartCoroutine(DelayTryOpenReward());
        }
        else if (shouldOpenRewardWhenAvailable)
        {
            // 如果标记了等待，也尝试打开（处理特殊情况）
            TryOpenRewardPanel();
        }
    }
    private IEnumerator DelayTryOpenReward()
    {
        yield return new WaitForSeconds(0.1f); // 短暂延迟确保UI清理完成
        TryOpenRewardPanel();
    }
    
    private void OnGameOverDo(bool v)
    {
        switch(v)
        {
            case false:
                //通知通关了，给我加金币
                if(gameSettlementCoroutine != null)
                    StopCoroutine(gameSettlementCoroutine);
                gameSettlementCoroutine = StartCoroutine(OnGameSettlement(false));
                break;
            case true:
                //失败也给加
                if (gameSettlementCoroutine != null)
                    StopCoroutine(gameSettlementCoroutine);
                gameSettlementCoroutine = StartCoroutine(OnGameSettlement(true));
                break;
        }

    }
    private IEnumerator OnGameSettlement(bool playerDead)
    {
        float rate = playerDead ? 0.04f : 0.2f;
        int reward = Mathf.FloorToInt(battleGold * rate);
        Debug.Log("添加金币"+reward);
        GameDataMgr.Instance.AddPlayerMoney(reward);
        GameDataMgr.Instance.SavePlayerData();
        yield return new WaitForSeconds(0.2f);
        //刷新状态
        FreshGamingState();
    }
    private void FreshGamingState()
    {
        LevelNodeMgr.GetInstance().Clear();
        NodeWaveSpawner.GetInstance().Clear();
        PoolMgr.GetInstance().Clear();
        UIManager.Instance.HidePanel<GamingPanel>();
        UIManager.Instance.HidePanel<ShopPanel>();
        UIManager.Instance.HidePanel<RewardPanel>();
    }
    private void OnEnterBattleNode(int v)//战斗节点
    {
        NodeWaveSpawner.GetInstance().StartSpawn(v);
    }
    private void OnEnterRestNode()//休息节点
    {
        EventCenter.GetInstance().EventTrigger<float>("PLAYER_HEAL_PERCENT", 0.5f);
        Debug.Log("成功恢复50%血量，准备进入下一节点");
        // 自动进入下一节点
        LevelNodeMgr.GetInstance().FinishCurNode();
        Debug.Log("结束当前节点，进入了下一节点");
    }
    private void OnEnterBOSSNode(int v)//BOSS节点
    {
        NodeWaveSpawner.GetInstance().SpawnBossGroup(v);
    }
    private void OnLeaveShop()
    {
        Debug.Log($"[DEBUG] OnLeaveShop");

        // 先恢复游戏（确保奖励面板可以打开）
        GamingLifecycleMgr.ResumeGame();

        // 不需要额外触发升级，因为已经在AddBuffToPlayer中处理了
        LevelNodeMgr.GetInstance().FinishCurNode();
    }
    private void OnFinishCurNode()
    {
        LevelNodeMgr.GetInstance().FinishCurNode();
    }
}
