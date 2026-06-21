using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreMgr : MonoBehaviour
{
    private void Awake()
    {
        //商店面板买角色
        EventCenter.GetInstance().AddEventListener<int>("STORE_TRY_BUY_ROLE", TryBuyRole);
    }
    private void OnDestroy()
    {
        EventCenter.GetInstance().RemoveEventListener<int>("STORE_TRY_BUY_ROLE", TryBuyRole);
    }
    private void TryBuyRole(int roleId)
    {
        var player = GameDataMgr.Instance.playerData;
        var role = GameDataMgr.Instance.roleInfoList.Find(g => g.id == roleId);
        Debug.Log("已得到要购买的角色id"+role.id);

        if (player.haveCharacter.Contains(roleId))
        {
            Debug.Log("已拥有，直接返回"+role.id);
            return;
        }
            

        if (player.haveMoney >= role.price)
        {
            player.haveMoney -= role.price;
            player.haveCharacter.Add(roleId);
            Debug.Log("购买成功" + role.id);
            GameDataMgr.Instance.SavePlayerData();
            EventCenter.GetInstance().EventTrigger<int>("STORE_BUY_SUCCESS", roleId);//刷新余额按钮
            Debug.Log("通知系统保存数据，刷新显示");
        }
        else
        {
            EventCenter.GetInstance().EventTrigger("STORE_BUY_FAIL");
            Debug.Log("钱不够！");
        }
    }
}
