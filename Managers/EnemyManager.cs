using BTD_Mod_Helper.Api;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace BTDAdventure.Managers;

///
/// TODO: Change the enemy group api
/// Attribute a risk value to each enemy
/// Each fight has its risk value
/// Try to separate the risk value between the enemies of the world
/// If risk value is remaining, spawn a world portal tha spawns a previous world enemy
/// with higher stats (lowest enemy of world / enemy %)
/// 
///

internal class EnemyManager
{
    Dictionary<string, List<(uint, Type)>> EnemiesTypes;

    public EnemyManager()
    {
        var enemies = ModContent.GetContent<EnemyCard>();

        EnemiesTypes = new();


        foreach (var enemy in enemies)
        {
            if (enemy == null || string.IsNullOrEmpty(enemy.World))
                continue;

            if (!EnemiesTypes.ContainsKey(enemy.World))
            {
                EnemiesTypes.Add(enemy.World,
                new()
                    {
                        (enemy.RiskValue, enemy.GetType())
                });
            }
            else
                EnemiesTypes[enemy.World].Add((enemy.RiskValue, enemy.GetType()));
        }

        foreach (var (_,value) in EnemiesTypes)
        {
            value.Sort((x, y) =>
            {
                return x.Item1.CompareTo(y.Item1);
            });
        }
    }

    internal Type?[] GenerateEnemies(string type, string world)
    {
        var enemies = new Type?[MaxEnemiesCount];

        uint risk = (uint)Random.Range(3, 3);
        uint min = 0;

        // R5
        // 1 Green
        // 1 Red

        // R6
        // 1 Green
        // 1 Blue
        
        // R7
        // 1 Green
        // 1 Blue
        // 1 Red

        // R8
        // 2 Green
        
        // R9
        // 2 Green
        // 1 Red

        // R10
        // 1 Yellow

        // R11
        // 1 Yellow
        // 1 Red

        // R12
        // 1 Yellow
        // 1 Blue

        // R13
        // 1 Yellow
        // 1 Blue
        // 1 Red

        // R14
        // 1 Yellow
        // 1 Green
        
        // R15
        // 1 Yellow
        // 1 Green
        // 1 Red

        // R16
        // 1 Yellow
        // 1 Green
        // 1 Blue

        // R24-29
        // 2 Yellow
        // 1 Green


        // Randomizing the enemy groups removes possible synergies
        // Randomizing the enemy groups allows more replayability

        // Normal < Elite < Boss
        // Normal battles must be easier than Elite battles
        // The risk of normal battles are lower than Elite battles
        // Randomize the risk boundaries of each tier
        // Normal should be [2 * lowest; lowest elite - 1]
        // Elite should be [2 * lowest; lowest boss - 1]

        // Increase the general difficulty after each fight ?

        var e = EnemiesTypes[world];

        // If risk is <= min
        // Search glitches foes

        // If risk is <= 
        // Find all enemies of risk X
        // Take a random one

        for (int i = 0; i < enemies.Length; i++)
        {
            uint enemyRisk;
            Type? enemyType;

            (enemyRisk, enemyType) = FindEnemyWithRisk(e, risk);

            if (enemyType == null)
                break;

            risk -= enemyRisk;

            enemies[i] = enemyType;

            if (risk <= 0)
                break;
        }

        return enemies;
    }

    private static (uint, Type?) FindEnemyWithRisk(List<(uint, Type)> enemiesAvailable, uint risk)
    {
        List<(uint, Type)> enemies = new();
        uint? target = null;

        for (int i = enemiesAvailable.Count - 1; i >= 0; i--)
        {
            if (target.HasValue)
            {
                // If enemy has a risk == target
                if (enemiesAvailable[i].Item1 == target.Value)
                {
                    // Add the enemy to the list
                    enemies.Add(enemiesAvailable[i]);
                }
                // If the enemy has a lower risk than the target, all enemies with risk == target were found
                else if (enemiesAvailable[i].Item1 < target.Value)
                    break;
            }
            else
            {
                // If risk is <= min
                // Search glitches foes

                // If the enemy has a risk <= than the remaining risk
                if (enemiesAvailable[i].Item1 <= risk)
                {
                    // Set the risk target
                    target = enemiesAvailable[i].Item1;

                    // Add the enemy to the list
                    enemies.Add(enemiesAvailable[i]);
                }
            }
        }
        return enemies.Count == 0 ? (default, null) : enemies[Random.Range(0, enemies.Count)];
    }
}