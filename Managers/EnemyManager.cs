using BTD_Mod_Helper;
using BTD_Mod_Helper.Extensions;
using BTDAdventure.Cards.EnemyCards;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace BTDAdventure.Managers;

internal class EnemyManager
{
    const string EnemyGroupsFileName = "enemy_groups.xml";

    private Type[]? EnemiesTypes;
    public bool IsEnemyTypeValid(Type t, bool sendErrorMessage = true) => IsGivenTypeInArray<EnemyCard>(EnemiesTypes, t, sendErrorMessage);

    internal Type? GetType(string? name)
    {
        if (name != null && EnemiesTypes != null)
        {
            foreach (var item in EnemiesTypes)
            {
                if (item.Name == name)
                    return item;
            }
        }
        return null;
    }

    public EnemyManager()
    {
        EnemiesTypes = InitializeAirport<EnemyCard>();
        GetEnemyGroups();
    }

    internal Type?[] GenerateEnemies(string type, string world)
    {
        Type?[] enemies = new Type?[MaxEnemiesCount];
        EnemyGroup[] validGroups = EnemyGroups.FindAll(x => x.Type == type && x.World == world);

        if (validGroups.Length > 0)
        {
            int index = UnityEngine.Random.Range(0, validGroups.Length);

#if DEBUG
            Log($"Groups #{index} chosen.");
#endif

            Type[] groupEnemies = validGroups[index].Enemies;

            int min = Math.Min(enemies.Length, groupEnemies.Length);

            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i] = groupEnemies.Length > i ? groupEnemies[i] : null;
            }
        }
        else
        {
            Log($"No group found of type \'{type}\' in the world \'{world}\'.");

            // Set bugged group
        }

        return enemies;
    }

    #region Enemy Group
    private EnemyGroup[]? EnemyGroups;

    internal void GetEnemyGroups()
    {
        List<EnemyGroup> groups = new();

        string path = Path.Combine(ModHelper.ModHelperDirectory, EnemyGroupsFileName);

        // If the file does not exist
        if (!File.Exists(path))
            CopyEnemyGroupXML(path);

        try
        {
            // Get file content
            XmlDocument doc = new();

            try { doc.Load(path); }
            catch (Exception e)
            {
                Log(e.Message);

                // If error, try to copy the internal
                CopyEnemyGroupXML(path);

                doc.Load(path); // If crash again, total error
            }

            XmlNodeList? nodes = doc.DocumentElement?.SelectNodes("/groups/group");

            // Read each group
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    string? type = node.Attributes?["type"]?.InnerText;
                    // Check if type is valid
                    if (!IsGroupTypeValid(type))
                        continue;

                    string? world = node.Attributes?["world"]?.InnerText;
                    // Check if world is valid
                    if (!IsWorldValid(world))
                        continue;

                    List<Type> enemies = new();
                    string[] enemiesTXT = node.InnerText.Split("|");

                    foreach (string item in enemiesTXT)
                    {
                        if (!IsEnemyTypeValid(item))
                            continue;

                        Type? t = GetType(item);
                        if (t != null)
                            enemies.Add(t);
                    }
#if DEBUG
                    Log($"Type: {type}, World: {world}");
#endif
                    groups.Add(new(type ?? "", enemies.ToArray(), world ?? ""));
                }
            }

            EnemyGroups = groups.ToArray();
        }
        catch (Exception e)
        {
            Log(e.Message);
            throw;
        }
    }
    private void CopyEnemyGroupXML(string path)
    {
        Log($"\'{EnemyGroupsFileName}\' not found at \'{path}\'. Resetting the file ...");

        Assembly assembly = Assembly.GetExecutingAssembly();

        // If not, copy intern file
        using var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.{EnemyGroupsFileName}");
        using var file = new FileStream(path, FileMode.Create, FileAccess.Write);
        resource?.CopyTo(file);
    }

    private bool IsGroupTypeValid(string? value, bool showMessage = true)
    {
        bool result = value != null;

        if (!result && showMessage)
            Log($"\'{value ?? "null"}\' is an invalid group type.");
        return result;
    }
    private bool IsWorldValid(string? value, bool showMessage = true)
    {
        bool result = value != null;

        if (!result && showMessage)
            Log($"\'{value ?? "null"}\' is an invalid world.");
        return result;
    }
    private bool IsEnemyTypeValid(string? value, bool showMessage = true)
    {
        bool result = value != null;

        if (result)
            result = EnemiesTypes.Any(x => x.Name == value);

        if (!result && showMessage)
            Log($"\'{value ?? "null"}\' is an invalid enemy.");
        return result;
    }

    struct EnemyGroup
    {
        public string Type;
        public Type[] Enemies;
        public string World;

        public EnemyGroup(string Type, Type[] Enemies, string World)
        {
            this.Type = Type;
            this.Enemies = Enemies;
            this.World = World;
        }
    }
    #endregion
}