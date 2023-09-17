using BTD_Mod_Helper.Api;
using BTDAdventure.Cards.Enemies;
using BTDAdventure.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BTDAdventure.Abstract;

public abstract class World : ModContent
{
    /// <summary>
    /// Name of the world that will be displayed
    /// </summary>
    public abstract string DisplayName { get; }

    readonly Dictionary<string, List<EnemyGroup>> EnemyGroupsForWorld = new();

    internal void FindEnemies()
    {
        var groups = GetContent<EnemyGroup>().Where(x => x.IsWorld(this));

        foreach (var group in groups)
        {
            if (group == null)
                continue;

            if (EnemyGroupsForWorld.ContainsKey(group.Type))
            {
                EnemyGroupsForWorld[group.Type].Add(group);
            }
            else
                EnemyGroupsForWorld.Add(group.Type, new List<EnemyGroup>() { group });
        }
    }

    protected override int Order => -1;

    #region Enemies Generation
    internal EnemyCard[] GetEnemies(string type)
    {
        if (EnemyGroupsForWorld.TryGetValue(type, out var enemyGroups))
            return enemyGroups[Random.Range(0, enemyGroups.Count)].Enemies;

        return type switch
        {
            MapGenerator.EliteNode => new EnemyCard[] { new Yellow1(), new Yellow2(), new Yellow1() },
            MapGenerator.BossNode => new EnemyCard[] { new Red1() },
            _ => new EnemyCard[] { new Red1(), new Red2(), new Red1() },
        };
    }
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
    public virtual string GetNodeType(int x, int y) => MapGenerator.NormalNode;

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