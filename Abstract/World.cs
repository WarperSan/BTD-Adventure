﻿using BTD_Mod_Helper.Api;
using BTDAdventure.Components;
using BTDAdventure.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace BTDAdventure.Abstract;

public abstract class World : ModContent
{
    /// <summary>
    /// Name of the world that will be displayed.
    /// </summary>
    public abstract string DisplayName { get; }

    #region Enemies Generation

    private readonly Dictionary<string, List<EnemyGroup>> EnemyGroups = new();

    internal void AddEnemyGroup(EnemyGroup enemyGroup)
    {
        var groups = EnemyGroups.TryGetValue(enemyGroup.Type, out var list) ? list : new List<EnemyGroup>();
        groups.Add(enemyGroup);

        EnemyGroups[enemyGroup.Type] = groups;
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

    /// <returns>Enemies to spawn on this encounter of a custom type.</returns>
    protected virtual EnemyCard[] SpawnCustom(string type) => DefaultSpawn(type);

    private EnemyCard[] DefaultSpawn(string type) => 
        EnemyGroups.TryGetValue(type, out var groups) ?
        groups[Random.Range(0, groups.Count)].Enemies : 
        System.Array.Empty<EnemyCard>();

    #endregion Enemies Generation

    #region Map generation

    /// <summary>
    /// Size of the world, where <see cref="Vector2Int.x"/> is the number of columns
    /// and <see cref="Vector2Int.y"/> is the number of lines.
    /// </summary>
    /// <remarks>
    /// If the size of any direction is less or equal to 0, 
    /// this particular size will be reset to their default value.
    /// </remarks>
    public abstract Vector2Int GetWorldSize();

    /// <summary>
    /// Number of paths that the world will create.
    /// </summary>
    /// <remarks>
    /// If the value returned is less or equal to 0, the trail count 
    /// will be reset to its default value.
    /// </remarks>
    public abstract uint GetTrailCount();

    public virtual void ResetCounters()
    { }

    /// <returns>
    /// Gets the node type of the node at the given position (<paramref name="x"/>; <paramref name="y"/>).
    /// </returns>
    public virtual string GetNodeType(int x, int y) => MapGenerator.NODE_TYPE_NORMAL;

    /// <returns>
    /// Gets the <see cref="Texture"/> of the given node type.
    /// </returns>
    /// <remarks>
    /// If you don't want to override the default icons but still want to
    /// set your custom icon, return null for the other types.
    /// </remarks>
    public virtual Texture? GetMapIcon(string type) => null;

    #endregion Map generation

    #region Rewards

    /// <returns>Is <paramref name="card"/> allowed to be picked for the rewards in this world ?</returns>
    public virtual bool RewardCardAllowed(HeroCard card, string encounterType) => true;

    /// <summary>
    /// Allows to change the rewards obtained by enemies depending on the world.
    /// </summary>
    /// <returns>Altered reward</returns>
    public virtual Reward AlterEnemyReward(Reward reward) => reward;

    #endregion

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