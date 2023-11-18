using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Extensions;
using BTDAdventure.Components;
using BTDAdventure.Enemy_Actions;
using BTDAdventure.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BTDAdventure.Managers;

internal class GameManager
{
    #region Constants

    public const int MaxEnemiesCount = 3;
    public const int MaxPlayerCardCount = 4;

    #endregion Constants

    internal static GameManager Instance { get; set; } = new();

    internal void Initialize()
    {
        EffectsType = InitializeAirport<Effect>();

        MainBundle = ModContent.GetBundle<Main>("btdadventure");
        Log("Bundle Loaded");
    }

    #region Static

    internal static Type[] InitializeAirport<T>()
    {
        try
        {
            // Register enemy types. If an enemy of another type is found, it is invalid.
            Type[] types = FindDerivedTypes<T>();
#if DEBUG
            Type givenType = typeof(T);

            foreach (var item in types)
            {
                Log($"{item.Name} registered.", $"{givenType.Name} Airport");
            }
#endif

            Log($"Found {types.Length} items of type \'{typeof(T).Name}\'.");
            return types;
        }
        catch (Exception e)
        {
            Log(e);
            throw;
        }
    }

    public static Type[] FindDerivedTypes<T>()
    {
        Type baseType = typeof(T);
        Type[] types = baseType.Assembly.GetTypes();
        List<Type> wantedTypes = new();
        foreach (var item in types)
        {
            if (item != baseType && baseType.IsAssignableFrom(item) && !item.IsAbstract)
                wantedTypes.Add(item);
        }
        return wantedTypes.ToArray();
    }

    public static bool CreateInstance<T>(Type targetType, out T? result)
    {
        result = (T?)Activator.CreateInstance(targetType);

        if (result == null)
        {
            Log($"Failed to create an instance of type \'{targetType.Name}\'.");
        }
        return result != null;
    }

    internal static bool IsGivenTypeInArray<T>(Type[]? array, Type t, bool sendErrorMessage)
    {
        if (array == null)
            return false;

        bool r = Array.IndexOf(array, t) != -1;

        if (!r && sendErrorMessage)
        {
            Log($"\'{t.Name}\' is not a valid {typeof(T).Name} type");
        }
        return r;
    }

    #endregion Static

    #region Airports

    internal PlayerEntity? Player;

    internal Type[]? EffectsType;

    public bool IsTypeAnEffect(Type type) => IsGivenTypeInArray<Effect>(EffectsType, type, true);

    #endregion Airports

    #region Managers

    internal UIManager? UiManager;
    private CardManager? CardManager;

    #endregion Managers

    private bool _isFirstFight;

    // Prevents infinite battles.
    // Each X turns, enemies will gain +1 base damage
    private const byte TurnsPerDifficulty = 10;

    private uint _turnCount = 0;
    public uint TurnCount => _turnCount;
    internal int FightDifficulty = 0;

    internal void StartGame()
    {
        Reset();

        // Initialize Prefabs
        RewardCardPrefab ??= UIManager.InitializeRewardPrefab();
        EffectSlotPrefab ??= GameObject.Instantiate(LoadAsset<GameObject>("EffectSlot"));
        PlayerCardPrefab ??= GameObject.Instantiate(LoadAsset<GameObject>("Card"));
        Main.blurMat = LoadAsset<Material>("BlurMat");

        TaskScheduler.ScheduleTask(new Action(() =>
        {
            Main.blurMat = LoadAsset<Material>("BlurMat");
        }), BTD_Mod_Helper.Api.Enums.ScheduleType.WaitForFrames, 3);

        // Create UIManager
        UiManager ??= new();

        // Remove base UI & add bundle UI
        Transform? mainUI = UiManager.SetUpMainUI();

        if (mainUI == null)
        {
            Log("No instance of Main UI was defined.");
            return;
        }

        // Blurry BG
        if (Camera.allCameras.Count > 0)
        {
            foreach (Camera cam in Camera.allCameras)
            {
                if (cam.gameObject.name == "SelectedTowerOutline")
                    continue;

                if (!cam.gameObject.HasComponent<ShaderEngine_CameraBehavior>())
                {
                    cam.gameObject.AddComponent<ShaderEngine_CameraBehavior>();
                }

                var rect = cam.pixelRect;
                rect.width *= 2;
                rect.height = rect.width;
                cam.pixelRect = rect;
            }
        }

        // Create Player entity
        Player = new(mainUI.gameObject, 50, new WarriorClass());

        this.CardManager = new(Player.RogueClass);

        Player?.UpdatePlayerUI(); // Put values on the labels
        UiManager?.GameUI?.SetActive(false);
    }

