using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleChangeMgr :MonoBehaviour
{
    private void Awake()
    {
        EventCenter.GetInstance().AddEventListener<int>("SELECT_HERO", OnChangeCharacterDo);
        Debug.Log("监听选角色事件成功");
    }
    private void OnDestroy()
    {
        EventCenter.GetInstance().RemoveEventListener<int>("SELECT_HERO", OnChangeCharacterDo);
    }
    private void OnChangeCharacterDo(int index)
    {
        Debug.Log("选中了角色索引：" + index+"发送改角色请求");
        EventCenter.GetInstance().EventTrigger<int>("CHANGE_SELECT_HERO", index);
    }
}
