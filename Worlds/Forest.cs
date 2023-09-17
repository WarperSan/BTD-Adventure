using BTDAdventure.Cards.Enemies;
using BTDAdventure.Components;
using System.Collections.Generic;
using UnityEngine;

namespace BTDAdventure.Worlds;

internal class Forest : World
{
    public override string DisplayName => "Forest";
    public override Vector2Int Size => new(5, 15);
    public override uint TrailsCount => 6;

    // 70% Normal
    // 30% Elite

    private readonly List<string> NodeTypes = new();
    public override void ResetCounters()
    {
        NodeTypes.Clear();

        for (int i = 0; i < 53; ++i) // 70%
        {
            NodeTypes.Add(MapGenerator.NormalNode);
        }

        for (int i = 0; i < 22; i++) // 30%
        {
            NodeTypes.Add(MapGenerator.EliteNode);
        }
    }

    public override string GetNodeType(int x, int y)
    {
        int rdmIndex = Random.Range(0, NodeTypes.Count);
        string node = NodeTypes[rdmIndex];
        NodeTypes[rdmIndex] = NodeTypes[^1];
        NodeTypes.RemoveAt(NodeTypes.Count - 1);
        return node;
    }

    //protected override EnemyCard[] SpawnElite()
    //{
    //    int rdm = Random.Range(0, 2);
    //    return rdm switch
    //    {
    //        1 => new EnemyCard[] { new Green2(), new Green3() },
    //        _ => base.SpawnElite(),
    //    };
    //}
}

class Forest_RedGroup1 : NormalEnemyGroup<Forest>
{
    public override EnemyCard[] Enemies => new EnemyCard[] { new Red1(), new Red2(), new Red1() };
}

class Forest_RedGroup2 : NormalEnemyGroup<Forest>
{
    public override EnemyCard[] Enemies => new EnemyCard[] { new Red2(), new Red2() };
}

class Forest_RedGroup3 : NormalEnemyGroup<Forest>
{
    public override EnemyCard[] Enemies => new EnemyCard[] { new Red3() };
}

class Forest_BlueGroup1 : NormalEnemyGroup<Forest>
{
    public override EnemyCard[] Enemies => new EnemyCard[] { new Blue1() };
}

class Forest_BlueGroup2 : NormalEnemyGroup<Forest>
{
    public override EnemyCard[] Enemies => new EnemyCard[] { new Red1(), new Blue2(), new Red1() };
}

class Forest_BlueGroup3 : NormalEnemyGroup<Forest>
{
    public override EnemyCard[] Enemies => new EnemyCard[] { new Blue3() };
}

class Forest_GreenGroup1 : NormalEnemyGroup<Forest>
{
    public override EnemyCard[] Enemies => new EnemyCard[] { new Green1() };
}