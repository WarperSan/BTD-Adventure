using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Extensions;
using BTDAdventure.Components;
using BTDAdventure.Enemy_Actions;
using BTDAdventure.Entities;
using BTDAdventure.Worlds;
using System;
using UnityEngine;

namespace BTDAdventure.Managers;

internal class GameManager
{
    #region Constants

    private const string NAME_MAIN_BUNDLE = "btdadventure";

    public const int COUNT_MAX_ENEMIES = 3;
    public const int PLAYER_HAND_SIZE = 4;

    #endregion Constants

    internal static GameManager Instance { get; set; } = new();

    static GameManager()
    {
        MainBundle = ModContent.GetBundle<Main>(NAME_MAIN_BUNDLE);
        Log("Bundle Loaded");
    }

    internal PlayerEntity? Player;

    #region Managers

    private CardManager? CardManager;

    #endregion Managers

    private bool _isFirstFight;

    private string? _currentEncounterType = null;

    // Prevents infinite battles.
    // Each X turns, enemies will gain +1 base damage
    private const byte TurnsPerDifficulty = 10;

    private uint _turnCount = 0;
    public uint TurnCount => _turnCount;
    internal int FightDifficulty = 0;

    internal void StartGame()
    {
        Reset();

        //Main.blurMat = LoadAsset<Material>("BlurMat");

        TaskScheduler.ScheduleTask(new Action(() =>
        {
            Main.blurMat = LoadAsset<Material>("BlurMat");
        }), BTD_Mod_Helper.Api.Enums.ScheduleType.WaitForFrames, 3);

        // Remove base UI & add bundle UI
        Transform? mainUI = UIManager.SetUpMainUI();

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
        Player = new(mainUI.gameObject, ModContent.GetInstance<WarriorClass>());

        this.CardManager = new(Player.RogueClass, PLAYER_HAND_SIZE);

        Player?.UpdatePlayerUI(); // Put values on the labels

        CurrentWorld = ModContent.GetInstance<Forest>();

        UIManager.InitializeWorld(2);
        UIManager.SetGameUIActive(false);
    }

    internal void StartFight(string type)
    {
        try
        {
            _turnCount = uint.MaxValue;
            FightDifficulty = 0;
            _currentEncounterType = type;

            // Clear status
            // Set-up player cards UI
            // Copy owned cards to draw pile
            // Fill hand
            PopulatePlayer();

            // Build skills

            // Get enmies group
            // Build enemies
            PopulateEnemies(type, GetWorld());

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
            UIManager.InitPlayerCards();
    }

    private void StartPlayerTurn()
    {
        if (!Player?.IsAlive ?? false)
            return;

        GameEndCheck();

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
            item.AdvanceIntent();
        }

        // Check fore-turn skills

        Player?.PreTurn();

        // Restore mana
        Player?.ResetMana();

        // Fill hand
        this.CardManager?.ShuffleHand(!_isFirstFight);

        // Check pre-turn skills

        UIManager.SetEndTurnBtnInteractable(true);
    }

    public void EndTurn()
    {
        this.CardManager?.EmptyHand();
        SelectCard(-1);

        Player?.PostTurn();

        UIManager.StartCoroutine(ExecuteActionCoroutine());

        //GameActivity.this.endTurn.clearAnimation();
        // Disable end turn btn
        //GameActivity.this.checkEndTurnSkill();
    }

    private void OnVictory()
    {
        SoundManager.PlaySound(SoundManager.SOUND_FIGHT_WON, SoundManager.GeneralGroup);

        UIManager.SetVictoryUIActive(true);
        UIManager.SetVictoryBtnEnable(true);
    }

    internal void OnDefeat()
    {
        SoundManager.PlaySound(SoundManager.SOUND_FIGHT_LOST, SoundManager.GeneralGroup);

        UIManager.ShowDeathUI();
    }

    internal void VictoryUIClose()
    {
        UIManager.SetVictoryBtnEnable(false);
        RewardManager.OpenRewardUI(_currentEncounterType ?? MapGenerator.NODE_TYPE_NORMAL);
    }

