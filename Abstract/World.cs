using BTD_Mod_Helper.Api;
using BTDAdventure.Components;
using System.Collections.Generic;
using UnityEngine;

namespace BTDAdventure.Abstract;

public abstract class World : ModContent
{
    /// <summary>
    /// Name of the world that will be displayed
    /// </summary>
    public abstract string DisplayName { get; }

    #region Enemies Generation

    private readonly Dictionary<string, List<EnemyGroup>> EnemyGroups = new();

    internal void AddEnemyGroup(EnemyGroup enemyGroup)
    {
        string enemyGroupType = enemyGroup.Type;

        if (EnemyGroups.TryGetValue(enemyGroupType, out var list))
        {
            list.Add(enemyGroup);
        }
        else
            EnemyGroups.Add(enemyGroupType, new List<EnemyGroup>() { enemyGroup });
    }

    internal EnemyCard[] GetEnemies(string type) => type switch
    {
        MapGenerator.NODE_TYPE_NORMAL => SpawnNormal(),
        MapGenerator.NODE_TYPE_ELITE => SpawnElite(),
        MapGenerator.NODE_TYPE_BOSS => SpawnBoss(),
        _ => SpawnCustom(type),
    };

    /// <returns>Enemies to spawn on this encounter of type '<see cref="MapGenerator.NODE_TYPE_NORMAL"/>'.</returns>
    protected virtual EnemyCard[] SpawnNormal() => DefaultSpawn(MapGenerator.NODE_TYPE_NORMAL);

    /// <returns>Enemies to spawn on this encounter of type '<see cref="MapGenerator.NODE_TYPE_ELITE"/>'.</returns>
    protected virtual EnemyCard[] SpawnElite() => DefaultSpawn(MapGenerator.NODE_TYPE_ELITE);

    /// <returns>Enemies to spawn on this encounter of type '<see cref="MapGenerator.NODE_TYPE_BOSS"/>'.</returns>
    protected virtual EnemyCard[] SpawnBoss() => DefaultSpawn(MapGenerator.NODE_TYPE_BOSS);

    private EnemyCard[] DefaultSpawn(string type)
    {
        if (EnemyGroups.TryGetValue(type, out var groups))
            return groups[Random.Range(0, groups.Count)].Enemies;
        return System.Array.Empty<EnemyCard>();
    }

    /// <returns>Enemies to spawn on this encounter of a custom type.</returns>
    protected virtual EnemyCard[] SpawnCustom(string type) => System.Array.Empty<EnemyCard>();

    #endregion Enemies Generation

    #region Map generation

    /// <summary>
    /// Size of the world, where <see cref="Vector2Int.x"/> is the number of columns
    /// and <see cref="Vector2Int.y"/> is the number of lines.
    /// </summary>
    /// <remarks>
    /// If the size is less or equal to 0, the size will be reset to the default value
    /// </remarks>
    public abstract Vector2Int GetWorldSize();

    /// <summary>
    /// Number of paths that the world will create
    /// </summary>
    public abstract uint GetTrailCount();

    public virtual void ResetCounters()
    { }

    /// <returns>
    /// Gets the node type of the node at the position (<paramref name="x"/>; <paramref name="y"/>)
    /// </returns>
    public virtual string GetNodeType(int x, int y) => MapGenerator.NODE_TYPE_NORMAL;

    /// <returns>
    /// Gets the <see cref="Texture"/> of the given node type.
    /// </returns>
    public virtual Texture? GetMapIcon(string type) => null;

    #endregion Map generation

    /// <inheritdoc/>
    protected sealed override int Order => 1; // Load before EnemyGroup

    /// <inheritdoc/>
    public override sealed void Register()
    {
#if DEBUG
        Log(DisplayName + " registered !");
#endif
    }
}