using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
//数据索引值 的变换--引起数据 的变换--引起信息 的更新
public class CharacterSelectPanel : BasePanel
{
    public Button BtnBack;
    public Button BtnSure;
    public Button BtnRole11;
    public Button BtnRole12; 
    public Button BtnRole13;
    public Button BtnRole14;

    public Image ImgRole;

    public GameObject characterHeadImgPrefab;
    public Transform contentParent;
    public List<RoleInfo> allRoleInfoListData;

    //当前选中——使用的角色 的数据
    private RoleInfo nowRoleData;
    //当前 角色 数据的 索引值
    private int nowIndex = 99;
    new private void Start()
    {
        allRoleInfoListData = GameDataMgr.Instance.roleInfoList;
        GenerateHeroList();
    }
    private void Awake()
    {
        ChangeHero(nowIndex);
        EventCenter.GetInstance().RemoveEventListener<int>("CHANGE_SELECT_HERO",ChangeHero);
        EventCenter.GetInstance().AddEventListener<int>("CHANGE_SELECT_HERO", ChangeHero);
        BtnBack.onClick.AddListener(OnBtnBackDo);
        BtnSure.onClick.AddListener(OnBtnSureDo);
    }
    public override void Init()
    {
       
    }
    private void OnDestroy()
    {
        EventCenter.GetInstance().RemoveEventListener<int>("CHANGE_SELECT_HERO", ChangeHero);
    }
    /// <summary>
    /// 改变Panel中显示的角色信息
    /// </summary>
    private void ChangeHero(int index)
    {
        nowIndex= index;
        Debug.Log("执行改角色请求，角色id："+nowIndex);
        //默认置空
        if (nowIndex > GameDataMgr.Instance.roleInfoList.Count)
            return;
        //创建  角色信息
        nowRoleData = GameDataMgr.Instance.roleInfoList[nowIndex];//取出数据 赋值索引值
        //实例化对象 并记录       用于下次切换时 删除
        ImgRole.sprite = Resources.Load<Sprite>(nowRoleData.res);
        
    }
    private void OnBtnBackDo()
    {
        UIManager.Instance.HidePanel<CharacterSelectPanel>();
        PoolMgr.GetInstance().Clear();
        ScenesMgr.GetInstance().LoadSceneAsyn("MainMenu", () =>
        {
            UIManager.Instance.ShowPanel<MainMenuPanel>();
        });
    }
    private void OnBtnSureDo()
    {
        if (nowIndex == 99)
        {
            Debug.Log("请选择角色！");
            return;
        }

        UIManager.Instance.HidePanel<CharacterSelectPanel>();
        PoolMgr.GetInstance().Clear();
        ScenesMgr.GetInstance().LoadSceneAsyn("Gaming1", () =>
        {
            StartCoroutine(ShowGamingPanelAfterFrame());
            OnChangeCursor();
        });
    }
    private IEnumerator ShowGamingPanelAfterFrame()
    {
        yield return null; // 等待 1 帧（场景加载完成后的第一帧）
        Debug.Log("延迟 1 帧后显示 GamingPanel");
        UIManager.Instance.ShowPanel<GamingPanel>();
        //把所选角色的信息，传出去
        Debug.Log("触发，赋值使用的角色，事件" + nowIndex);
        EventCenter.GetInstance().EventTrigger<int>("CHOOSED_HERO_ID", nowIndex);
    }
    private void GenerateHeroList()
    {
        foreach (Transform child in contentParent)
        {
            if (child.gameObject.activeSelf) Destroy(child.gameObject);
        }

        foreach (int roleId in GameDataMgr.Instance.playerData.haveCharacter)
        {
            var role = GameDataMgr.Instance.roleInfoList[roleId];

            GameObject item = Instantiate(characterHeadImgPrefab, contentParent);
            item.GetComponent<Image>().sprite =Resources.Load<Sprite>(role.res);

            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnHeroHeadClicked(roleId);
            });
        }
    }
    private void OnHeroHeadClicked(int id)
    {
        Debug.Log("选择了英雄: " + id);
        EventCenter.GetInstance().EventTrigger<int>("SELECT_HERO", id);
    }
    private void OnChangeCursor()
    {
        // 锁定鼠标到屏幕中心
        Cursor.lockState = CursorLockMode.Locked;
        // 隐藏鼠标指针
        Cursor.visible = false;
    }
}
