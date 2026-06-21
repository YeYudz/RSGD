using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BulletInfo
{
    public int id;              // 子弹ID
    public string res;          // Resources 路径
    public float moveSpeed;         // 飞行速度
    public float lifeTime;      // 存活时间（秒）
    public int count;          //一次生成的数量
}