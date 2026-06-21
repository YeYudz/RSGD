using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 专门用来管理数据的类
/// </summary>
public class GameDataMgr 
{
    private static GameDataMgr instance=new GameDataMgr();
    public static GameDataMgr Instance=>instance;

    //音效相关数据
    public MusicData musicData;
    //角色数据
    public List<RoleInfo> roleInfoList;
    //敌人AI数据
    public List<EnemyInfo> enemyInfoList;
    //怪物数据
    public List<MonsterInfo> monsterInfoList;
    //子弹数据
    public List<BulletInfo> bulletInfoList;
    //经验相关数据
    public List<LevelInfo> levelInfoList;
    //节点相关数据
    public List<LevelNodeInfo> levelNodeInfoList;
    //怪物群组相关数据
    public List<MonsterGroupInfo> monsterGroupInfoList;
    //商品相关数据
    public List<ShopItemInfo> shopItemInfoList;
    //奖励相关数据
    public List<RewardItemInfo> rewardItemInfoList;
    //玩家相关数据
    public PlayerData playerData;
    public string CurAccountName { get; private set; } = "";


    private GameDataMgr()
    {
        //初始化一些默认数据
        musicData = JsonMgr.Instance.LoadData<MusicData>("MusicData");
        //获取角色 数据
        roleInfoList = JsonMgr.Instance.LoadData<List<RoleInfo>>("RoleInfo");
        //获取敌人AI 数据
        enemyInfoList = JsonMgr.Instance.LoadData<List<EnemyInfo>>("EnemyInfo");
        //获取怪物 数据
        monsterInfoList = JsonMgr.Instance.LoadData<List<MonsterInfo>>("MonsterInfo");
        //获取子弹 数据
        bulletInfoList = JsonMgr.Instance.LoadData<List<BulletInfo>>("BulletInfo");
        //获取经验 数据
        levelInfoList = JsonMgr.Instance.LoadData<List<LevelInfo>>("LevelInfo");
        //获取节点 数据
        levelNodeInfoList = JsonMgr.Instance.LoadData<List<LevelNodeInfo>>("LevelNodeInfo");
        //获取怪物群组 数据
        monsterGroupInfoList = JsonMgr.Instance.LoadData<List<MonsterGroupInfo>>("MonsterGroupInfo");
        //获取商品 数据
        shopItemInfoList = JsonMgr.Instance.LoadData<List<ShopItemInfo>>("ShopItemInfo");
        //获取奖励 数据
        rewardItemInfoList = JsonMgr.Instance.LoadData<List<RewardItemInfo>>("RewardItemInfo");
        //获取初始化 玩家数据
        playerData = null;

    }
    /// <summary>
    /// 登录账号
    /// </summary>
    /// <param name="accountName"></param>
    public void LoadPlayerData(string accountName)
    {
        CurAccountName = accountName;
        string fileName = "PlayerData_" + accountName;

        Debug.Log("[GameDataMgr] 加载玩家数据：" + fileName);

        PlayerData loadedData =
            JsonMgr.Instance.LoadData<PlayerData>(fileName);

        if (loadedData == null)
        {
            Debug.LogWarning("[GameDataMgr] 未找到存档，创建新玩家数据");
            playerData = new PlayerData();
            playerData.haveMoney = 500;
            playerData.haveCharacter.Add(0);
            SavePlayerData(); 
        }
        else
        {
            playerData = loadedData;
        }

        Debug.Log($"[GameDataMgr] 当前金币：{playerData.haveMoney}");
        EventCenter.GetInstance().EventTrigger("PLAYER_DATA_LOAD", playerData);
    }
    /// <summary>
    /// 退出账号
    /// </summary>
    public void ClearPlayerData()
    {
        playerData = null;
        CurAccountName = "";
    }
    /// <summary>
    /// 存储玩家数据
    /// </summary>
    public void SavePlayerData()
    {
        if (string.IsNullOrEmpty(CurAccountName))
        {
            Debug.LogError("[GameDataMgr] 未登录账号，禁止保存！");
            return;
        }

        string fileName = "PlayerData_" + CurAccountName;
        JsonMgr.Instance.SaveData(playerData, fileName);

        Debug.Log("[GameDataMgr] 玩家数据已保存：" + fileName);
    }
    /// <summary>
    /// 添加账号金币
    /// </summary>
    /// <param name="v"></param>
    public void AddPlayerMoney(int v)
    {
        if (playerData == null)
        {
            Debug.LogError("未登录玩家，不能加钱！");
            return;
        }
        playerData.haveMoney+=v;
        SavePlayerData();
    }
    /// <summary>
    /// 存储音效数据
    /// </summary>
    public void SaveMusicData()
    {
        JsonMgr.Instance.SaveData(musicData, "MusicData");
    }
    /// <summary>
    /// 播放音效方法
    /// </summary>
    /// <param name="resName"></param>
    public void PlaySound(string resName)
    {
        GameObject musicObj = new GameObject();
        AudioSource a = musicObj.AddComponent<AudioSource>();
        a.clip = Resources.Load<AudioClip>(resName);
        a.volume = musicData.soundValue;
        a.mute = !musicData.soundIsOpen;
        a.Play();

        GameObject.Destroy(musicObj, 1);
    }
}
