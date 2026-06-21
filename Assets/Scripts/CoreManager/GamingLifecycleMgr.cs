using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GamingLifecycleMgr : MonoBehaviour
{
    public static GamingLifecycleMgr Instance{get; private set;}
    public static bool IsRunning { get; private set; }
    public static bool IsPaused { get; private set; }
    public static bool IsGameActive =>IsRunning && !IsPaused;
    public static bool CanInput => IsRunning && !IsPaused;

    public static event Action OnGameStart;
    public static event Action OnGamePause;
    public static event Action OnGameResume;
    public static event Action OnGameShutdown;
    private void Awake()
    {
        Debug.Log("进入游戏场景");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("GamingLifecycleMgr创建，监听事件");
        // 先清一次，防止残留
        RemoveAllListeners();
        //监听事件
        EventCenter.GetInstance().AddEventListener("GAME_OVER_CONFIRMED", ShutdownGame);
        EventCenter.GetInstance().AddEventListener("REQUEST_SETTINGS", OnRequestSettings);
        EventCenter.GetInstance().AddEventListener("SETTING_CLOSE", OnSettingClose);
        EventCenter.GetInstance().AddEventListener("SETTING_CLOSE_TO_MENU", OnSettingCloseToMenu);
        EventCenter.GetInstance().AddEventListener("ENTER_SHOP_NODE", OnEnterShop);
        EventCenter.GetInstance().AddEventListener("SHOP_LEAVE", OnShopLeave);
        EventCenter.GetInstance().AddEventListener("REWARD_OPEN", OnRewardOpen);
        EventCenter.GetInstance().AddEventListener("REWARD_OVER", OnRewardOver);
        EventCenter.GetInstance().AddEventListener("OVER_PANEL_OPEN", OnOverPanelOpen);
        Debug.Log($"GamingLifecycleMgr 拿到的 EventCenter Hash: {EventCenter.GetInstance().GetHashCode()}");
    }
    private void OnDestroy()
    {
        Debug.Log("退出游戏场景，GamingLifecycleMgr销毁，清空事件");
        RemoveAllListeners();
    }
    public static void StartGame()
    {
        IsRunning = true;
        IsPaused = false;

        GamingPauseMgr.SetPause(false);
        MouseStateMgr.GetInstance().Reset();
        InputMgr.GetInstance().StartOrEndCheck(true);
        if (TimeMgr.Instance != null)
            TimeMgr.Instance.StartTimer();

        OnGameStart?.Invoke();
    }
    public static void PauseGame()
    {
        if (!IsRunning || IsPaused) return;

        IsPaused = true;
        GamingPauseMgr.SetPause(true);
        InputMgr.GetInstance().StartOrEndCheck(false);
        if (TimeMgr.Instance != null)
            TimeMgr.Instance.PauseTimer();

        OnGamePause?.Invoke();
    }
    public static void ResumeGame()
    {
        if (!IsRunning || !IsPaused) return;

        IsPaused = false;
        GamingPauseMgr.SetPause(false);
        InputMgr.GetInstance().StartOrEndCheck(true);
        if (TimeMgr.Instance != null)
            TimeMgr.Instance.ResumeTimer();

        OnGameResume?.Invoke();
    }
    public static void ShutdownGame()
    {
        bool wasRunning = IsRunning;
        IsRunning = false;
        IsPaused = false;

        InputMgr.GetInstance().StartOrEndCheck(false);
        GamingPauseMgr.SetPause(false);
        MouseStateMgr.GetInstance().Reset();
        if (TimeMgr.Instance != null)
            TimeMgr.Instance.ResetTimer();

        if (wasRunning)
            OnGameShutdown?.Invoke();
    }
    private void OnRequestSettings()
    {
        Debug.Log($"OnRequestSettings 被调用 | IsRunning={IsRunning} | IsPaused={IsPaused}");
        Debug.Log("接收到打开设置面板请求");

        if (!IsRunning) return;

        if (IsPaused)
        {
            UIManager.Instance.ShowPanel<SettingPanel>();
            EventCenter.GetInstance().EventTrigger("Gaming Hide UI");
            Debug.Log("设置面板已show");
            MouseStateMgr.GetInstance().RequireMouse();
            return;
        }

        PauseGame();
        UIManager.Instance.ShowPanel<SettingPanel>();
        EventCenter.GetInstance().EventTrigger("Gaming Hide UI");
        Debug.Log("设置面板已show");
        MouseStateMgr.GetInstance().RequireMouse();
    }
    private void OnSettingClose()
    {
        ResumeGame();
        MouseStateMgr.GetInstance().ReleaseMouse();
    }
    private void OnSettingCloseToMenu()
    {
        ShutdownGame();
        MouseStateMgr.GetInstance().RequireMouse();
    }
    private void OnEnterShop()
    {
        if (!IsRunning) return;

        if (IsPaused)
        {
            UIManager.Instance.ShowPanel<ShopPanel>();
            MouseStateMgr.GetInstance().RequireMouse();
            return;
        }
        PauseGame();
        UIManager.Instance.ShowPanel<ShopPanel>();
        MouseStateMgr.GetInstance().RequireMouse();
    }
    private void OnShopLeave()
    {
        ResumeGame();
        MouseStateMgr.GetInstance().ReleaseMouse();
    }
    private void OnRewardOpen()
    {
        if (!IsRunning) return;

        if (IsPaused)
        {
            MouseStateMgr.GetInstance().RequireMouse();
            return;
        }
        PauseGame();
        MouseStateMgr.GetInstance().RequireMouse();
    }
    private void OnRewardOver()
    {
        ResumeGame();
        MouseStateMgr.GetInstance().ReleaseMouse();
    }
    private void OnOverPanelOpen()
    {
        if (!IsRunning) return;

        if (IsPaused)
        {
            UIManager.Instance.ShowPanel<GameOverPanel>();
            MouseStateMgr.GetInstance().RequireMouse();
            return;
        }
        PauseGame();
        UIManager.Instance.ShowPanel<GameOverPanel>();
        MouseStateMgr.GetInstance().RequireMouse();
    }
    private void RemoveAllListeners()
    {
        EventCenter.GetInstance().RemoveEventListener("GAME_OVER_CONFIRMED", ShutdownGame);
        EventCenter.GetInstance().RemoveEventListener("REQUEST_SETTINGS", OnRequestSettings);
        EventCenter.GetInstance().RemoveEventListener("SETTING_CLOSE", OnSettingClose);
        EventCenter.GetInstance().RemoveEventListener("SETTING_CLOSE_TO_MENU", OnSettingCloseToMenu);
        EventCenter.GetInstance().RemoveEventListener("ENTER_SHOP_NODE", OnEnterShop);
        EventCenter.GetInstance().RemoveEventListener("SHOP_LEAVE", OnShopLeave);
        EventCenter.GetInstance().RemoveEventListener("REWARD_OPEN", OnRewardOpen);
        EventCenter.GetInstance().RemoveEventListener("REWARD_OVER", OnRewardOver);
        EventCenter.GetInstance().RemoveEventListener("OVER_PANEL_OPEN", OnOverPanelOpen);
    }
}
