using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public static TestScript Instance { get; private set; }

    private PlayerObject player;
    private bool locking = true;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        EventCenter.GetInstance().AddEventListener<PlayerObject>("PLAYER_SPAWNED", OnPlayerSpawned);
    }
    private void OnDestroy()
    {
        EventCenter.GetInstance().RemoveEventListener<PlayerObject>("PLAYER_SPAWNED", OnPlayerSpawned);
        EventCenter.GetInstance().RemoveEventListener<object>("某键按下", CheckInputDown);
    }
    void Start()
    {
        Debug.Log($"TestScript 拿到的 EventCenter Hash: {EventCenter.GetInstance().GetHashCode()}");
        EventCenter.GetInstance().AddEventListener<object>("某键按下", CheckInputDown);
    }

    private void CheckInputDown(object key)
    {
        KeyCode code = (KeyCode)key;
        switch(code)
        {
            case KeyCode.E:
                // 触发E技能
                EventCenter.GetInstance().EventTrigger("USE_SKILL_E");
                break;
            case KeyCode.Q:
                // 触发Q技能
                EventCenter.GetInstance().EventTrigger("USE_SKILL_Q");
                break;
            case KeyCode.R:
                EventCenter.GetInstance().EventTrigger("PLAYER_RELOAD");
                break;
            case KeyCode.Mouse0:
                if (player == null)
                    return;
                // 检查是否正在射击或正在换弹
                if (player.animator.GetBool("ISFIRING") || player.animator.GetBool("ISRELOADING"))
                    return;
                player.animator.SetTrigger("FIRE");
                player.animator.SetBool("ISFIRING", true);
                break;
            case KeyCode.Escape:
                Debug.Log("准备发送打开设置面板请求");
                EventCenter.GetInstance().EventTrigger("REQUEST_SETTINGS");
                Debug.Log("打开了设置面板");
                break;
            case KeyCode.LeftAlt:
                OnCursorDo(locking);
                break;
            default:
                break;
        }
    }
    private void OnPlayerSpawned(PlayerObject p)
    {
        player = p;
        Debug.Log("TestScript 成功绑定 PlayerObject");
    }
    public void OnCursorDo(bool v)
    {
        switch(v)
        {
            case true:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                locking = false;
                break;
            case false:
                // 锁定鼠标到屏幕中心
                Cursor.lockState = CursorLockMode.Locked;
                // 隐藏鼠标指针
                Cursor.visible = false;
                locking = true;
                break;
        }
    }
}
