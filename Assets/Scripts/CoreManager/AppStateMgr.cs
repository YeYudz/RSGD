using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppStateMgr : MonoBehaviour
{
    public static AppStateMgr Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
    }
    private void Start()
    {
        EventCenter.GetInstance().PrintAllEvents();
    }

    /// <summary>
    /// 外部唯一调用：进入游戏
    /// </summary>
    public void EnterGame()
    {
        GamingLifecycleMgr.StartGame();
        LevelNodeMgr.GetInstance().StartGame();

    }

    /// <summary>
    /// 外部唯一调用：退出游戏
    /// </summary>
    public void ExitGame()
    {
        GamingLifecycleMgr.ShutdownGame();
    }
}
