using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : BasePanel
{
    public Button BtnRegister;
    public Button BtnReset2;
    public Button BtnGoLogin;

    public Text TxtTip2;

    public InputField inputAccount2;
    public Text TxtTipUser2;
    public InputField inputPwd2;
    public Text TxtTipPwd2;
    public override void Init()
    {

    }

    private void Awake()
    {
        BtnRegister.onClick.AddListener(OnBtnRegisterDo);
        BtnReset2.onClick.AddListener(OnBtnReset2Do);
        BtnGoLogin.onClick.AddListener(OnBtnGoLoginDo);

        inputAccount2.onValueChanged.AddListener(OnAccount2ValueChanged);
        inputPwd2.onValueChanged.AddListener(OnPwd2ValueChanged);
        UpdateRegisterButtonState();
    }
    private void OnBtnRegisterDo()
    {
        var result = AccountMgr.GetInstance().Register(inputAccount2.text,inputPwd2.text);

        switch (result)
        {
            case AccountResult.Success:
                TxtTip2.text = "注册成功";
                break;
            case AccountResult.AccountExist:
                TxtTip2.text = "账号已存在";
                break;
            case AccountResult.AccountNull:
                TxtTip2.text = "请输入账号";
                break;
            case AccountResult.AccountTooShort:
                TxtTip2.text = "账号长度不能少于8位";
                break;
            case AccountResult.PasswordTooShort:
                TxtTip2.text = "密码长度不能少于6位";
                break;
            case AccountResult.PasswordNull:
                TxtTip2.text = "请输入密码";
                break;
        }
    }
    private void OnBtnReset2Do()
    {
        inputAccount2.text = null;
        inputPwd2.text = null;
    }
    private void OnBtnGoLoginDo()
    {
        UIManager.Instance.HidePanel<RegisterPanel>();
        UIManager.Instance.ShowPanel<LoginPanel>();
    }
    private void OnPwd2ValueChanged(string pwd)
    {
        UpdateRegisterButtonState();
    }
    private void OnAccount2ValueChanged(string account)
    {
        UpdateRegisterButtonState();
    }
    private void UpdateRegisterButtonState()
    {
        
        if (inputAccount2.text.Length >= 8)
            TxtTipUser2.text = "";
        else
            TxtTipUser2.text = "账号长度不小于8位";

        if (inputPwd2.text.Length >= 6)
            TxtTipPwd2.text = "";
        else
            TxtTipPwd2.text = "密码长度不小于6位";

        BtnRegister.interactable = (inputPwd2.text.Length >= 6);//inputAccount2.text.Length >= 8 &&
    }
}
