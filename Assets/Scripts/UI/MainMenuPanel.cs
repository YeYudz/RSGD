using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuPanel : BasePanel
{
    public Button BtnBegin;
    public Button BtnSetting;
    public Button BtnQuit;
    public Button BtnStore;
    public Button BtnLogOut;
    public override void Init()
    {
        BtnBegin.onClick.AddListener(OnBtnBeginDo);
        BtnSetting.onClick.AddListener(OnBtnSettingDo);
        BtnQuit.onClick.AddListener(OnBtnQuitDo);
        BtnStore.onClick.AddListener(OnBtnStoreDo);
        BtnLogOut.onClick.AddListener(OnBtnLogOutDo);
    }

    private void OnBtnBeginDo()
    {
        UIManager.Instance.HidePanel<MainMenuPanel>();
        PoolMgr.GetInstance().Clear();
        ScenesMgr.GetInstance().LoadSceneAsyn("CharacterSelect", () =>
        {
            UIManager.Instance.ShowPanel<CharacterSelectPanel>();
        });
    }
    private void OnBtnSettingDo()
    {
        EventCenter.GetInstance().AddEventListener("SETTING_OPEN",()=>
        {
            EventCenter.GetInstance().EventTrigger("IM_MAIN_MENU");
        });
        UIManager.Instance.ShowPanel<SettingPanel>();
    }
    private void OnBtnQuitDo()
    {
        Application.Quit();
    }
    private void OnBtnStoreDo()
    {
        UIManager.Instance.HidePanel<MainMenuPanel>();
        UIManager.Instance.ShowPanel<StorePanel>();
    }
    private void OnBtnLogOutDo()
    {
        GameDataMgr.Instance.ClearPlayerData();
        UIManager.Instance.HidePanel<MainMenuPanel>();
        ScenesMgr.GetInstance().LoadSceneAsyn("Login", () =>
        {
            UIManager.Instance.ShowPanel<LogAndRegPanel>();
            UIManager.Instance.ShowPanel<LoginPanel>();
        });
    }
}
