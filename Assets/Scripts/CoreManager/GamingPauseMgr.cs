using System;
using System.Collections.Generic;
using UnityEngine;

public class GamingPauseMgr : BaseManager<GamingPauseMgr>
{
    public static bool isPause { get; private set; }
    public static event Action<bool> OnPauseChanged;

    public static void SetPause(bool pause)
    {
        if (isPause == pause) return;

        isPause = pause;
        OnPauseChanged?.Invoke(pause);
        //通知外部 当前状态是 暂停/进行
        if (isPause == true)
            EventCenter.GetInstance().EventTrigger<bool>("GAME_PAUSE",isPause);
        else
            EventCenter.GetInstance().EventTrigger<bool>("GAME_PAUSE", isPause);
    }

    public static void Pause()
    {
        isPause = true;
        EventCenter.GetInstance().EventTrigger<bool>("IS_PAUSE", true);
    }

    public static void Resume()
    {
        isPause = false;
        EventCenter.GetInstance().EventTrigger<bool>("IS_PAUSE", false);
    }
}