using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public struct ShopItem
{
    public int cardId;   // 第几个选项（1~3）
    public int itemId;   // 祝福ID
}

public class ShopPanel : BasePanel
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

    public Button BtnBuy;
    public Button BtnClose;
    public Text TxtNowMoney;
    public Text TxtTip;

    public List<ShopItemInfo> allItemInfoListData;
    public int haveMoney=0;

    // 当前商店生成的 3 个祝福 ID
    private List<int> currentShopItemIds = new List<int>();

    // 当前选中的祝福索引（0~2）
    private int selectedIndex = -1;

    private void OnDestroy()
    {
        // 移除本面板注册的所有事件监听
        EventCenter.GetInstance().RemoveEventListener<int>("HAVE_GOLD", UpdateHaveMoney);
    }
    private void Awake()
    {
        Init();
        EventCenter.GetInstance().AddEventListener<int>("HAVE_GOLD", UpdateHaveMoney);

        //告诉外部 我显示出来了  给我传金币数量
        EventCenter.GetInstance().EventTrigger("SHOP_IS_SHOWED");
        BtnClose.onClick.AddListener(OnBtnCloseDo);
        BtnBuy.onClick.AddListener(OnBtnBuyDo);

        GenerateShopItems();
    }
    public override void Init()
    {
        allItemInfoListData = GameDataMgr.Instance.shopItemInfoList;
        if (allItemInfoListData == null)
        {
            Debug.LogError("❌ GameDataMgr 里的 shopItemInfoList 是空的！请检查 GameDataMgr 的加载逻辑。");
            allItemInfoListData = new List<ShopItemInfo>();
        }
    }
    private void GenerateShopItems()
    {
        currentShopItemIds.Clear();
        selectedIndex = -1;

        var tempList = new List<ShopItemInfo>(allItemInfoListData);

        for (int i = 0; i < 3 && tempList.Count > 0; i++)
        {
            int randIndex = Random.Range(0, tempList.Count);
            var info = tempList[randIndex];

            currentShopItemIds.Add(info.id);
            tempList.RemoveAt(randIndex);

            // 绑定按钮
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

            txt.text = $"{info.txtTip}\n消耗: {info.price}";

            int capturedIndex = i;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnItemSelected(capturedIndex));
        }

        UpdateHaveMoney(haveMoney);
        TxtTip.text = "请选择一个祝福";
    }
    
    private void OnItemSelected(int index)
    {
        if (index < 0 || index >= currentShopItemIds.Count)
            return;

        selectedIndex = index;

        EventCenter.GetInstance().EventTrigger<ShopItem>("选商品",new ShopItem
        {
            cardId = index + 1,
            itemId = currentShopItemIds[index]
        });

        TxtTip.text = $"已选择：{allItemInfoListData.Find(x => x.id == currentShopItemIds[index]).txtTip}";
    }

    private void OnBtnBuyDo()
    {
        if (selectedIndex == -1)
        {
            TxtTip.text = "请先选择一个祝福";
            return;
        }

        int itemId = currentShopItemIds[selectedIndex];
        ShopItemInfo info = allItemInfoListData.Find(x => x.id == itemId);

        if (info == null)
        {
            TxtTip.text = "数据错误";
            return;
        }

        if (haveMoney < info.price)
        {
            TxtTip.text = "金币不足";
            return;
        }

        // 扣钱
        haveMoney -= info.price;
        UpdateHaveMoney(haveMoney);
        //通知UImgr 同步扣钱
        EventCenter.GetInstance().EventTrigger<int>("BUY_ITEM",info.price);

        // 触发 Buff
        Debug.Log("选了id为" + info.id + "的祝福，效果是" + info.txtTip); 
        if (info.levelBonus > 0)
        {
            EventCenter.GetInstance().EventTrigger<int>("SHOP_UPGRADE_LEVEL",info.levelBonus);
        }
        EventCenter.GetInstance().EventTrigger<int>("GET_BUFF", itemId);

        // 失活该选项
        DisableSelectedItem();

        TxtTip.text = "购买成功！";
    }

    private void DisableSelectedItem()
    {
        var btn = selectedIndex == 0 ? BtnItem1 :
                  selectedIndex == 1 ? BtnItem2 :
                                       BtnItem3;

        btn.interactable = false;
        selectedIndex = -1;
    }

    private void OnBtnCloseDo()
    {
        TxtTip.text = null;
        UIManager.Instance.HidePanel<ShopPanel>();

        EventCenter.GetInstance().EventTrigger("SHOP_LEAVE");
    }

    
    private void UpdateHaveMoney(int v)
    {
        if (this == null || gameObject == null)
        {
            Debug.LogWarning("ShopPanel 已被销毁，跳过 UpdateHaveMoney");
            return;
        }
        //数值上 更新
        haveMoney =v;
        //UI上 更新
        TxtNowMoney.text= haveMoney.ToString();
    }
}