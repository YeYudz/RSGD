using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    public Button BtnClose;
    public Button BtnBack;

    public Toggle TogMusic;
    public Toggle TogSound;

    public Slider SliMusic;
    public Slider SliSound;

    private int setting;
    private UnityAction onIMMainMenuAction;

    public override void Init()
    {
        InitDataShow();

        BtnClose.onClick.AddListener(OnBtnCloseDo);
        BtnBack.onClick.AddListener(OnBtnBackDo);
        TogMusic.onValueChanged.AddListener(OnTogMusic);
        TogSound.onValueChanged.AddListener(OnTogSound);
        SliMusic.onValueChanged.AddListener(OnSliMusic); 
        SliSound.onValueChanged.AddListener(OnSliSound);
    }

    private void Awake()
    {
        onIMMainMenuAction = () =>
        {
            setting = 1;
        };
        EventCenter.GetInstance().AddEventListener("IM_MAIN_MENU", onIMMainMenuAction);
        EventCenter.GetInstance().EventTrigger("SETTING_OPEN");
    }

    private void OnDestroy()
    {
        EventCenter.GetInstance().RemoveEventListener("IM_MAIN_MENU", onIMMainMenuAction);
    }

    private void InitDataShow()
    {
        MusicData data = GameDataMgr.Instance.musicData;

        TogMusic.isOn = data.musicIsOpen;
        TogSound.isOn = data.soundIsOpen;

        SliMusic.value = data.musicValue;
        SliSound.value = data.soundValue;
    }

    private void OnBtnCloseDo()
    {
        EventCenter.GetInstance().EventTrigger("SETTING_CLOSE");
        UIManager.Instance.HidePanel<SettingPanel>();
    }

    private void OnBtnBackDo()
    {
        PoolMgr.GetInstance().Clear();
        EventCenter.GetInstance().EventTrigger("SETTING_CLOSE_TO_MENU");
        UIManager.Instance.HidePanel<SettingPanel>();
        EventCenter.GetInstance().EventTrigger("Gaming Under Setting");
        ScenesMgr.GetInstance().LoadSceneAsyn("MainMenu", () =>
        {
            UIManager.Instance.ShowPanel<MainMenuPanel>();
        });
    }

    private void OnTogMusic(bool v)
    {
        BkMusic.Instance.SetIsOpen(v);
        GameDataMgr.Instance.musicData.musicIsOpen = v;
    }

    private void OnTogSound(bool v)
    {
        GameDataMgr.Instance.musicData.soundIsOpen = v;
    }

    private void OnSliMusic(float v)
    {
        BkMusic.Instance.ChangeValue(v);
        GameDataMgr.Instance.musicData.musicValue = v;
    }

    private void OnSliSound(float v)
    {
        GameDataMgr.Instance.musicData.soundValue = v;
    }
}
