using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardPanel : BasePanel
{
    /* ===== UI 引用 ===== */
    public Button BtnItem1;
    public Button BtnItem2;
    public Button BtnItem3;

    public Image ImgItem1;
    public Image ImgItem2;
    public Image ImgItem3;

    public Text TxtItem1;
    public Text TxtItem2;
    public Text TxtItem3;

    public Button BtnSure;
    public Button BtnClose;
    public Button BtnFresh;
    public Text TxtFreshCounts;
    public Text TxtTip;

    public List<RewardItemInfo> allItemInfoListData;

    private List<int> currentRewardItemIds = new List<int>();
    private int selectedIndex = -1;

    // 核心参数
    private int upgradeLevel = 0;
    private int freshCount = 0;
    // 本轮是否已经刷新过
    private bool hasRefreshedThisRound = false;
    private void OnDestroy()
    {
        // 移除本面板注册的所有事件监听
        EventCenter.GetInstance().RemoveEventListener<int>("GET_LEVEL_INFO", OnUpgradeLevel);
    }
    private void Awake()
    {
        Init();

        EventCenter.GetInstance().AddEventListener<int>("GET_LEVEL_INFO", OnUpgradeLevel);

        //告诉外部 我显示出来了  可以给我传信息数据了
        EventCenter.GetInstance().EventTrigger("REWARD_OPEN");
        BtnSure.onClick.AddListener(OnBtnSureDo);
        BtnFresh.onClick.AddListener(OnBtnFreshDo);

        GenerateRewardItems();

        transform.SetAsLastSibling();//置顶
    }
    public override void Init()
    {

        allItemInfoListData = GameDataMgr.Instance.rewardItemInfoList;
        if (allItemInfoListData == null)
        {
            Debug.LogError("❌ GameDataMgr 里的 rewardItemInfoList 是空的！请检查 GameDataMgr 的加载逻辑。");
            allItemInfoListData = new List<RewardItemInfo>();
        }
    }
    private void OnUpgradeLevel(int level)
    {
        upgradeLevel = level;
        freshCount = upgradeLevel;
        UpdateFreshShow(freshCount);
        GenerateRewardItems();
    }
    private void GenerateRewardItems()
    {
        currentRewardItemIds.Clear();
        selectedIndex = -1;

        var tempList = new List<RewardItemInfo>(allItemInfoListData);

        for (int i = 0; i < 3 && tempList.Count > 0; i++)
        {
            int randIndex = Random.Range(0, tempList.Count);
            var info = tempList[randIndex];
            currentRewardItemIds.Add(info.id);
            tempList.RemoveAt(randIndex);

            var btn = i == 0 ? BtnItem1 :
                      i == 1 ? BtnItem2 :
                               BtnItem3;

            var txt = i == 0 ? TxtItem1 :
                      i == 1 ? TxtItem2 :
                               TxtItem3;

            var img = i == 0 ? ImgItem1 :
                      i == 1 ? ImgItem2 :
                               ImgItem3;

            img.sprite = Resources.Load<Sprite>(info.itemPicRes);
            txt.text = info.txtTip;

            int capturedIndex = i;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnItemSelected(capturedIndex));
        }

        TxtTip.text = "请选择一个奖励";
    }
    private void OnItemSelected(int index)
    {
        selectedIndex = index;
        var info = allItemInfoListData.Find(
            x => x.id == currentRewardItemIds[index]
        );
        TxtTip.text = $"已选择：{info.txtTip}";
    }
    private void OnBtnSureDo()
    {
        if (selectedIndex == -1)
        {
            TxtTip.text = "请先选择一个奖励";
            return;
        }

        int itemId = currentRewardItemIds[selectedIndex];

        // 触发 Buff
        EventCenter.GetInstance().EventTrigger<int>("GET_BUFF", itemId + 1000);

        // 消耗一次升级机会
        upgradeLevel--;

        if (upgradeLevel <= 0)
        {
            ClosePanel();
            return;
        }

        hasRefreshedThisRound = false; // 新一轮，允许刷新一次
                                       // 重新生成
        GenerateRewardItems();
    }
    private void OnBtnFreshDo()
    {
        if (freshCount <= 0)
        {
            TxtTip.text = "没有剩余次数";
            return;
        }
        if (hasRefreshedThisRound)
        {
            TxtTip.text = "本轮已经刷新过，请先确定";
            return;
        } 
        // 第一次刷新
        hasRefreshedThisRound = true;

        freshCount--;
        UpdateFreshShow(freshCount);

        GenerateRewardItems();
    }
    private void ClosePanel()
    {
        UIManager.Instance.HidePanel<RewardPanel>();

        // 通知 GamingStateMgr 奖励面板已关闭
        EventCenter.GetInstance().EventTrigger("REWARD_PANEL_CLOSED");
        EventCenter.GetInstance().EventTrigger("REWARD_OVER");
        Debug.Log("触发了奖励结束事件，通知GamingStateMgr");
    }
    private void UpdateFreshShow(int v)
    {
        TxtFreshCounts.text = "刷新\n" + $"剩余次数:{v}";
    }
}
