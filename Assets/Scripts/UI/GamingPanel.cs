using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamingPanel : BasePanel
{
    public Button BtnStop;

    public Image ImgMap;
    public Text GamingTime;

    public Image ImgCharacterHead;
    public Image ImgHpBottom;
    public Image ImgHpTop;

    public float nowAtk;
    public Text TxtAtk;

    public Image ImgExpBottom;
    public Image ImgExpTop;

    public int nowLevel=1;
    public Text TxtLevel;

    public Text TexMoney;
    int sumMoney;

    public Image ImgWeapon;
    public Text TexBulletCount;

    public Image ImgSkill1;
    public Image ImgSkill2;

    // 技能冷却遮罩
    public Image ImgSkill1Cooldown;  // E技能冷却遮罩
    public Image ImgSkill2Cooldown;  // Q技能冷却遮罩

    // 游戏结束协程
    private Coroutine gameOverCoroutine;

    //当前选中——使用的角色 的数据
    private RoleInfo nowRoleData;

    public override void Init()
    {
        BtnStop.onClick.AddListener(OnBtnStopDo);
    }
    private void Awake()
    {
        Debug.Log("注册监听事件GamingPanel初始化的事件");
        // 基础UI事件 - 直接监听原始事件
        EventCenter.GetInstance().AddEventListener<int>("CHOOSED_HERO_ID", InitUI);
        EventCenter.GetInstance().AddEventListener<float>("UI_HP_UPDATE", UpdateUI);
        EventCenter.GetInstance().AddEventListener<int>("UI_GOLD_UPDATE", OnGoldReceived);
        EventCenter.GetInstance().AddEventListener<int>("WEAPON_BULLET_COUNTS_UPDATE", UpdateBulletCount);
        EventCenter.GetInstance().AddEventListener<float>("UI_EXP_UPDATE", UpdateExpUI);
        EventCenter.GetInstance().AddEventListener("UPDATE_LEVEL_SHOW", UpdateLevelShow);
        EventCenter.GetInstance().AddEventListener<float>("ATK_FRESH", OnAtkFresh);
        // 商店购买事件
        EventCenter.GetInstance().AddEventListener<int>("BUY_ITEM", OnBuyItem);
        // 游戏结束事件
        EventCenter.GetInstance().AddEventListener<bool>("GAME_OVER", OnGameOver);
        //接收ShopPanel是否显示 显示了后,修改这个金币显示
        EventCenter.GetInstance().AddEventListener("SHOP_IS_SHOWED", UpdateShopShow);

        // 技能冷却事件监听
        EventCenter.GetInstance().AddEventListener<bool>("SKILL_E_COOLDOWN", OnSkillECooldown);
        EventCenter.GetInstance().AddEventListener<float>("SKILL_E_COOLDOWN_PROGRESS", OnSkillECooldownProgress);
        EventCenter.GetInstance().AddEventListener<bool>("SKILL_Q_COOLDOWN", OnSkillQCooldown);
        EventCenter.GetInstance().AddEventListener<float>("SKILL_Q_COOLDOWN_PROGRESS", OnSkillQCooldownProgress);
    }
    private void OnDestroy()
    {
        Debug.Log("移除监听事件GamingPanel初始化的事件");
        // 基础UI事件
        EventCenter.GetInstance().RemoveEventListener<int>("CHOOSED_HERO_ID", InitUI);
        EventCenter.GetInstance().RemoveEventListener<float>("UI_HP_UPDATE", UpdateUI);
        EventCenter.GetInstance().RemoveEventListener<int>("UI_GOLD_UPDATE", OnGoldReceived);
        EventCenter.GetInstance().RemoveEventListener<int>("WEAPON_BULLET_COUNTS_UPDATE", UpdateBulletCount);
        EventCenter.GetInstance().RemoveEventListener<float>("UI_EXP_UPDATE", UpdateExpUI);
        EventCenter.GetInstance().RemoveEventListener("UPDATE_LEVEL_SHOW", UpdateLevelShow);
        EventCenter.GetInstance().RemoveEventListener<float>("ATK_FRESH", OnAtkFresh);
        // 商店购买事件
        EventCenter.GetInstance().RemoveEventListener<int>("BUY_ITEM", OnBuyItem);
        // 游戏结束事件
        EventCenter.GetInstance().RemoveEventListener<bool>("GAME_OVER", OnGameOver);
        //接收ShopPanel是否显示 显示了后,修改这个金币显示
        EventCenter.GetInstance().RemoveEventListener("SHOP_IS_SHOWED", UpdateShopShow);

        // 移除技能冷却事件监听
        EventCenter.GetInstance().RemoveEventListener<bool>("SKILL_E_COOLDOWN", OnSkillECooldown);
        EventCenter.GetInstance().RemoveEventListener<float>("SKILL_E_COOLDOWN_PROGRESS", OnSkillECooldownProgress);
        EventCenter.GetInstance().RemoveEventListener<bool>("SKILL_Q_COOLDOWN", OnSkillQCooldown);
        EventCenter.GetInstance().RemoveEventListener<float>("SKILL_Q_COOLDOWN_PROGRESS", OnSkillQCooldownProgress);
    }
    private void OnBtnStopDo()
    {
        EventCenter.GetInstance().EventTrigger("REQUEST_SETTINGS");
    }
    private void InitUI(int index)
    {
        //接收角色ID对应的Index
        nowRoleData = GameDataMgr.Instance.roleInfoList[index];//获取角色 对应信息
        Debug.Log("执行刷新UI" + nowRoleData.id + nowRoleData.res);
        ImgCharacterHead.sprite = Resources.Load<Sprite>(nowRoleData.res);
        ImgSkill1.sprite = Resources.Load<Sprite>(nowRoleData.res);
        ImgSkill2.sprite = Resources.Load<Sprite>(nowRoleData.res);
        ImgWeapon.sprite = Resources.Load<Sprite>(nowRoleData.weaponRes);
        TexBulletCount.text = nowRoleData.bulletCounts.ToString() + " / " + nowRoleData.bulletCounts.ToString();

        //初始化 金币值=0
        sumMoney = 0;
        TexMoney.text = "金币:" + sumMoney.ToString();
    }
    private void UpdateUI(float v)
    {
        v = Mathf.Clamp01(v);
        ImgHpTop.fillAmount = v * 1f;
    }
    private void OnGoldReceived(int v)
    {
        sumMoney += v;
        //更新金币显示
        TexMoney.text = "金币:" + sumMoney.ToString();
    }
    private void OnBuyItem(int price)
    {
        // 购买物品时扣除金币
        sumMoney -= price;
        TexMoney.text = "金币:" + sumMoney.ToString();
    }
    private void UpdateBulletCount(int v)
    {
        if(nowRoleData==null)
        {
            Debug.Log("未获取到nowRoleData信息");
            return;
        }
        TexBulletCount.text = v.ToString() + " / " + nowRoleData.bulletCounts.ToString();
    }
    private void UpdateExpUI(float v)
    {
        v= Mathf.Clamp01(v);
        ImgExpTop.fillAmount = v*1f;
    }
    private void UpdateShopShow()
    {
        //我的金币 同步
        EventCenter.GetInstance().EventTrigger<int>("HAVE_GOLD", sumMoney);
    }
    private void UpdateLevelShow()
    {
        //v是增加的等级
        nowLevel += 1;
        TxtLevel.text = "LV:" + nowLevel.ToString();
    }
    private void OnAtkFresh(float v)
    {
        TxtAtk.text = "攻击力:"+v.ToString();
    }

    // 技能冷却显示
    private void OnSkillECooldown(bool isOnCooldown)
    {
        if (ImgSkill1Cooldown != null)
        {
            ImgSkill1Cooldown.gameObject.SetActive(isOnCooldown);
            if (!isOnCooldown)
            {
                ImgSkill1Cooldown.fillAmount = 0f;
            }
        }
    }

    private void OnSkillECooldownProgress(float progress)
    {
        if (ImgSkill1Cooldown != null)
        {
            // 确保遮罩激活（即使之前被隐藏了）
            ImgSkill1Cooldown.gameObject.SetActive(true);
            ImgSkill1Cooldown.fillAmount = progress;
        }
    }

    private void OnSkillQCooldown(bool isOnCooldown)
    {
        if (ImgSkill2Cooldown != null)
        {
            ImgSkill2Cooldown.gameObject.SetActive(isOnCooldown);
            if (!isOnCooldown)
            {
                ImgSkill2Cooldown.fillAmount = 0f;
            }
        }
    }

    private void OnSkillQCooldownProgress(float progress)
    {
        if (ImgSkill2Cooldown != null)
        {
            // 确保遮罩激活（即使之前被隐藏了）
            ImgSkill2Cooldown.gameObject.SetActive(true);
            ImgSkill2Cooldown.fillAmount = progress;
        }
    }

    // 游戏结束处理
    private void OnGameOver(bool playerDead)
    {
        // 打开游戏结束面板
        EventCenter.GetInstance().EventTrigger("OVER_PANEL_OPEN");

        // 停止之前的协程
        if (gameOverCoroutine != null)
            StopCoroutine(gameOverCoroutine);

        // 启动延迟显示协程
        gameOverCoroutine = StartCoroutine(UpdateGameOverShow(playerDead));
    }

    private IEnumerator UpdateGameOverShow(bool playerDead)
    {
        yield return new WaitForSeconds(0.2f);
        if (playerDead)
        {
            EventCenter.GetInstance().EventTrigger("PLAYER_DIE");
        }
        else
        {
            EventCenter.GetInstance().EventTrigger("GAME_CLEARED");
        }
    }
}