    internal void StartFight(string type)
    {
        try
        {
            _turnCount = uint.MaxValue;
            FightDifficulty = 0;

            // Clear status
            // Set-up player cards UI
            // Copy owned cards to draw pile
            // Fill hand
            PopulatePlayer();

            // Build skills

            // Get enmies group
            // Build enemies
            PopulateEnemies(type, CurrentWorld);

            StartPlayerTurn();

            _isFirstFight = false;
        }
        catch (Exception e)
        {
            Log(e.Message);
            throw;
        }
    }

    private void PopulatePlayer()
    {
        Player?.UpdatePlayerUI();

        // Clear all status
        Player?.RemoveAllEffects();

        // Reset piles
        // Copy owned cards to draw pile
        this.CardManager?.ResetPiles();
        this.CardManager?.ClearCounters();

        // Set-up player cards UI if necessary
        if (_isFirstFight)
            this.UiManager?.InitPlayerCards();
    }

    private void StartPlayerTurn()
    {
        if (!Player?.IsAlive ?? false)
            return;

        // Increases general difficulty
        _turnCount++;

        if (_turnCount >= TurnsPerDifficulty)
        {
            _turnCount -= TurnsPerDifficulty;
            FightDifficulty++;
        }

        // Set all skills to their lowest value
        // If lowest value equals 0, remove the effect

        // Check skills
        //setBuffIcon();

        // Check skills
        // Enable end turn btn

        foreach (var item in _enemies)
        {
            if (item == null)
                continue;

            item.UpdateEffects();
            item.MoveIntent();
        }

        // Check fore-turn skills

        Player?.ClearShield();

        // Restore mana
        Player?.ResetMana();

        // Fill hand
        this.CardManager?.FillHand(MaxPlayerCardCount, swingAnimation: !_isFirstFight);

        // Check pre-turn skills

        if (UiManager != null && UiManager.EndTurnBtn != null)
            UiManager.EndTurnBtn.interactable = true;
    }

    public void EndTurn()
    {
        this.CardManager?.EmptyHand();
        SelectCard(-1);
        Player?.UpdateEffects();

        MelonLoader.MelonCoroutines.Start(ExecuteActionCoroutine());

        //GameActivity.this.endTurn.clearAnimation();
        // Disable end turn btn
        //GameActivity.this.checkEndTurnSkill();
    }

    private void OnVictory()
    {
        SoundManager.PlaySound(SoundManager.SOUND_FIGHT_WON, SoundManager.GeneralGroup);

        UiManager?.VictoryUI?.SetActive(true);
        if (UiManager != null && UiManager.VictoryBtn != null)
            UiManager.VictoryBtn.enabled = true;
    }

    internal void VictoryUIClose()
    {
        if (UiManager != null && UiManager.VictoryBtn != null)
            UiManager.VictoryBtn.enabled = false;

        RewardManager rewardManager = new(UiManager);
        rewardManager.OpenRewardUI();
    }

    internal void OnDefeat()
    {
        SoundManager.PlaySound(SoundManager.SOUND_FIGHT_LOST, SoundManager.GeneralGroup);

        UiManager?.ShowDeathUI();
    }

    internal void Reset()
    {
        // Reset first fight flag
        _isFirstFight = true;

        // Reset indexes
        SelectedEnemyIndex = -1;

        // Reset enemies
        for (int i = 0; i < _enemies.Length; i++) { _enemies[i] = null; }
    }

    #region World

    private World? CurrentWorld;

    internal void SetWord(World world)
    {
        CurrentWorld = world;
    }

#nullable disable

    internal World GetWorld()
    {
        if (CurrentWorld == null)
            SetWord(new Worlds.Forest());
        return CurrentWorld;
    }

#nullable enable

    #endregion World

    #region Player

    #region Card

    internal void AddCardPermanent(HeroCard card) => this.CardManager?.AddCard(card, true);

    internal void AddCard(HeroCard card) => this.CardManager?.AddCard(card, false);

    internal HeroCard? GetCard() => this.CardManager?.GetCard();

    internal void SelectCard(int index) => this.CardManager?.SelectCard(index);

    internal int GetCounter(string name) => this.CardManager?.GetCounter(name) ?? 0;

    internal int AddCounter(string name, int value) => this.CardManager?.AddCounter(name, value) ?? 0;

    #endregion Card

    #region Effect

    internal int GetEffectLevelPlayer<T>() where T : Effect => GetEffectLevelTarget<T>(Player);

    internal void AddPermanentLevelPlayer<T>(int amount) where T : Effect => AddPermaLevelToTarget<T>(null, Player, amount);

    /// <summary>
    /// Adds <paramref name="amount"/> to <see cref="Effect.Level"/> of the effect of <paramref name="type"/>
    /// </summary>
    /// <remarks>
    /// If no instance of an effect of <paramref name="type"/> is found, an instance will be created
    /// </remarks>
    internal void AddLevelPlayer<T>(int amount) where T : Effect => AddLevelToTarget<T>(null, Player, amount);

