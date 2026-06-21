using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : MonoBehaviour
{
    public float roundSpeed = 480;//灵敏度
    public Animator animator;
    public RoleInfo roleInfo;
    public Transform shootPoint;

    private CharacterController cc;
    private Vector3 rootMotionDelta;

    private bool haveBulletCounts = true;

    // 技能状态
    private bool isInfiniteAmmoState = false;  // 无限弹药状态（角色3的Q）
    private bool isMultiShotState = false;     // 多重射击状态（角色4的Q）

    // 射击状态控制
    private bool isShooting = false;          // 是否正在射击（防止重复开火）
    private bool isReloading = false;         // 是否正在换弹

    // 技能状态公共属性（供外部访问）
    public bool IsInfiniteAmmoState => isInfiniteAmmoState;
    public bool IsMultiShotState => isMultiShotState;

    private void Awake()
    {
        Debug.Log("玩家创建，添加监听");
        animator = this.GetComponent<Animator>();
        cc = this.GetComponent<CharacterController>();
        EventCenter.GetInstance().AddEventListener<RoleInfo>("PLAYER_SPAWN", Init);
        EventCenter.GetInstance().AddEventListener<float>("ATK_FRESH", OnAtkFresh);
        EventCenter.GetInstance().AddEventListener("PLAYER_IS_WOUNDED", Wounded);
        EventCenter.GetInstance().AddEventListener("PLAYER_RELOAD", Reload);
        EventCenter.GetInstance().AddEventListener("NO_SOUND",OnNoSound);
        EventCenter.GetInstance().AddEventListener<bool>("GAME_PAUSE", OnPause);

        // 技能状态监听
        EventCenter.GetInstance().AddEventListener<bool>("INFINITE_AMMO_STATE", OnInfiniteAmmoState);
        EventCenter.GetInstance().AddEventListener<bool>("MULTI_SHOT_STATE", OnMultiShotState);
    }

    public void Init(RoleInfo info)
    {
        roleInfo = info;
    }
    void Start()
    {
        if (cc == null)
        {
            Debug.LogError("PlayerObject 缺少 CharacterController！");
        }

        EventCenter.GetInstance().EventTrigger<PlayerObject>("PLAYER_SPAWNED", this);
    }
    private void OnDisable()
    {
        Debug.Log("玩家禁用，清除事件");
        EventCenter.GetInstance().RemoveEventListener<RoleInfo>("PLAYER_SPAWN", Init);
        EventCenter.GetInstance().RemoveEventListener<float>("ATK_FRESH", OnAtkFresh);
        EventCenter.GetInstance().RemoveEventListener("PLAYER_IS_WOUNDED", Wounded);
        EventCenter.GetInstance().RemoveEventListener("PLAYER_RELOAD", Reload);
        EventCenter.GetInstance().RemoveEventListener("NO_SOUND", OnNoSound);
        EventCenter.GetInstance().RemoveEventListener<bool>("GAME_PAUSE", OnPause);

        // 移除技能状态监听
        EventCenter.GetInstance().RemoveEventListener<bool>("INFINITE_AMMO_STATE", OnInfiniteAmmoState);
        EventCenter.GetInstance().RemoveEventListener<bool>("MULTI_SHOT_STATE", OnMultiShotState);
    }
    void Update()
    {
        if (GamingStateMgr.IsFrozen) return;

        // 控制旋转
        transform.Rotate(Vector3.up,Input.GetAxis("Mouse X") * roundSpeed * Time.deltaTime*2f);

        // 输入传给 Animator
        animator.SetFloat("VSpeed", Input.GetAxis("Vertical"));
        animator.SetFloat("HSpeed", Input.GetAxis("Horizontal"));
    }
    private void OnAnimatorMove()
    {
        if (GamingStateMgr.IsFrozen) return;
        // 使用动画的 RootMotion 位移
        rootMotionDelta = animator.deltaPosition;

        // 防止上下坡出现奇怪位移
        if (rootMotionDelta.y != 0)
            rootMotionDelta.y = 0;

        // 通过 CharacterController 移动
        if (cc != null)
        {
            cc.Move(rootMotionDelta);
        }

        // 防止被怪物挤上天，限制Y轴位置
        Vector3 pos = transform.position;
        if (pos.y > 0.5f) // 假设地面高度为0，允许轻微浮动
        {
            pos.y = 0.5f;
            transform.position = pos;
        }
    }
    public void AtkEvent()
    {
        if (roleInfo == null || GamingStateMgr.IsFrozen) return;

        // 如果正在射击或正在换弹，不允许再次开火
        if (isShooting || isReloading)
            return;

        switch (roleInfo.type)
        {
            case 1:
                KnifeEvent();
                break;
            case 2:
                // 标记为正在射击
                isShooting = true;
                // 射击时禁用换弹
                isReloading = false;
                animator.SetBool("ISFIRING", true);
                break;
        }
    }
    public void KnifeEvent()
    {
        EventCenter.GetInstance().EventTrigger("PLAYER_KNIFE_ATTACK");
        GameDataMgr.Instance.PlaySound("Music/Knife");
        animator.SetBool("ISFIRING", false);
    }
    public void ShootEvent()
    {
        // 播放射击音效
        OnPlayShootSound(roleInfo.bulletId);

        // 计算实际生成的子弹数量
        int actualCount = roleInfo.shootCount;

        // 角色4的Q技能：多重射击状态，每消耗1发子弹额外生成2发（只生成不消耗）
        if (isMultiShotState)
        {
            actualCount = roleInfo.shootCount + 2; // 原始数量 + 额外2发
        }

        EventCenter.GetInstance().EventTrigger<SpawnBulletParam>("SPAWN_PLAYER_BULLET",
            new SpawnBulletParam
            {
                bulletId = roleInfo.bulletId,
                count = roleInfo.shootCount,      // 发送原始数量用于消耗
                extraCount = isMultiShotState ? 2 : 0, // 额外生成的子弹数量
                shootPoint = this.shootPoint,
                shootDir = Vector3.zero,
                owner = BulletOwner.Player,
                atk = roleInfo.atk,
            });

        // 射击完成
        animator.SetBool("ISFIRING", false);
        isShooting = false;
    }
    private void OnPlayShootSound(int id)
    {
        // 无限弹药状态下不检查haveBulletCounts
        if (!haveBulletCounts && !isInfiniteAmmoState)
            return;
        switch (id)
        {
            case 1:
                GameDataMgr.Instance.PlaySound("Music/Gun");
                break;
            case 2:
                GameDataMgr.Instance.PlaySound("Music/Kin");
                break;
            case 3:
                GameDataMgr.Instance.PlaySound("Music/Tower");
                break;
            case 4:
                GameDataMgr.Instance.PlaySound("Music/Tower");
                break;
        }
    }
    public Vector3 GetShootDirection()
    {
        return transform.forward;
    }
    public void Wounded()
    {
        // 双重判空保护
        if (this == null || animator == null || GamingStateMgr.IsFrozen) return;
        animator.SetTrigger("WOUND");
        GameDataMgr.Instance.PlaySound("Music/Wound");
    }
    public void Reload()
    {
        // 正在射击时不允许换弹
        if (isShooting || isReloading)
            return;

        isReloading = true;
        //先执行换弹动作
        animator.SetTrigger("RELOAD");
        animator.SetBool("ISRELOADING", true);
        Debug.Log("已经开始执行换弹动作");
    }
    public void ReloadEvent()
    {
        if (GamingStateMgr.IsFrozen)
            return;
        animator.SetBool("ISRELOADING", false);
        haveBulletCounts = true;
        isReloading = false;  // 重置换弹状态
        Debug.Log("已经结束换弹");
        //做完换弹动作后  发送通知   通知系统   给我把子弹装上   并更新UI显示
        EventCenter.GetInstance().EventTrigger<RoleInfo>("FINISH_RELOAD",roleInfo);//换弹结束后 应该处理的逻辑
        Debug.Log("触发了换弹结束后的逻辑处理");
    }
    private void OnPause(bool pause)
    {
        if (animator == null) return;

        animator.speed = pause ? 0 : 1;
    }
    private void OnAtkFresh(float newAtk)
    {
        if (roleInfo != null)
        {
            roleInfo.atk = newAtk;
        }
    }
    private void OnNoSound()
    {
        haveBulletCounts = false;
    }

    // 技能状态处理
    private void OnInfiniteAmmoState(bool isActive)
    {
        isInfiniteAmmoState = isActive;
        if (isActive)
        {
            Debug.Log("进入无限弹药状态");
        }
        else
        {
            Debug.Log("退出无限弹药状态");
        }
    }

    private void OnMultiShotState(bool isActive)
    {
        isMultiShotState = isActive;
        if (isActive)
        {
            Debug.Log("进入多重射击状态");
        }
        else
        {
            Debug.Log("退出多重射击状态");
        }
    }
}