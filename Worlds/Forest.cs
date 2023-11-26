using BTDAdventure.Cards.Enemies;
using BTDAdventure.Components;
using System.Collections.Generic;
using UnityEngine;

namespace BTDAdventure.Worlds;

internal class Forest : World
{
    /// <inheritdoc/>
    public override string DisplayName => "Forest";

    /// <inheritdoc/>
    public override Vector2Int GetWorldSize() => new(15, 5);

    /// <inheritdoc/>
    public override uint GetTrailCount() => 6;

    // 70% Normal
    // 30% Elite

    private readonly List<string> NodeTypes = new();

    public override void ResetCounters()
    {
        NodeTypes.Clear();

        for (int i = 0; i < 53; ++i) // 70%
            NodeTypes.Add(MapGenerator.NODE_TYPE_NORMAL);

        for (int i = 0; i < 22; ++i) // 30%
            NodeTypes.Add(MapGenerator.NODE_TYPE_ELITE);
    }

    public override string GetNodeType(int x, int y)
    {
        int rdmIndex = Random.Range(0, NodeTypes.Count);
        string node = NodeTypes[rdmIndex];
        NodeTypes[rdmIndex] = NodeTypes[^1];
        NodeTypes.RemoveAt(NodeTypes.Count - 1);
        return node;
    }
}

internal class Forest_RedGroup1 : NormalEnemyGroup<Forest>
{
    public override EnemyCard[] Enemies => new EnemyCard[] { new Red1(), new Red1() };
}

internal class Forest_RedGroup2 : NormalEnemyGroup<Forest>
{
    public override EnemyCard[] Enemies => new EnemyCard[] { new Red2(), new Red3(), new Red2() };
}

internal class Forest_BlueGroup1 : NormalEnemyGroup<Forest>
{
    public override EnemyCard[] Enemies => new EnemyCard[] { new Blue1(), new Blue2() };
}