    #endregion Effect

    internal int GetPlayerDamage(int damage) => Player?.GetAttack(damage) ?? 0;

    internal void GainMana(uint amount) => Player?.AddMana(amount);

    #endregion Player

    #region Coroutines

    private IEnumerator ExecuteActionCoroutine()
    {
        // Lock all cards
        UiManager?.SetLockState(true);

        if (Player != null)
            Player.AreCardsLocked = true;

        foreach (var item in _enemies) item?.PreTurn();

        float value = (float)Main.GetSettingValue<double>(Main.SETTING_ENEMY_SPEED);

        foreach (var item in _enemies)
        {
            // If the item is null or the player is dead
            if (item == null || Player?.IsAlive == false)
                continue;

            yield return new WaitForSeconds(value);

            if (item == null)
            {
#if DEBUG
                Log("Tried to execute an action on an enemy that does not exist.");
#endif
            }
            else
            {
                if (Player == null)
                {
                    Log("No instance of Player was defined.");
                    continue;
                }

                item.PreAction();

                yield return new WaitForSeconds(item.ExecuteIntent()); // Wait for animation

                item.PostAction();
            }
        }

        // If player is alive
        if (Player?.IsAlive ?? false)
        {
            // Execute enemies' post turn
            foreach (var item in _enemies) item?.PostTurn();
        }

        StartPlayerTurn();
    }

    #endregion Coroutines

    #region Enemy

    private int SelectedEnemyIndex = -1;

    /// <summary>
    /// Array of the enemies. If the item is null, no enemy is at this position
    /// </summary>
    private readonly EnemyEntity?[] _enemies = new EnemyEntity?[MaxEnemiesCount];

    internal void SelectEnemy(int index)
    {
        SelectedEnemyIndex = index;

#if DEBUG
        Log("The current enemy selected is: " + SelectedEnemyIndex);
#endif

        if (Player?.Mana == 0 || Player?.IsAlive == false)
            return;

        this.CardManager?.PlayCard();
    }

    /// <summary>
    /// Tries to add an enemy of type <paramref name="enemyType"/> to the scene
    /// </summary>
    /// <param name="enemyType"></param>
    /// <returns>Has the enemy been added properly ?</returns>
    internal bool AddEnemy(EnemyCard enemyCard, bool initIntent)
    {
        // Possible changes:
        // - Check here if the type is valid, instead of assuming it has been validated before

        // If the give enemy type is not valid (null if EnemyManager was not defined or false if invalid)
        //if (EnemyManager?.IsEnemyTypeValid(enemyType) != true)
        //    return;

        int position = -1;

        for (int i = 0; i < _enemies.Length; i++)
        {
            if (_enemies[i] == null)
            {
                position = i;
                break;
            }
        }

        if (position == -1)
        {
#if DEBUG
            Log("No valid position found.");
#endif
            return false;
        }

        _enemies[position] = UiManager?.AddEnemy(enemyCard, position);

        if (initIntent)
            _enemies[position]?.SetIntent(new WaitAction());
        return _enemies[position] != null;
    }

    /// <summary>
    /// Kills the <see cref="EnemyEntity"/> at <paramref name="position"/>
    /// </summary>
    /// <returns>The enemy at <paramref name="position"/> died correctly</returns>
    internal bool KillEnemy(int position)
    {
        if (position < 0 || position >= _enemies.Length)
            return false;

        EnemyEntity? enemyKilled = _enemies[position];

        if (enemyKilled == null)
            return false;

        Player?.AddCoins(enemyKilled.GetCoinsGiven());

        UiManager?.KillEnemy(position, _enemies[position]);
        _enemies[position] = null;

        // OnDeathAbility

        if (GetEnemyCount() == 0)
            OnVictory();
        return true;
    }

    #region Attack

    /// <summary>
    /// Attacks the selected enemy with the given damage
    /// </summary>
    internal void AttackEnemy(Damage damage) => AttackEnemy(damage, _enemies[SelectedEnemyIndex]);

    /// <summary>
    /// Attacks all enemies the given damage
    /// </summary>
    internal void AttackAllEnemies(Damage damage)
    { foreach (var item in _enemies) AttackEnemy(damage, item); }

    /// <summary>
    /// Attacks <paramref name="target"/> with <paramref name="damage"/>
    /// </summary>
    private void AttackEnemy(Damage damage, Entity? target)
    {
        Player?.SetDamage(damage.Amount);
        damage.Amount = Player?.GetAttack() ?? 0;
        Player?.AttackTarget(damage, target);
    }

    #endregion Attack

    #region Effect

