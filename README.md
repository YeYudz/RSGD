# RSGD - Roguelike Shooting Game Demo

一款基于 Unity 开发的 Roguelike 射击游戏 Demo，展示了完整的游戏架构设计和核心玩法。

##  游戏特性

### 核心玩法
- **Roguelike 节点系统**: 战斗节点、商店节点、休息节点、Boss 节点
- **角色系统**: 4 个独特角色，各具特色技能
- **技能系统**: E/Q 双技能，支持乘法叠加机制
- **波次战斗**: 分波次生成怪物，战斗节奏可控
- **升级奖励**: 击杀怪物获得经验，升级解锁奖励

### 技术亮点
- **事件驱动架构**: 基于 EventCenter 的观察者模式，模块完全解耦
- **对象池系统**: 子弹、怪物高频对象缓存复用，优化性能
- **数据驱动设计**: 角色/怪物/技能/关卡全 JSON 配置
- **技能乘法叠加**: E/Q 技能倍率相乘，避免数值膨胀
- **Billboard 血条**: 怪物血条始终朝向摄像机（只狼风格）

##  架构设计

```
ProjectBase/          # 基础框架模块
├── Event/            # 事件中心
├── Pool/             # 对象池
├── UI/               # UI管理
├── Res/              # 资源管理
├── Mono/             # 协程管理
└── Base/             # 单例基类

CoreManager/          # 游戏核心逻辑
├── GamingStateMgr    # 游戏状态管理
├── GamingLifecycleMgr # 生命周期管理
├── SkillMgr          # 技能系统
├── LevelNodeMgr      # 节点系统
├── NodeWaveSpawner   # 波次生成
└── StoreMgr          # 商店系统

Object/               # 游戏对象
├── Player/           # 玩家
├── Monster/          # 怪物
└── Bullet/           # 子弹

UI/                   # 界面面板
├── GamingPanel       # 游戏主界面
├── CharacterSelectPanel # 角色选择
├── ShopPanel         # 商店
├── RewardPanel       # 奖励
└── GameOverPanel     # 游戏结束
```

##  项目结构

| 目录 | 说明 |
|------|------|
| `Assets/Scripts/ProjectBase/` | 基础框架模块（可复用） |
| `Assets/Scripts/CoreManager/` | 游戏核心逻辑 |
| `Assets/Scripts/Object/` | 游戏对象脚本 |
| `Assets/Scripts/UI/` | UI 面板 |
| `Assets/Scripts/Data/` | 数据模型定义 |
| `Assets/StreamingAssets/` | JSON 配置文件 |

##  角色介绍

| 角色 | 定位 | E技能 | Q技能 |
|------|------|-------|-------|
| **角色1** | 近战/基础 | 强力攻击（+100%攻击力） | 狂暴（+200%攻击力） |
| **角色2** | 近战/爆发 | 锋利（+150%攻击力） | 牺牲（扣血+武器变大+200%攻击力） |
| **角色3** | 远程/火力 | 火力全开（+150%攻击力） | 狂热（无限弹药） |
| **角色4** | 远程/重火力 | 重型打击（+150%攻击力） | 无双（额外多发子弹） |

##  技术栈

- **引擎**: Unity 2022.x
- **语言**: C#
- **UI**: Unity UGUI
- **寻路**: NavMesh
- **序列化**: LitJson
- **事件系统**: 自定义 EventCenter（观察者模式）

##  快速开始

### 环境要求
- Unity 2022.3.x 或更高版本
- .NET Framework 4.x

### 运行项目
1. 克隆仓库：`git clone https://github.com/YeYudz/RSGD.git`
2. 使用 Unity 打开项目
3. 运行 `Init` 场景
4. 点击开始游戏，选择角色进入战斗

### 操作说明
- **WASD**: 移动
- **鼠标左键**: 射击
- **E**: 使用 E 技能
- **Q**: 使用 Q 技能
- **R**: 换弹

##  核心系统详解

### 事件中心 (EventCenter)
基于泛型和委托实现的事件系统，支持任意类型参数传递：
```csharp
// 监听事件
EventCenter.GetInstance().AddEventListener<int>("MONSTER_DIE", OnMonsterDie);

// 触发事件
EventCenter.GetInstance().EventTrigger<int>("MONSTER_DIE", monsterId);
```

### 对象池 (PoolMgr)
采用 Dictionary+List 结构，实现对象的缓存与复用：
```csharp
// 获取对象
PoolMgr.GetInstance().GetObj("Bullet/Obj/1", (obj) => { /* 使用对象 */ });

// 回收对象
PoolMgr.GetInstance().PushObj("Bullet/Obj/1", obj);
```

### 技能系统 (SkillMgr)
支持多种技能类型，攻击力加成采用乘法叠加：
```csharp
float totalMultiplier = skillEAtkMultiplier * skillQAtkMultiplier;
float totalBonus = totalMultiplier - 1f;
```

##  开发日志

### v1.0.0
- 完成基础架构搭建
- 实现 4 个角色技能系统
- 实现 Roguelike 节点系统
- 实现波次怪物生成
- 实现对象池优化

##  许可证

MIT License

##  贡献

欢迎提交 Issue 和 Pull Request！

*Made with for Game Development*
