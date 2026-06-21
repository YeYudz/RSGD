using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private BulletInfo cfg;
    private float damage;  
    private Vector3 direction;
    public BulletOwner owner;
    private void Awake()
    {
    }
    
    public void Init(BulletInfo info, float damage, Vector3 dir, BulletOwner owner)
    {
        this.owner = owner;
        cfg = info;
        this.damage = damage;
        direction = dir.normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        StartCoroutine(AutoRecycle());
    }
    private void OnDisable()
    {
        // 移除暂停检查，总是停止协程
        StopAllCoroutines();
    }
    private void Update()
    {
        if (!GamingLifecycleMgr.IsRunning)
        {
            Recycle();
            return;
        }
        Move();
    }

    private void Move()
    {
        if (GamingStateMgr.IsFrozen) return;
        if (GamingLifecycleMgr.IsPaused) return;

        // 添加空引用检查
        if (cfg == null)
        {
            Recycle();
            return;
        }

        transform.Translate(direction * cfg.moveSpeed * Time.deltaTime, Space.World);
    }

    private IEnumerator AutoRecycle()
    {
        // 添加空引用检查
        if (cfg == null)
        {
            Recycle();
            yield break;
        }

        if (!GamingLifecycleMgr.IsRunning)
        {
            Recycle();
            yield break;
        }
        float timer = cfg.lifeTime;
        while (timer > 0)
        {
            if (!GamingLifecycleMgr.IsRunning)
            {
                Recycle();
                yield break;
            }

            if (!GamingLifecycleMgr.IsPaused)
                timer -= Time.deltaTime;

            yield return null;
        }
        Recycle();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!GamingLifecycleMgr.IsRunning)
        {
            Recycle();
            return;
        }

        // 添加空引用检查
        if (cfg == null)
        {
            Recycle();
            return;
        }

        // 玩家子弹
        if (owner == BulletOwner.Player)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Enemy"))
                return;
            if (other.TryGetComponent<Monster>(out var monster))
            {
                monster.TakeDamage(damage);
                Recycle();
            }
        }
        // 怪物子弹
        else if (owner == BulletOwner.Monster)
        {
            if (other.TryGetComponent<PlayerObject>(out var player))
            {
                EventCenter.GetInstance().EventTrigger<float>("PLAYER_HURT", damage);
                Recycle();
            }
        }

    }

    private void Recycle()
    {
        StopAllCoroutines();
        // 添加空引用检查
        if (cfg != null)
            PoolMgr.GetInstance().PushObj(cfg.res, gameObject);
    }
    public void ClearBulletsBeforeSceneChange()
    {
        // 获取所有子弹（可以用标签或管理器）
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (var bullet in bullets)
        {
            if (bullet.TryGetComponent<Bullet>(out var bulletComp))
            {
                bulletComp.Recycle(); // 这会停止协程并回池
            }
        }
    }
}