    /// <summary>
    /// Adds <paramref name="amount"/> to <see cref="Effect.Level"/> of the effect of <paramref name="type"/>
    /// </summary>
    /// <remarks>
    /// If no instance of an effect of <paramref name="type"/> is found, an instance will be created
    /// </remarks>
    internal void AddLevelEnemy<T>(int amount) where T : Effect => AddLevelToTarget<T>(Player, _enemies[SelectedEnemyIndex], amount);

    /// <summary>
    /// Adds <paramref name="amount"/> to <see cref="Effect.Level"/> of the effect of <paramref name="type"/>
    /// </summary>
    /// <remarks>
    /// If no instance of an effect of <paramref name="type"/> is found, an instance will be created
    /// </remarks>
    internal void AddLevelToAll<T>(int amount) where T : Effect
    { foreach (var item in _enemies) AddLevelToTarget<T>(Player, item, amount); }

    private static void AddLevelToTarget<T>(Entity? source, Entity? target, int amount) where T : Effect
    {
        if (source != null)
        {
            // Modify the amount depending on the source
        }

        target?.AddLevel<T>(amount);
    }

    private static void AddPermaLevelToTarget<T>(Entity? source, Entity? target, int amount) where T : Effect
    {
        if (source != null)
        {
            // Modify the amount depending on the source
        }

        target?.AddPermanentLevel<T>(amount);
    }

    private static int GetEffectLevelTarget<T>(Entity? target) where T : Effect => target?.GetEffectLevel<T>() ?? 0;

    #endregion Effect

    /// <returns>Count of enemies still alive</returns>
    public int GetEnemyCount()
    {
        int count = 0;

        foreach (var item in _enemies)
        {
            if (item != null)
                count++;
        }
        return count;
    }

    /// <summary>
    /// Spawns a random encounter depending on the given parameters
    /// </summary>
    /// <param name="encounterType">Type of the encounter (Normal, Elite, Boss)</param>
    /// <param name="world">Name of the current world</param>
    private void PopulateEnemies(string encounterType, World? world)
    {
        if (world == null)
        {
            Log("No world was defined.");
            return;
        }

        CurrentWorld ??= new Worlds.Forest();

        EnemyCard[] enemies = world.GetEnemies(encounterType);

        for (int i = 0; i < MaxEnemiesCount; i++)
        {
            EnemyCard? enemy = enemies.Length > i ? enemies[i] : null;

            if (enemy == null)
            {
                UiManager?.SetEnemyState(false, i);
                continue;
            }

            AddEnemy(enemy, false);
        }
    }

    #endregion Enemy

    #region Bundle

    private static AssetBundle? MainBundle;

    internal GameObject? PlayerCardPrefab;
    internal GameObject? RewardCardPrefab;
    internal GameObject? EffectSlotPrefab;

    /// <summary>
    /// Tries to load the asset from <see cref="MainBundle"/>
    /// </summary>
    /// <typeparam name="T">Type of the asset</typeparam>
    /// <param name="name">Name of the asset</param>
    /// <returns>If found, returns the asset. If not, returns the default value of <typeparamref name="T"/></returns>
    internal static T? LoadAsset<T>(string name) where T : Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
    {
        if (MainBundle == null)
        {
            Log(nameof(MainBundle) + " was not defined.");
            return default;
        }

        UnityEngine.Object? asset = MainBundle.LoadAsset(name);

        if (asset == null)
        {
            Log($"No asset named \'{name}\' exists in the bundle.");
            return default;
        }

        try
        {
            T c = asset.Cast<T>();

#if DEBUG
            Log($"Loaded asset \'{name}\' ({typeof(T).Name}).");
#endif
            return c;
        }
        catch (Exception e)
        {
            Log(e.Message);
            return default;
        }
    }

    #endregion Bundle

    #region Logging

    /// <summary>
    /// Called whenever you need to check if <paramref name="obj"/> is null and display an error message if it is
    /// </summary>
    /// <returns>True if <paramref name="obj"/> is null</returns>
    public static bool LogNull<T>(T obj, string nameOfObj, [System.Runtime.CompilerServices.CallerMemberName] string source = "")
    {
        bool result = obj == null;
        if (result)
            Log($"The object \'{nameOfObj}\' of type \'{typeof(T).Name}\' cannot be null.", source);
        return result;
    }

    /// <summary>
    /// Called to display a message into the console
    /// </summary>
    public static void Log(object? obj, [System.Runtime.CompilerServices.CallerMemberName] string? source = null)
        => BTD_Mod_Helper.ModHelper.GetMod<Main>().LoggerInstance.Msg($"{(string.IsNullOrEmpty(source) ? "" : $"[{source}]")} {obj ?? "null"}");

    #endregion Logging
}