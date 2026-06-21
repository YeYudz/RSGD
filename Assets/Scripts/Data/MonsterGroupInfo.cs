using System.Collections.Generic;
using UnityEngine;

public class MonsterGroupInfo
{
    public int groupId;
    public List<MonsterSpawnInfo> spawnList;
}

public class MonsterSpawnInfo
{
    public int monsterId;
    public int count;
}
