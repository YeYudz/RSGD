using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : BasePanel
{
    public Button BtnLogin;
    public Button BtnReset;
    public Button BtnGoRegister;
    public Toggle TogRemember;

    public Text TxtTip;

    public InputField inputAccount;
    public Text TxtTipUser;
    public InputField inputPwd;
    public Text TxtTipPwd;

    public override void Init()
    {

    }
    private void Awake()
    {
        BtnLogin.onClick.AddListener(OnBtnLoginDo);
        BtnReset.onClick.AddListener(OnBtnResetDo);
        BtnGoRegister.onClick.AddListener(OnBtnGoRegisterDo);

        inputAccount.onValueChanged.AddListener(OnAccountValueChanged);
        inputPwd.onValueChanged.AddListener(OnPwdValueChanged);
        if (TogRemember != null)
            TogRemember.onValueChanged.AddListener(OnRememberPasswordChanged);

        LoadLastLoginInfo();
        UpdateLoginButtonState();
    }
    private void OnBtnLoginDo()
    {
        var result = AccountMgr.GetInstance().Login(inputAccount.text,inputPwd.text);

        switch (result)
        {
            case AccountResult.Success:
                TxtTip.text = "";
                // 保存登录信息到PlayerPrefs
                SaveLoginInfo();

                UIManager.Instance.HidePanel<LoginPanel>();
                UIManager.Instance.HidePanel<LogAndRegPanel>();
                //EventCenter.GetInstance().Clear();
                PoolMgr.GetInstance().Clear();

                ScenesMgr.GetInstance().LoadSceneAsyn("MainMenu", () =>
                {
                    UIManager.Instance.ShowPanel<MainMenuPanel>();
                });

                EventCenter.GetInstance().EventTrigger("LOGIN_SUCCESS");
                break;
            case AccountResult.AccountNotExist:
                TxtTip.text = "账号不存在";
                break;
            case AccountResult.AccountTooShort:
                TxtTip.text = "账号格式不正确";
                break;
            case AccountResult.AccountNull:
                TxtTip.text = "请输入账号";
                break;
            case AccountResult.PasswordWrong:
                TxtTip.text = "密码错误";
                break;
            case AccountResult.PasswordNull:
                TxtTip.text = "请输入密码";
                break;
        }
    }
    private void OnBtnResetDo()
    {
        inputAccount.text = null;
        inputPwd.text = null;
    }
    private void OnBtnGoRegisterDo()
    {
       UIManager.Instance.HidePanel<LoginPanel>();
       UIManager.Instance.ShowPanel<RegisterPanel>();
    }
    private void SaveLoginInfo()// 保存登录信息到PlayerPrefs
    {
        // 保存账号总是必要的
        PlayerPrefs.SetString("LastLoginAccount", inputAccount.text);

        // 根据是否记住密码来决定是否保存密码
        if (TogRemember != null && TogRemember.isOn)
        {
            PlayerPrefs.SetInt("RememberPassword", 1);
            PlayerPrefs.SetString("LastLoginPassword", inputPwd.text);
        }
        else
        {
            PlayerPrefs.SetInt("RememberPassword", 0);
            // 如果不记住密码，则清空保存的密码
            PlayerPrefs.DeleteKey("LastLoginPassword");
        }

        PlayerPrefs.Save(); // 确保立即保存
    }
    private void LoadLastLoginInfo()
    {
        // 尝试从PlayerPrefs中获取保存的信息
        string lastAccount = PlayerPrefs.GetString("LastLoginAccount", "");
        bool rememberPassword = PlayerPrefs.GetInt("RememberPassword", 0) == 1; // 0为false, 1为true
        string lastPassword = "";

        if (rememberPassword)
        {
            lastPassword = PlayerPrefs.GetString("LastLoginPassword", "");
        }

        // 设置账号
        if (!string.IsNullOrEmpty(lastAccount))
        {
            inputAccount.text = lastAccount;
        }

        // 设置密码（如果选择了记住密码）
        if (rememberPassword && !string.IsNullOrEmpty(lastPassword))
        {
            inputPwd.text = lastPassword;
            if (TogRemember != null)
                TogRemember.isOn = true;
        }
        else if (TogRemember != null)
        {
            TogRemember.isOn = false;
        }

        UpdateLoginButtonState();
    }// 加载上次的登录信息
    private void OnRememberPasswordChanged(bool isOn)
    {
        UpdateLoginButtonState();
    }
    private void OnPwdValueChanged(string pwd)
    {
        UpdateLoginButtonState();
    }
    private void OnAccountValueChanged(string account)
    {
        UpdateLoginButtonState();
    }
    private void UpdateLoginButtonState()
    {
        
        if (inputAccount.text.Length >= 8)
            TxtTipUser.text = "";
        else
            TxtTipUser.text = "账号长度不小于8位";

        if (inputPwd.text.Length >= 6)
            TxtTipPwd.text = "";
        else
            TxtTipPwd.text = "密码长度不小于6位";

        BtnLogin.interactable = (inputPwd.text.Length >= 6);//inputAccount.text.Length >= 8 && 
    }
}
