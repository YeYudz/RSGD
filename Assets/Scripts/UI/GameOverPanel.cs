using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : BasePanel
{
    public Image ImgBackGround;
    public Text TxtTip;
    public Text TxtOverShow;
    public Button BtnSure;
    public override void Init()
    {

    }
    private void Awake()
    {
        EventCenter.GetInstance().AddEventListener("PLAYER_DIE",OnPlayerDieDo);
        EventCenter.GetInstance().AddEventListener("GAME_CLEARED",OnGameClearedDo);

        BtnSure.onClick.AddListener(OnBtnSureDo);
    }
    private void OnDestroy()
    {

        EventCenter.GetInstance().RemoveEventListener("PLAYER_DIE", OnPlayerDieDo);
        EventCenter.GetInstance().RemoveEventListener("GAME_CLEARED", OnGameClearedDo);
    }
    private void OnBtnSureDo()
    {
        PoolMgr.GetInstance().Clear();
        UIManager.Instance.HidePanel<GameOverPanel>();
        UIManager.Instance.HidePanel<GamingPanel>(); 
        UIManager.Instance.HidePanel<ShopPanel>();
        UIManager.Instance.HidePanel<RewardPanel>();

        ScenesMgr.GetInstance().LoadSceneAsyn("MainMenu", () =>
        {
            UIManager.Instance.ShowPanel<MainMenuPanel>();
        });
        EventCenter.GetInstance().EventTrigger("GAME_OVER_CONFIRMED");
    }
    private void OnPlayerDieDo()
    {
        TxtOverShow.text = "游戏结束\n" + "玩家阵亡";
    }
    private void OnGameClearedDo()
    {
        TxtOverShow.text = "游戏结束\n" + "关卡通过";
    }
}
