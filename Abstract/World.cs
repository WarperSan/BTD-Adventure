using BTD_Mod_Helper.Api;
using BTDAdventure.Cards.Enemies;
using BTDAdventure.Components;
using System.Collections.Generic;
using UnityEngine;

namespace BTDAdventure.Abstract;

internal abstract class World : ModContent
{
    /// <summary>
    /// Name of the world that will be displayed
    /// </summary>
    public abstract string DisplayName { get; }

    #region Enemies Generation
    internal EnemyCard[] GetEnemies(string type) => type switch
    {
        MapGenerator.NormalNode => SpawnNormal(),
        MapGenerator.EliteNode => SpawnElite(),
        MapGenerator.BossNode => SpawnBoss(),
        _ => SpawnNormal(),
    };

    protected virtual EnemyCard[] SpawnNormal() => new EnemyCard[] { new Red1(), new Red2(), new Red1() };
    protected virtual EnemyCard[] SpawnElite() => new EnemyCard[] { new Yellow1(), new Yellow2(), new Yellow1() };
    protected virtual EnemyCard[] SpawnBoss() => new EnemyCard[] { new Red1() };
    #endregion

    #region Map generation
    /// <summary>
    /// Size of the world
    /// </summary>
    /// <remarks>
    /// If the size is less or equal to 0, the size will be reset to the default value
    /// </remarks>
    public abstract Vector2Int Size { get; }

    /// <summary>
    /// Number of paths that the world will create
    /// </summary>
    public abstract uint TrailsCount { get; }

    public virtual void ResetCounters() { }

    /// <returns>
    /// Gets the node type of the node at the position (<paramref name="x"/>; <paramref name="y"/>)
    /// </returns>
    public virtual string GetNodeType(int x, int y) => "normal";

    /// <returns>
    /// Gets the <see cref="Texture"/> of the node type if it was not found before.
    /// </returns>
    public virtual Texture? GetMapIcon(string type) => null;
    #endregion

    /// <inheritdoc/>
    public override void Register()
    {
#if DEBUG
        Log(DisplayName + " registered !");
#endif
    }
}

class Forest : World
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

    protected override EnemyCard[] SpawnNormal()
    {
        int rdm = Random.Range(0, 8);

        return rdm switch
        {
            0 => new EnemyCard[] { new Red1(), new Red2(), new Red1() },
            1 => new EnemyCard[] { new Red2(), new Red2() },
            2 => new EnemyCard[] { new Red3() },
            3 => new EnemyCard[] { new Blue1() },
            4 => new EnemyCard[] { new Red1(), new Blue2(), new Red1() },
            5 => new EnemyCard[] { new Blue3() },
            6 => new EnemyCard[] { new Green1() },
            7 => new EnemyCard[] { new Green2(), new Green3() },
            _ => base.SpawnNormal(),
        };
    }

    protected override EnemyCard[] SpawnElite()
    {
        return base.SpawnNormal();
    }
}