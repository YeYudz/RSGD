using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class StorePanel : BasePanel
{
    public Transform RoleContent;
    public GameObject roleItemPrefab;
    public List<RoleInfo> allRoles;

    public Button BtnBuy;
    public Text TxtBuy;
    public Button BtnClose;
    public Text TxtMoney;
    public Text TxtTip;

    // 技能描述显示
    public Text TxtSkillEName;
    public Text TxtSkillEDescription;
    public Text TxtSkillQName;
    public Text TxtSkillQDescription;

    //当前选中——使用的角色 的数据
    private RoleInfo nowRoleData;
    //当前 角色 数据的 索引值
    private int curSelectRoleId = 99;

    public override void Init()
    {
    }

    private new void Start()
    {
        allRoles = GameDataMgr.Instance.roleInfoList;
        RefreshMoneyUI();
        GenerateRoleList();
    }

    private void Awake()
    {
        OnSelectRole(curSelectRoleId);
        EventCenter.GetInstance().AddEventListener<int>("STORE_SELECT_ROLE", OnSelectRole);
        EventCenter.GetInstance().AddEventListener<int>("STORE_BUY_SUCCESS", OnBuySuccess);
        EventCenter.GetInstance().AddEventListener("STORE_BUY_FAIL", OnBuyFail);

        BtnBuy.onClick.AddListener(OnBuyClick);
        BtnClose.onClick.AddListener(OnBtnCloseDo);
    }

    private void OnDestroy()
    {
        EventCenter.GetInstance().RemoveEventListener<int>("STORE_SELECT_ROLE", OnSelectRole);
        EventCenter.GetInstance().RemoveEventListener<int>("STORE_BUY_SUCCESS", OnBuySuccess);
        EventCenter.GetInstance().RemoveEventListener("STORE_BUY_FAIL", OnBuyFail);
    }

    private void GenerateRoleList()
    {
        foreach (var role in allRoles)
        {
            GameObject item = Instantiate(roleItemPrefab, RoleContent);
            item.GetComponent<Image>().sprite = Resources.Load<Sprite>(role.res);
            Debug.Log("角色路径加载" + role.res);
            bool unlocked = GameDataMgr.Instance.playerData.haveCharacter.Contains(role.id);
            //绑定点击事件
            Button btn = item.GetComponent<Button>();
            btn.onClick.AddListener(() => OnHeroHeadClicked(role));
        }
    }

    private void OnHeroHeadClicked(RoleInfo selectedHero)
    {
        Debug.Log("选择英雄: " + selectedHero.name);
        EventCenter.GetInstance().EventTrigger<int>("STORE_SELECT_ROLE", selectedHero.id);
    }

    private void OnSelectRole(int roleId)
    {
        curSelectRoleId = roleId;
        Debug.Log("执行更新角色选中后，角色id为" + curSelectRoleId);
        //默认选空
        if (curSelectRoleId > GameDataMgr.Instance.roleInfoList.Count)
            return;
        RoleInfo info = GameDataMgr.Instance.roleInfoList[roleId];

        bool owned = GameDataMgr.Instance.playerData.haveCharacter.Contains(roleId);

        BtnBuy.GetComponentInChildren<Text>().text = owned ? "已拥有" : $"购买\n({info.price})";
        Debug.Log($"roleId={roleId}, count={GameDataMgr.Instance.playerData.haveCharacter.Count}");
        BtnBuy.interactable = !owned;

        // 更新技能描述
        UpdateSkillDescription(info);
    }

    private void UpdateSkillDescription(RoleInfo info)
    {
        if (TxtSkillEName != null && info.skillE != null)
        {
            TxtSkillEName.text = $"E技能：{info.skillE.name}";
            TxtSkillEDescription.text = info.skillE.description;
        }

        if (TxtSkillQName != null && info.skillQ != null)
        {
            TxtSkillQName.text = $"Q技能：{info.skillQ.name}";
            TxtSkillQDescription.text = info.skillQ.description;
        }
    }

    private void OnBuyClick()
    {
        if (curSelectRoleId == 99)
        {
            Debug.Log("未选择角色");
            return;
        }

        EventCenter.GetInstance().EventTrigger<int>("STORE_TRY_BUY_ROLE", curSelectRoleId);
    }

    private void RefreshMoneyUI()
    {
        TxtMoney.text = $"金币:{GameDataMgr.Instance.playerData.haveMoney}";
    }

    private void OnBtnCloseDo()
    {
        UIManager.Instance.HidePanel<StorePanel>();
        UIManager.Instance.ShowPanel<MainMenuPanel>();
    }

    private void OnBuySuccess(int roleId)
    {
        OnSelectRole(roleId);
        RefreshMoneyUI();
        OnTipFresh(true);
    }

    private void OnBuyFail()
    {
        OnTipFresh(false);
    }

    private void OnTipFresh(bool v)
    {
        if (v)
            TxtTip.text = "购买成功！";
        else
            TxtTip.text = "金币不足！";
    }
}