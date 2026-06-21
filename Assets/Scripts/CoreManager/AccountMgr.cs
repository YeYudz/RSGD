using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum AccountResult
{
    Success,

    AccountNotExist,
    PasswordWrong,

    AccountExist,
    AccountTooShort,
    AccountNull,

    PasswordTooShort,
    PasswordNull,
}
public class AccountMgr : BaseManager<AccountMgr>
{
    // 当前登录账号
    public string CurAccountName { get; private set; }

    // 所有账号列表（只存账号名，真实数据按文件名区分）
    private List<AccountData> accountList = new List<AccountData>();

    public AccountMgr()
    {
        // 加载账号列表
        accountList = JsonMgr.Instance.LoadData<List<AccountData>>("AccountList");
        if (accountList == null)
            accountList = new List<AccountData>();
    }

    /// <summary>
    /// 注册账号
    /// </summary>
    public AccountResult Register(string account, string pwd)
    {
        if (account.Length == 0)
            return AccountResult.AccountNull;
        else if (account.Length < 8)
            return AccountResult.AccountTooShort;
        else if (pwd.Length == 0)
            return AccountResult.PasswordNull;

        // 账号长度校验
        if (account.Length < 8)
            return AccountResult.AccountTooShort;
        // 密码校验
        if (pwd.Length < 6)
            return AccountResult.PasswordTooShort;
        // 账号重复校验
        if (accountList.Exists(a => a.account == account))
            return AccountResult.AccountExist;

        // 创建账号
        accountList.Add(new AccountData { account = account, password = pwd });
        JsonMgr.Instance.SaveData(accountList, "AccountList");

        // 创建玩家数据
        PlayerData pd = new PlayerData();
        pd.haveMoney = 500;
        pd.haveCharacter.Add(0);
        JsonMgr.Instance.SaveData(pd, GetPlayerDataFileName(account));

        return AccountResult.Success;
    }

    /// <summary>
    /// 登录账号
    /// </summary>
    public AccountResult Login(string account, string pwd)
    {
        if(account.Length ==0)
            return AccountResult.AccountNull;
        else if(account.Length<8)
            return AccountResult.AccountTooShort;
        else if (pwd.Length == 0)
            return AccountResult.PasswordNull;

            var acc = accountList.Find(a => a.account == account);
        //if (acc.account.Length == 0)
        //    return AccountResult.AccountNull;
        if (acc == null)
            return AccountResult.AccountNotExist;
        if (acc.password != pwd)
            return AccountResult.PasswordWrong;

        CurAccountName = account;
        GameDataMgr.Instance.LoadPlayerData(account);
        return AccountResult.Success;
    }

    /// <summary>
    /// 登出
    /// </summary>
    public void Logout()
    {
        CurAccountName = "";
        GameDataMgr.Instance.ClearPlayerData();
    }

    private string GetPlayerDataFileName(string account)
    {
        return "PlayerData_" + account;
    }
}