    private void Reset()
    {
        // Reset first fight flag
        _isFirstFight = true;

        // Reset indexes
        SelectedEnemyIndex = -1;

        // Reset enemies
        for (int i = 0; i < _enemies.Length; i++) { _enemies[i] = null; }
    }

    private void GameEndCheck()
    {
        if (GetEnemyCount() == 0)
            OnVictory();
    }

    #region World

    private World? CurrentWorld;

    internal void SetWord(World world) => CurrentWorld = world;

    internal World GetWorld()
    {
        if (CurrentWorld == null)
            SetWord(new Worlds.Forest());
#nullable disable
        return CurrentWorld;
#nullable restore
    }

    #endregion World

    #region Card

    /// <summary>
    /// Adds <paramref name="card"/> to the global deck. This modification will be kept after the fight.
    /// </summary>
    internal void AddCardPermanent(HeroCard card) => this.CardManager?.AddCard(card, true);

    /// <summary>
    /// Adds <paramref name="card"/> to the local deck. This modification will not be kept after the fight.
    /// </summary>
    internal void AddCard(HeroCard card) => this.CardManager?.AddCard(card, false);

    /// <inheritdoc cref="CardManager.GetCard"/>
    internal HeroCard? GetCard() => this.CardManager?.GetCard();

    /// <inheritdoc cref="CardManager.SelectCard(int)"/>
    internal void SelectCard(int index) => this.CardManager?.SelectCard(index);

    /// <inheritdoc cref="CardManager.GetCounter(string)"/>
    internal int GetCounter(string name) => this.CardManager?.GetCounter(name) ?? 0;

    /// <inheritdoc cref="CardManager.AddCounter(string, int)"/>
    internal int AddCounter(string name, int value) => this.CardManager?.AddCounter(name, value) ?? 0;

    #endregion Card

    #region Player

    /// <returns>Amount of damage that an attack dealing <paramref name="damage"/> damage would deal to <see cref="GameManager.Player"/>.</returns>
    internal int GetPlayerDamage(int damage) => Player?.CalculateDamage(damage) ?? 0;

    internal void GainMana(uint amount) => Player?.AddMana(amount);

    #endregion Player

    #region Enemy

    private int SelectedEnemyIndex = -1;

    /// <summary>
    /// Array of the enemies. If the item is null, no enemy is at this position
    /// </summary>
    private readonly EnemyEntity?[] _enemies = new EnemyEntity?[COUNT_MAX_ENEMIES];

    internal void SelectEnemy(int index)
    {
        SelectedEnemyIndex = index;

#if DEBUG
        Log("The current enemy selected is: " + SelectedEnemyIndex);
#endif

        if (Player?.Mana == 0 || Player?.IsAlive == false)
            return;

        this.CardManager?.PlayCard();

        GameEndCheck();
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

        _enemies[position] = UIManager.AddEnemy(enemyCard, position);

        if (initIntent)
            _enemies[position]?.SetIntent(new WaitAction());
        return _enemies[position] != null;
    }

    /// <summary>
    /// Kills the <see cref="EnemyEntity"/> at <paramref name="position"/>
    /// </summary>
    /// <returns>This call killed an enemy.</returns>
    internal bool KillEnemy(int position)
    {
        if (position < 0 || position >= _enemies.Length)
            return false;

        EnemyEntity? enemyKilled = _enemies[position];

        if (enemyKilled == null)
            return false;

        Reward reward = enemyKilled.GetReward();
        reward = GetWorld().AlterEnemyReward(reward);
        reward.CollectReward();

        UIManager.KillEnemy(position, _enemies[position]);
        _enemies[position] = null;

        // OnDeathAbility
        return true;
    }

    /// <returns>Count of enemies still alive</returns>
    internal int GetEnemyCount()
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
    /// <param name="encounterType">Type of the encounter</param>
    /// <param name="world">World to take a group from</param>
    private void PopulateEnemies(string encounterType, World world)
    {
        EnemyCard[] enemies = world.GetEnemies(encounterType);

        for (int i = 0; i < COUNT_MAX_ENEMIES; i++)
        {
            EnemyCard? enemy = enemies.Length > i ? enemies[i] : null;

            if (enemy == null)
            {
                UIManager.SetEnemyState(false, i);
                continue;
            }

            AddEnemy(enemy, false);
        }
    }

