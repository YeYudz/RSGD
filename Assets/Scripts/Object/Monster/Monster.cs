using System.Collections.Generic;
using System.Collections;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    [Header("状态")]
    public MonsterInfo monsterCfg;
    public float curHp;
    private bool isInitialized;
    private bool isDead;
    private bool isBorn;
    private Coroutine hpBarCoroutine;
    private float hpBarHideDelay = 3f;

    public Transform shootPoint;

    [Header("组件")]
    private NavMeshAgent agent;
    private Animator animator;
    private Rigidbody rigidbody; // ✅ 添加Rigidbody用于碰撞控制
    public Transform player;
    public Canvas canvasHp;
    private Transform hpBarTransform;

    public Image ImgHpBottom;
    public Image ImgHpTop;

    [Header("攻击冷却")]
    private float attackTimer;
    private bool canAttack = true;

    private void Awake()
    {
        Debug.Log("怪物被创建，添加监听");
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        EventCenter.GetInstance().AddEventListener<bool>("GAME_PAUSE", OnPause);
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("Monster: 未找到 Player");
        if (canvasHp == null)
        {
            Debug.LogError("怪物 " + gameObject.name + " 缺少 CanvasHp 引用！请在 Inspector 里把 Canvas 拖进去。");
            return;
        }

        // 默认隐藏
        canvasHp.gameObject.SetActive(false);
        
        // 获取血条的Transform引用
        hpBarTransform = canvasHp != null ? canvasHp.transform : null;
    }

    private void OnEnable()
    {
        EventCenter.GetInstance().AddEventListener<float>("MONSTER_TAKE_DAMAGE", TakeDamage);
        EventCenter.GetInstance().AddEventListener<bool>("GAME_PAUSE", OnPause);
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position); // 强制放回 NavMesh
        }
    }

    private void OnDisable()
    {
        Debug.Log("怪物回收，移除监听");
        EventCenter.GetInstance().RemoveEventListener<float>("MONSTER_TAKE_DAMAGE", TakeDamage); 
        EventCenter.GetInstance().RemoveEventListener<bool>("GAME_PAUSE", OnPause);
        if (agent != null)
            agent.enabled = false;
    }

    public void Init(MonsterInfo cfg)
    {
        if (cfg == null)
        {
            Debug.LogError("Monster Init Failed");
            return;
        }

        monsterCfg = cfg;
        curHp = cfg.hp;
        agent.speed = cfg.moveSpeed;
        agent.angularSpeed = cfg.roundSpeed;
        attackTimer = cfg.atkOffTime;

        animator.runtimeAnimatorController =Resources.Load<RuntimeAnimatorController>(cfg.animator);

        isInitialized = true;
        if (GamingStateMgr.IsFrozen) return;
        StartCoroutine(DelayedReady());
    }
    private IEnumerator DelayedReady()
    {
        yield return null; // 等一帧
        isBorn = false;   // 防止 Update 提前跑
    }
    private void Update()
    {
        if (GamingStateMgr.IsFrozen) return;

        if (!isInitialized || player == null || isDead || !isBorn)
            return;
        if (agent == null || !agent.isOnNavMesh)
            return;
        if (GamingStateMgr.IsFrozen)
            return;

        float distance = Vector3.Distance(transform.position, player.position);
        // 任何状态下都朝向玩家
        FaceToPlayer();

        // 超出攻击范围-追击
        if (distance > monsterCfg.atkRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetBool("Run", true);
        }
        else
        {
            agent.isStopped = true;
            animator.SetBool("Run", false);

            if (canAttack)
            {
                animator.SetTrigger("Atk");
                canAttack = false;
                attackTimer = monsterCfg.atkOffTime;
            }
        }

        if (!canAttack)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
                canAttack = true;
        }
    }
    private void LateUpdate()
    {
        // 血条始终朝向摄像机（只狼风格）
        if (hpBarTransform != null && Camera.main != null)
        {
            // 1. 计算朝向摄像机的方向（忽略Y轴）
            Vector3 targetDir = Camera.main.transform.position - hpBarTransform.position;
            targetDir.y = 0; // 保持水平，避免上下倾斜
            
            if (targetDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(targetDir);
                hpBarTransform.rotation = targetRot;
            }
            
            // 2. 确保血条位置在怪物头顶
            Vector3 headPos = transform.position;
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                // 使用渲染器的边界框获取头顶位置
                headPos.y = renderer.bounds.max.y + 0.5f; // +0.5f是血条与头顶的间距
            }
            else
            {
                // 没有渲染器时，使用固定偏移
                headPos.y += 2f; // 默认高度偏移
            }
            hpBarTransform.position = headPos;
        }
    }
    private void FaceToPlayer()//转向 始终面朝玩家
    {
        if (player == null||GamingStateMgr.IsFrozen) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0; // 防止上下倾斜

        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * monsterCfg.roundSpeed
        );
    }
    // 攻击动画事件调用
    public void AtkEvent()
    {
        if (monsterCfg == null||GamingStateMgr.IsFrozen) return;

        FaceToPlayer(); // 攻击前强制对准玩家
        switch (monsterCfg.type)
        {
            case 1:
                KnifeEvent();
                break;
            case 2:
                ShootEvent();
                break;
            default:
                break;
        }
    }
    public void KnifeEvent()//近战伤害判定
    {
        if (monsterCfg == null || monsterCfg.type != 1)
            return;
        GameDataMgr.Instance.PlaySound("Music/Eat");
        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward + transform.up,
            monsterCfg.atkRange-1f,
            1 << LayerMask.NameToLayer("Player")
        );

        foreach (var hit in hits)
        {
            EventCenter.GetInstance().EventTrigger<float>("PLAYER_HURT", monsterCfg.atk);
        }
    }
    public void ShootEvent()//远程伤害判定
    {
        if (monsterCfg == null || monsterCfg.type != 2)
            return;

        Vector3 shootDir = transform.forward;

        OnPlayShootSound(monsterCfg.bulletId);
        EventCenter.GetInstance().EventTrigger<SpawnBulletParam>("SPAWN_MONSTER_BULLET",
            new SpawnBulletParam
            {
                bulletId = monsterCfg.bulletId,
                count = monsterCfg.shootCount,
                shootPoint = shootPoint != null ? shootPoint : transform,
                shootDir = shootDir,
                owner = BulletOwner.Monster,
                atk = monsterCfg.atk
            }
        );
    }
    private void OnPlayShootSound(int id)
    {
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
    public void Dead()
    {
        //死亡动画播放完毕后 移除对象 PoolMgr对象池回收+活跃列表移除
        isDead = true;
        agent.isStopped = true;
        animator.SetBool("Dead", true);
        GameDataMgr.Instance.PlaySound("Music/dead");
        // 切换 Layer
        gameObject.layer = LayerMask.NameToLayer("EnemyDead");

        // 立刻隐藏血条
        if (canvasHp != null)
            canvasHp.gameObject.SetActive(false);

        // 停掉血条计时器，防止协程冲突
        if (hpBarCoroutine != null)
        {
            StopCoroutine(hpBarCoroutine);
            hpBarCoroutine = null;
        }

        EventCenter.GetInstance().EventTrigger<int>("MONSTER_DIE", monsterCfg.id);

    }
    public void DeadEvent()
    {
        PoolMgr.GetInstance().PushObj(monsterCfg.res, gameObject);
    }
    public void BornOver()
    {
        isDead = false;
        isBorn = true;
        if (agent == null || !agent.isOnNavMesh)
            return;
        agent.stoppingDistance = monsterCfg.atkRange;
        agent.speed = monsterCfg.moveSpeed;
        agent.isStopped = false;
        //播放移动动画
        if (player != null)
            agent.SetDestination(player.position);
    }
    public void TakeDamage(float damage)
    {
        if (isDead||GamingStateMgr.IsFrozen) return;
        curHp -= damage;
        float showHp=(float)curHp/monsterCfg.hp;
        showHp=Mathf.Clamp01(showHp);
        ImgHpTop.fillAmount = showHp*1f;
        animator.SetTrigger("Wound");

        if (canvasHp != null)
            canvasHp.gameObject.SetActive(true);
        ResetHpBarTimer();

        if (curHp <= 0)
            Dead();
    }
    public void ResetMonster()
    {
        // 重置Tag
        gameObject.tag = "Enemy";
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        // 重置血量
        curHp = monsterCfg.hp;

        // 重置血条显示
        if (canvasHp != null)
            canvasHp.gameObject.SetActive(false);

        // 重置血条数值
        if (ImgHpTop != null)
            ImgHpTop.fillAmount = 1f;

        if (ImgHpBottom != null)
            ImgHpBottom.fillAmount = 1f;

        // 停止血条隐藏计时器
        if (hpBarCoroutine != null)
        {
            StopCoroutine(hpBarCoroutine);
            hpBarCoroutine = null;
        }

        // 重置状态机
        isDead = false;
        isBorn = false;
        canAttack = true;
        attackTimer = monsterCfg.atkOffTime;

        // 重置动画
        animator.Rebind();
        animator.Update(0f);

        // 重置 NavMeshAgent
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
            agent.isStopped = true;
        }

        // 同步当前游戏暂停状态
        if (GamingStateMgr.IsFrozen)
        {
            OnPause(true);
        }
    }
    private void ResetHpBarTimer()
    {
        if (GamingStateMgr.IsFrozen)
            return;
        if (hpBarCoroutine != null)
            StopCoroutine(hpBarCoroutine);

        if (GamingStateMgr.IsFrozen) return;
        hpBarCoroutine = StartCoroutine(HideHpBarAfterDelay());
    }
    private IEnumerator HideHpBarAfterDelay()
    {
        yield return new WaitForSeconds(hpBarHideDelay);

        if (canvasHp != null)
            canvasHp.gameObject.SetActive(false);
    }
    private void OnPause(bool pause)
    {
        if (animator != null)
            animator.speed = pause ? 0 : 1;

        if (agent == null || !agent.isOnNavMesh)
            return;

        if (pause)
        {
            // 冻结
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            // 防止被其他 Agent / 玩家挤动
            agent.avoidancePriority = 0;
        }
        else
        {
            // 恢复
            agent.updatePosition = true;
            agent.updateRotation = true;
            agent.isStopped = false;
            agent.avoidancePriority = 50;
        }
    }

    // 碰撞处理，防止怪物把玩家挤上天
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && rigidbody != null)
        {
            // 限制Y轴速度，防止向上推
            Vector3 velocity = rigidbody.velocity;
            velocity.y = Mathf.Min(velocity.y, 0); // 只允许向下，不允许向上
            rigidbody.velocity = velocity;
        }
    }
}