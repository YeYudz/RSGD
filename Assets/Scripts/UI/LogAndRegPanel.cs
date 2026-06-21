using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogAndRegPanel : BasePanel
{
    public Button BtnBack;
    public Button BtnQuit;

    public override void Init()
    {

    }
    private void Awake()
    {
        BtnBack.onClick.AddListener(OnBtnBackDo);
        BtnQuit.onClick.AddListener(OnBtnQuitDo); 

    }
    private void OnBtnBackDo()
    {
        UIManager.Instance.HidePanel<LogAndRegPanel>();
        UIManager.Instance.HidePanel<RegisterPanel>();
        UIManager.Instance.HidePanel<LoginPanel>();
        ScenesMgr.GetInstance().LoadSceneAsyn("Init", () =>
        {
            UIManager.Instance.ShowPanel<InitPanel>();
        });
    }
    private void OnBtnQuitDo()
    {
        Application.Quit();
    }
}