    #endregion Enemy

    #region Attack

    /// <summary>
    /// Attacks the selected enemy with an attack dealing <paramref name="damage"/> damage.
    /// </summary>
    /// <returns>
    /// Total amount of damage dealt to the selected enemy.
    /// </returns>
    internal int AttackEnemy(Damage damage) => AttackEnemy(damage, _enemies[SelectedEnemyIndex]);

    /// <summary>
    /// Attacks all enemies with an attack dealing <paramref name="damage"/> damage.
    /// </summary>
    /// <returns>
    /// Total amount of damage dealt across all enemies.
    /// </returns>
    internal int AttackAllEnemies(Damage damage)
    {
        int amount = 0;
        foreach (var item in _enemies) 
            amount += AttackEnemy(damage, item);
        return amount;
    }

    /// <summary>
    /// Attacks <paramref name="target"/> with an attack dealing <paramref name="damage"/> damage.
    /// </summary>
    /// <returns>
    /// Total amount of damage dealt to <paramref name="target"/>.
    /// </returns>
    private int AttackEnemy(Damage damage, Entity? target)
    {
        if (Player == null || target == null)
            return -1;

        Player.SetDamage(damage.Amount);
        damage.Amount = Player.CalculateDamage();
        int damageAmount = Player.AttackTarget(damage, target);

        if (target != null)
            target.PlayEffectVisual("AttackEffect");

        return damageAmount;
    }

    #endregion Attack

    #region Effects

    // --- ENEMY ---
    /// <summary>
    /// Adds <paramref name="amount"/> levels to the effect of type <typeparamref name="T"/> to the target enemy.
    /// </summary>
    /// <inheritdoc cref="AddLevelToEnemyPosition{T}(Entity?, int, int)"/>
    internal int AddLevelEnemy<T>(int amount) where T : Effect => AddLevelToEnemyPosition<T>(Player, amount, SelectedEnemyIndex);

    /// <summary>
    /// Adds <paramref name="amount"/> levels to the effect of type <typeparamref name="T"/> to every enemy alive.
    /// </summary>
    /// <inheritdoc cref="AddLevelToEnemyPosition{T}(Entity?, int, int)"/>
    internal int[] AddLevelToAll<T>(int amount) where T : Effect
    {
        int[] levels = new int[COUNT_MAX_ENEMIES];

        for (int i = 0; i < COUNT_MAX_ENEMIES; i++)
            levels[i] = AddLevelToEnemyPosition<T>(Player, amount, i);

        return levels;
    }

    /// <summary>
    /// Adds <paramref name="amount"/> levels to the effect of type <typeparamref name="T"/> to 
    /// the enemy at <paramref name="position"/> from <paramref name="source"/>.
    /// </summary>
    /// <inheritdoc cref="AddLevelToTarget{T}(Entity?, Entity?, int)"/>
    private int AddLevelToEnemyPosition<T>(Entity? source, int amount, int position) where T : Effect 
        => AddLevelToTarget<T>(source, _enemies[position], amount);

    internal int GetEffectLevelEnemy<T>() where T : Effect => _enemies[SelectedEnemyIndex]?.GetEffectLevel<T>() ?? 0;

    // --- PLAYER ---
    /// <returns>Level of the effect of type <typeparamref name="T"/> on <see cref="GameManager.Player"/>.</returns>
    internal int GetEffectLevelPlayer<T>() where T : Effect => Player?.GetEffectLevel<T>() ?? 0;

    /// <summary>
    /// Adds <paramref name="amount"/> permanent levels of the effect of type <typeparamref name="T"/>
    /// to <see cref="GameManager.Player"/>.
    /// </summary>
    /// <inheritdoc cref="AddPermaLevelToTarget{T}(Entity?, Entity?, int)"/>
    internal int AddPermanentLevelPlayer<T>(int amount) where T : Effect => AddPermaLevelToTarget<T>(null, Player, amount);

