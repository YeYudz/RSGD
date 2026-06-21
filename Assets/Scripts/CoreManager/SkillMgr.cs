using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillMgr : MonoBehaviour
{
    public static SkillMgr Instance { get; private set; }

    private RoleInfo currentRole;
    private PlayerObject player;

    // 技能状态
    private bool isSkillEActive = false;
    private bool isSkillQActive = false;
    private bool isSkillEOnCooldown = false;
    private bool isSkillQOnCooldown = false;

    // 技能计时器
    private float skillERemainingTime = 0f;
    private float skillQRemainingTime = 0f;
    private float skillECooldownTimer = 0f;
    private float skillQCooldownTimer = 0f;

    // 技能效果状态
    private float originalAtk = 0f;
    private float originalAtkRange = 0f;
    private Transform weaponTransform;

    // 当前激活的技能攻击力加成倍率（乘法叠加）
    private float skillEAtkMultiplier = 1f;
    private float skillQAtkMultiplier = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        EventCenter.GetInstance().AddEventListener<RoleInfo>("PLAYER_SPAWN", OnPlayerSpawn);
        EventCenter.GetInstance().AddEventListener("USE_SKILL_E", UseSkillE);
        EventCenter.GetInstance().AddEventListener("USE_SKILL_Q", UseSkillQ);
    }

    private void OnDestroy()
    {
        // 停止所有协程
        StopAllCoroutines();

        // 重置冷却状态
        isSkillEOnCooldown = false;
        isSkillQOnCooldown = false;
        isSkillEActive = false;
        isSkillQActive = false;

        EventCenter.GetInstance().RemoveEventListener<RoleInfo>("PLAYER_SPAWN", OnPlayerSpawn);
        EventCenter.GetInstance().RemoveEventListener("USE_SKILL_E", UseSkillE);
        EventCenter.GetInstance().RemoveEventListener("USE_SKILL_Q", UseSkillQ);
    }

    private void OnPlayerSpawn(RoleInfo roleInfo)
    {
        currentRole = roleInfo;
        player = FindObjectOfType<PlayerObject>();

        if (player != null)
        {
            originalAtk = roleInfo.atk;
            originalAtkRange = roleInfo.atkRange;

            // 获取武器Transform（用于角色2的Q技能）
            Transform[] children = player.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (child.name.Contains("Weapon") || child.name.Contains("weapon"))
                {
                    weaponTransform = child;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        if (GamingStateMgr.IsFrozen || currentRole == null) return;

        // 更新E技能持续时间
        if (isSkillEActive)
        {
            skillERemainingTime -= Time.deltaTime;
            if (skillERemainingTime <= 0f)
            {
                DeactivateSkillE();
            }
        }

        // 更新Q技能持续时间
        if (isSkillQActive)
        {
            skillQRemainingTime -= Time.deltaTime;
            if (skillQRemainingTime <= 0f)
            {
                DeactivateSkillQ();
            }
        }

        // 更新E技能冷却
        if (isSkillEOnCooldown)
        {
            skillECooldownTimer -= Time.deltaTime;
            if (skillECooldownTimer <= 0f)
            {
                isSkillEOnCooldown = false;
                skillECooldownTimer = 0f;
                EventCenter.GetInstance().EventTrigger<bool>("SKILL_E_COOLDOWN", false);
            }
            else
            {
                float progress = skillECooldownTimer / currentRole.skillE.cooldown;
                EventCenter.GetInstance().EventTrigger<float>("SKILL_E_COOLDOWN_PROGRESS", progress);
            }
        }

        // 更新Q技能冷却
        if (isSkillQOnCooldown)
        {
            skillQCooldownTimer -= Time.deltaTime;
            if (skillQCooldownTimer <= 0f)
            {
                isSkillQOnCooldown = false;
                skillQCooldownTimer = 0f;
                EventCenter.GetInstance().EventTrigger<bool>("SKILL_Q_COOLDOWN", false);
            }
            else
            {
                float progress = skillQCooldownTimer / currentRole.skillQ.cooldown;
                EventCenter.GetInstance().EventTrigger<float>("SKILL_Q_COOLDOWN_PROGRESS", progress);
            }
        }
    }

    // 使用E技能
    private void UseSkillE()
    {
        if (currentRole == null || currentRole.skillE == null) return;

        // 检查冷却
        if (isSkillEOnCooldown)
        {
            Debug.Log("E技能冷却中！");
            return;
        }

        Debug.Log($"使用E技能：{currentRole.skillE.name}");

        // 根据技能类型应用效果
        ApplySkillEffect(currentRole.skillE, false);

        // 激活技能
        isSkillEActive = true;
        skillERemainingTime = currentRole.skillE.duration;

        // 进入冷却
        isSkillEOnCooldown = true;
        skillECooldownTimer = currentRole.skillE.cooldown;
        EventCenter.GetInstance().EventTrigger<bool>("SKILL_E_COOLDOWN", true);

        // 触发UI更新
        EventCenter.GetInstance().EventTrigger<string>("SKILL_E_USED", currentRole.skillE.name);
    }

    // 使用Q技能
    private void UseSkillQ()
    {
        if (currentRole == null || currentRole.skillQ == null) return;

        // 检查冷却
        if (isSkillQOnCooldown)
        {
            Debug.Log("Q技能冷却中！");
            return;
        }

        Debug.Log($"使用Q技能：{currentRole.skillQ.name}");

        // 根据技能类型应用效果
        ApplySkillEffect(currentRole.skillQ, true);

        // 激活技能
        isSkillQActive = true;
        skillQRemainingTime = currentRole.skillQ.duration;

        // Q技能：立即设置遮罩为1（满冷却状态），持续时间结束后才开始倒计时
        EventCenter.GetInstance().EventTrigger<float>("SKILL_Q_COOLDOWN_PROGRESS", 1f);

        // 进入冷却（持续时间结束后）
        MonoMgr.GetInstance().StartCoroutine(StartSkillQCooldownAfterDuration());

        // 触发UI更新
        EventCenter.GetInstance().EventTrigger<string>("SKILL_Q_USED", currentRole.skillQ.name);
    }

    private IEnumerator StartSkillQCooldownAfterDuration()
    {
        // 等待技能持续时间结束
        yield return new WaitForSeconds(currentRole.skillQ.duration);

        // 持续时间结束后，开始冷却倒计时
        isSkillQOnCooldown = true;
        skillQCooldownTimer = currentRole.skillQ.cooldown;

        // 持续更新冷却进度，直到冷却结束
        while (skillQCooldownTimer > 0f)
        {
            // 使用真实时间，确保暂停时不会继续倒计时
            yield return new WaitForSecondsRealtime(0.05f);

            if (GamingStateMgr.IsFrozen || currentRole == null)
                continue;

            skillQCooldownTimer -= 0.05f;
            if (skillQCooldownTimer <= 0f)
            {
                skillQCooldownTimer = 0f;
                isSkillQOnCooldown = false;
                EventCenter.GetInstance().EventTrigger<float>("SKILL_Q_COOLDOWN_PROGRESS", 0f);
            }
            else
            {
                float progress = skillQCooldownTimer / currentRole.skillQ.cooldown;
                EventCenter.GetInstance().EventTrigger<float>("SKILL_Q_COOLDOWN_PROGRESS", progress);
            }
        }
    }

    // 应用技能效果
    private void ApplySkillEffect(SkillInfo skill, bool isQSkill)
    {
        switch (skill.type)
        {
            case SkillType.ATK_BOOST:
                if (isQSkill)
                {
                    skillQAtkMultiplier = 1f + skill.atkBonus;
                }
                else
                {
                    skillEAtkMultiplier = 1f + skill.atkBonus;
                }
                UpdateTotalAtkBonus();
                break;

            case SkillType.ATK_BOUST_HP_COST:
                float hpCost = originalAtk * skill.hpCost;
                EventCenter.GetInstance().EventTrigger<float>("PLAYER_HURT", hpCost);

                if (isQSkill)
                {
                    skillQAtkMultiplier = 1f + skill.atkBonus;
                }
                else
                {
                    skillEAtkMultiplier = 1f + skill.atkBonus;
                }
                UpdateTotalAtkBonus();

                if (weaponTransform != null)
                {
                    weaponTransform.localScale = Vector3.one * skill.weaponScale;
                    EventCenter.GetInstance().EventTrigger<float>("ATK_RANGE_FRESH", originalAtkRange * skill.weaponScale);
                }
                break;

            case SkillType.INFINITE_AMMO:
                EventCenter.GetInstance().EventTrigger<bool>("INFINITE_AMMO_STATE", true);
                break;

            case SkillType.MULTI_SHOT:
                EventCenter.GetInstance().EventTrigger<bool>("MULTI_SHOT_STATE", true);
                break;
        }
    }

    private void UpdateTotalAtkBonus()
    {
        float totalMultiplier = skillEAtkMultiplier * skillQAtkMultiplier;
        float totalBonus = totalMultiplier - 1f;
        EventCenter.GetInstance().EventTrigger<float>("SKILL_ATK_BONUS", totalBonus);
        Debug.Log($"技能攻击力加成更新：E倍率={skillEAtkMultiplier}, Q倍率={skillQAtkMultiplier}, 总倍率={totalMultiplier}, 总加成={totalBonus}");
    }

    // 停用E技能
    private void DeactivateSkillE()
    {
        if (!isSkillEActive) return;

        isSkillEActive = false;

        switch (currentRole.skillE.type)
        {
            case SkillType.ATK_BOOST:
            case SkillType.ATK_BOUST_HP_COST:
                skillEAtkMultiplier = 1f;
                UpdateTotalAtkBonus();
                break;
        }

        Debug.Log("E技能效果结束");
    }

    // 停用Q技能
    private void DeactivateSkillQ()
    {
        if (!isSkillQActive) return;

        isSkillQActive = false;

        switch (currentRole.skillQ.type)
        {
            case SkillType.ATK_BOOST:
                skillQAtkMultiplier = 1f;
                UpdateTotalAtkBonus();
                break;

            case SkillType.ATK_BOUST_HP_COST:
                skillQAtkMultiplier = 1f;
                UpdateTotalAtkBonus();

                if (weaponTransform != null)
                {
                    weaponTransform.localScale = Vector3.one;
                    EventCenter.GetInstance().EventTrigger<float>("ATK_RANGE_FRESH", originalAtkRange);
                }
                break;

            case SkillType.INFINITE_AMMO:
                EventCenter.GetInstance().EventTrigger<bool>("INFINITE_AMMO_STATE", false);
                break;

            case SkillType.MULTI_SHOT:
                EventCenter.GetInstance().EventTrigger<bool>("MULTI_SHOT_STATE", false);
                break;
        }

        Debug.Log("Q技能效果结束");
    }

    // 获取技能冷却进度（用于UI显示）
    public float GetSkillECooldownProgress()
    {
        if (!isSkillEOnCooldown) return 1f;
        return 1f - (skillECooldownTimer / currentRole.skillE.cooldown);
    }

    public float GetSkillQCooldownProgress()
    {
        if (!isSkillQOnCooldown) return 1f;
        return 1f - (skillQCooldownTimer / currentRole.skillQ.cooldown);
    }

    // 检查技能是否可用
    public bool IsSkillEAvailable()
    {
        return !isSkillEOnCooldown && currentRole != null && currentRole.skillE != null;
    }

    public bool IsSkillQAvailable()
    {
        return !isSkillQOnCooldown && currentRole != null && currentRole.skillQ != null;
    }
}