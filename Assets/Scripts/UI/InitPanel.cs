using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InitPanel : BasePanel
{
    public Button btnStart;
    public override void Init()
    {
        btnStart.onClick.AddListener(OnBtnStartDo);
    }
    private void OnBtnStartDo()
    {
        UIManager.Instance.HidePanel<InitPanel>();
        PoolMgr.GetInstance().Clear();
        ScenesMgr.GetInstance().LoadSceneAsyn("Login", () =>
        {
            UIManager.Instance.ShowPanel<LogAndRegPanel>();
            UIManager.Instance.ShowPanel<LoginPanel>();
        });
    }

}