    /// <summary>
    /// Adds <paramref name="amount"/> levels to the effect of type <paramref name="type"/>
    /// to <see cref="GameManager.Player"/>.
    /// </summary>
    /// <inheritdoc cref="AddLevelToTarget{T}(Entity?, Entity?, int)"/>
    internal int AddLevelPlayer<T>(int amount) where T : Effect => AddLevelToTarget<T>(null, Player, amount);

    // --- GENERAL ---
    /// <summary>
    /// Adds <paramref name="amount"/> levels to the effect of type <typeparamref name="T"/> to <paramref name="target"/> 
    /// from <paramref name="source"/>.
    /// </summary>
    /// <remarks>
    /// If the target is not defined, the returning value will be -1.
    /// </remarks>
    /// <inheritdoc cref="Entity.AddLevel{T}(int)"/>
    private static int AddLevelToTarget<T>(Entity? source, Entity? target, int amount) where T : Effect
    {
        if (target == null)
            return -1;

        if (source != null)
        {
            // Modify the amount depending on the source
        }

        return target.AddLevel<T>(amount);
    }

    /// <summary>
    /// Adds <paramref name="amount"/> permanent levels to the effect of type <typeparamref name="T"/> to <paramref name="target"/>
    /// from <paramref name="source"/>.
    /// </summary>
    /// <remarks>
    /// If the target is not defined, the returning value will be -1.
    /// </remarks>
    /// <inheritdoc cref="Entity.AddPermanentLevel{T}(int)"/>
    private static int AddPermaLevelToTarget<T>(Entity? source, Entity? target, int amount) where T : Effect
    {
        if (target == null)
            return -1;

        if (source != null)
        {
            // Modify the amount depending on the source
        }

        return target.AddPermanentLevel<T>(amount);
    }

    #endregion Effects

    #region Coroutines

    private IEnumerator ExecuteActionCoroutine()
    {
        // Lock all cards
        UIManager.SetLockState(true);

        if (Player != null)
            Player.AreCardsLocked = true;

        foreach (var item in _enemies) 
            item?.PreTurn();

        float value = (float)Settings.GetSettingValue<double>(Settings.SETTING_ENEMY_SPEED);

        foreach (var item in _enemies)
        {
            if (Player == null)
            {
                Log("No instance of Player was defined.");
                break;
            }

            // If there is no enemy defined at this position
            if (item == null)
                continue;

            // If the item is null or the player is dead
            if (item == null || Player.IsAlive == false)
                continue;

            yield return new WaitForSeconds(value);

            item.PreAction(null);

            yield return new WaitForSeconds(item.ExecuteIntent()); // Wait for animation

            item.PostAction(null);
        }

        // If player is alive
        if (Player?.IsAlive ?? false)
        {
            // Execute enemies' post turn
            foreach (var item in _enemies) 
                item?.PostTurn();
        }

        StartPlayerTurn();
    }

    #endregion Coroutines

    #region Bundle

    private static AssetBundle? MainBundle;

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
    /// Called whenever you need to check if <paramref name="obj"/> is null and display an error message if it is.
    /// </summary>
    /// <returns><paramref name="obj"/> is null.</returns>
    internal static bool LogNull<T>(T obj, string nameOfObj, [System.Runtime.CompilerServices.CallerMemberName] string source = "")
    {
        bool result = obj == null;
        if (result)
            Log($"The object \'{nameOfObj}\' of type \'{typeof(T).Name}\' cannot be null.", source);
        return result;
    }

    /// <summary>
    /// Called to display a message into the console
    /// </summary>
    internal static void Log(object? obj, [System.Runtime.CompilerServices.CallerMemberName] string? source = null)
        => BTD_Mod_Helper.ModHelper.GetMod<Main>().LoggerInstance.Msg($"{(string.IsNullOrEmpty(source) ? "" : $"[{source}]")} {obj ?? "null"}");

    #endregion Logging
}