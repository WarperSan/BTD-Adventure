using BTD_Mod_Helper.Api;
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
    #endregion

    internal static GameManager Instance { get; set; } = new();


    internal void Initialize()
    {
        EffectsType = InitializeAirport<Effect>();

        this.EnemyManager = new();

        MainBundle = ModContent.GetBundle<BTDAdventure>("btdadventure");
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
    #endregion

    #region Airports
    internal PlayerEntity? Player;

    internal Type[]? EffectsType;
    public bool IsTypeAnEffect(Type type) => IsGivenTypeInArray<Effect>(EffectsType, type, true);
    #endregion


    /// Game Loop:
    /// - You enter a fight
    ///     - Get enemies group
    ///     - Build player
    ///     - Build skills
    ///     - Build deck
    /// - You play 0-3 cards
    /// - You end your turn
    /// - Each enemy do their action
    /// - Repeat cards play if the player is alive
    /// 

    #region Piles
    private readonly List<HeroCard> _globalCardList = new();
    private readonly List<HeroCard> _exileCardList = new();
    private readonly List<HeroCard> _discardCardList = new();
    private List<HeroCard> _drawCardList = new();

    public int DiscardPileCount => _discardCardList.Count;
    public int DrawPileCount => _drawCardList.Count;

    private readonly HeroCard?[] _hand = new HeroCard?[MaxPlayerCardCount];
    #endregion

    #region Managers
    private UIManager? UiManager;
    private EnemyManager? EnemyManager;
    #endregion

    private bool _isFirstFight = true;

    // Prevents infinite battles.
    // Each X turns, enemies will gain +1 base damage
    private const byte TurnsPerDifficulty = 10;
    public int TurnCount { get; private set; }
    private byte _turnCount = 0;
    internal int FightDifficulty = 0;

    internal void StartGame()
    {
        RewardCardPrefab = UIManager.InitializeRewardPrefab();
        EffectSlotPrefab = GameObject.Instantiate(LoadAsset<GameObject>("EffectSlot"));

        PlayerCardPrefab = LoadAsset<GameObject>("Card");

        UiManager = new();

        // Remove base UI & add bundle UI
        Transform? mainUI = UiManager.SetUpMainUI();

        if (mainUI == null)
        {
            Log("No instance of Main UI was defined.");
            return;
        }

        Player = new(mainUI.gameObject, 50, new WarriorClass());

        List<HeroCard> c = Player.RogueClass.InitialCards();

        _globalCardList.Clear();
        foreach (var item in c)
        {
            _globalCardList.Add(item);
        }

        StartFight();
    }

    internal void StartFight()
    {
        try
        {
            _turnCount = 0;
            FightDifficulty = 0;

            // Clear status
            // Set-up player cards UI
            // Copy owned cards to draw pile
            // Fill hand
            PopulatePlayer();

            // Build skills

            // Get enmies group
            // Build enemies
            PopulateEnemies("normal", "forest");

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
        EmptyHand(false);
        _discardCardList.Clear();
        _exileCardList.Clear();

        // Set-up player cards UI if necessary
        if (_isFirstFight)
            this.UiManager?.InitPlayerCards();

        // Copy owned cards to draw pile
        _drawCardList = new(_globalCardList);
    }

    private void FillHand(int amount, int? position = null, bool swingAnimation = false)
    {
        if (amount > MaxPlayerCardCount)
        {
            Log($"Cannot fill a hand bigger than the set limit. ({amount} > {MaxPlayerCardCount})");
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            // Empty discard pile if needed
            if (_drawCardList.Count == 0)
            {
                _drawCardList = new(_discardCardList);
                _discardCardList.Clear();
            }

            int rdmIndex = UnityEngine.Random.Range(0, _drawCardList.Count);

            HeroCard selectedCard = _drawCardList[rdmIndex];

            int pos = position ?? i;

            // Update card
            //selectedCard.UpdateCard(ownCardsObjects[pos].transform);

            HeroCard? handCard = _hand[pos];

            if (handCard != null)
            {
                _discardCardList.Add(handCard);
            }

            _hand[pos] = selectedCard;
            UiManager?.SetUpPlayerCard(pos, selectedCard, swingAnimation);

            _drawCardList.RemoveAt(rdmIndex);
        }

        Player?.UpdatePiles();
    }

    /// <summary>
    /// Spawns a random encounter depending on the given parameters
    /// </summary>
    /// <param name="encounterType">Type of the encounter (Normal, Elite, Boss)</param>
    /// <param name="world">Name of the current world</param>
    private void PopulateEnemies(string encounterType, string world)
    {
        if (this.EnemyManager == null)
        {
            Log("Enemy Manager was not defined.");
            return;
        }

        Type?[] enemies = this.EnemyManager.GenerateEnemies(encounterType, world);

        for (int i = 0; i < enemies.Length; i++)
        {
            Type? item = enemies[i];

            if (item == null)
            {
                UiManager?.SetEnemyState(false, i);
                continue;
            }

            AddEnemy(item);
        }
    }

    private void StartPlayerTurn()
    {
        // Increases general difficulty
        _turnCount++;
        TurnCount++;

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
        FillHand(MaxPlayerCardCount, swingAnimation: TurnCount != 1);

        // Check pre-turn skills

        // Unlock all cards
        //UiManager.SetLockState(false); // Handle by UIManager.SwingCard
    }

    private void EmptyHand(bool updatePiles = true)
    {
        for (int i = 0; i < _hand.Length; i++)
        {
            HeroCard? handCard = _hand[i];

            if (handCard != null)
                _discardCardList.Add(handCard);

            _hand[i] = null;
        }

        if (updatePiles)
            Player?.UpdatePiles();
    }

    public void EndTurn()
    {
        EmptyHand();
        Player?.UpdateEffects();

        MelonLoader.MelonCoroutines.Start(ExecuteActionCoroutine());

        //GameActivity.this.endTurn.clearAnimation();
        // Disable end turn btn
        //GameActivity.this.checkEndTurnSkill();
    }

    private void EndFight()
    {
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

    private int tS = -1;
    private int eS = -1;
    internal HeroCard? GetCard() => tS == -1 ? null : _hand[tS];

    internal void SelectCard(int index)
    {
        tS = index;

        Log("The current card selected is: " + tS);
    }

    internal void SelectEnemy(int index)
    {
        eS = index;

        Log("The current enemy selected is: " + eS);

        if (tS == -1 || Player?.Mana == 0)
            return;

        if (Player == null)
        {
            Log("No player instance was assigned.");
            return;
        }

        // Play card
        Player.RemoveMana(1);

        _hand[tS]?.PlayCard();
        Entity.ExecuteOnEffect(Player, new Action<IPostActionEffect>(effect =>
        {
            IPostActionEffect.OnAction(effect, Player, _hand[tS]);
        }));

        FillHand(1, tS, true);

        tS = -1;
        eS = -1;
    }

    public void AddCard(HeroCard card) => _globalCardList.Add(card);

    internal int GetPlayerDamage(int damage) => Player?.GetAttack(damage) ?? 0;

    #region Coroutines
    IEnumerator ExecuteActionCoroutine()
    {
        // Lock all cards
        UiManager?.SetLockState(true);

        if (Player != null)
            Player.AreCardsLocked = true;

        foreach (var item in _enemies) { item?.ClearShield(); }

        float value = (float)(double)BTDAdventure.EnemySpeed.GetValue();

        foreach (var item in _enemies)
        {
            if (item == null)
                continue;

            Entity.ExecuteOnEffect(item, new Action<IPreTurnEffect>(effect =>
            {
                effect.OnEffect(item);
            }));
        }

        foreach (var item in _enemies)
        {
            if (item == null)
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
                Entity.ExecuteOnEffect(item, new Action<IPreActionEffect>(effect =>
                {
                    //IPreActionEffect.OnAction(effect, item, null);
                }));

                yield return new WaitForSeconds(item.ExecuteIntent()); // Wait for animation

                Entity.ExecuteOnEffect(item, new Action<IPostActionEffect>(effect =>
                {
                    IPostActionEffect.OnAction(effect, item, null);
                }));
            }
        }

        foreach (var item in _enemies)
        {
            if (item == null)
                continue;

            Entity.ExecuteOnEffect(item, new Action<IPostTurnEffect>(effect =>
            {
                effect.OnEffect(item);
            }));
        }

        StartPlayerTurn();
    }
    #endregion

    #region Enemy
    /// <summary>
    /// Array of the enemies. If the item is null, no enemy is at this position
    /// </summary>
    private readonly EnemyEntity?[] _enemies = new EnemyEntity?[MaxEnemiesCount];

    /// <summary>
    /// Tries to add an enemy of type <paramref name="enemyType"/> to the scene
    /// </summary>
    /// <param name="enemyType"></param>
    /// <returns>Has the enemy been added properly ?</returns>
    internal bool AddEnemy(Type enemyType)
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

        _enemies[position] = UiManager?.AddEnemy(enemyType, position);
        return _enemies[position] != null;
    }

    /// <summary>
    /// Kills the <see cref="EnemyEntity"/> at <paramref name="position"/>
    /// </summary>
    /// <returns>The enemy at <paramref name="position"/> died correctly</returns>
    public bool KillEnemy(int position)
    {
        if (position < 0 || position >= _enemies.Length)
            return false;

        EnemyEntity? enemyKilled = _enemies[position];

        if (enemyKilled == null)
            return false;

        Player?.AddCoins(enemyKilled.GetCoinsGiven());

        _enemies[position] = null;
        UiManager?.KillEnemy(position);

        // OnDeathAbility

        if (GetEnemyCount() == 0)
        {
            EndFight();
        }
        return true;
    }

    /// <summary>
    /// Fetches the <see cref="EnemyAction"/> which have the tag <paramref name="intent"/>
    /// </summary>
    /// <returns>The <see cref="EnemyAction"/> or null if not found</returns>
    internal static EnemyAction? GetIntentAction(string? intent)
    {
        if (string.IsNullOrEmpty(intent)) return null;
        var validActions = ModContent.GetContent<EnemyAction>().FindAll(x => x.Tag == intent);

        validActions.Sort();

        return validActions.Count > 0 ? validActions[0] : null;
    }

    /// <summary>
    /// Attacks the selected enemy with the given damage
    /// </summary>
    internal void AttackEnemy(Damage damage) => AttackEnemy(damage, _enemies[eS]);

    /// <summary>
    /// Attacks all enemies the given damage
    /// </summary>
    internal void AttackAllEnemies(Damage damage) { foreach (var item in _enemies) AttackEnemy(damage, item); }

    /// <summary>
    /// Attacks <paramref name="target"/> with <paramref name="damage"/>
    /// </summary>
    private void AttackEnemy(Damage damage, Entity? target)
    {
        Player?.SetDamage(damage.Amount);
        damage.Amount = Player?.GetAttack() ?? 0;
        Player?.AttackTarget(damage, target);
    }

    /// <summary>
    /// Adds <paramref name="amount"/> to <see cref="Effect.Level"/> of the effect of <paramref name="type"/>
    /// </summary>
    /// <remarks>
    /// If no instance of an effect of <paramref name="type"/> is found, an instance will be created
    /// </remarks>
    internal void AddLevelPlayer<T>(int amount) where T : Effect => AddLevelToTarget<T>(null, Player, amount);

    /// <summary>
    /// Adds <paramref name="amount"/> to <see cref="Effect.Level"/> of the effect of <paramref name="type"/>
    /// </summary>
    /// <remarks>
    /// If no instance of an effect of <paramref name="type"/> is found, an instance will be created
    /// </remarks>
    internal void AddLevelEnemy<T>(int amount) where T : Effect => AddLevelToTarget<T>(Player, _enemies[eS], amount);

    /// <summary>
    /// Adds <paramref name="amount"/> to <see cref="Effect.Level"/> of the effect of <paramref name="type"/>
    /// </summary>
    /// <remarks>
    /// If no instance of an effect of <paramref name="type"/> is found, an instance will be created
    /// </remarks>
    internal void AddLevelToAll<T>(int amount) where T : Effect { foreach (var item in _enemies) AddLevelToTarget<T>(Player, item, amount); }

    private static void AddLevelToTarget<T>(Entity? source, Entity? target, int amount) where T : Effect
    {
        if (source != null)
        {
            // Modify the amount depending on the source
        }

        target?.AddLevel<T>(amount);
    }

    internal void AddPermanentLevelPlayer<T>(int amount) where T : Effect => AddPermaLevelToTarget<T>(null, Player, amount);

    private static void AddPermaLevelToTarget<T>(Entity? source, Entity? target, int amount) where T : Effect
    {
        if (source != null)
        {
            // Modify the amount depending on the source
        }

        target?.AddPermanentLevel<T>(amount);
    }

    internal int GetEffectLevelPlayer<T>() where T : Effect => GetEffectLevelTarget<T>(Player);
    private static int GetEffectLevelTarget<T>(Entity? target) where T : Effect => target?.GetEffectLevel<T>() ?? 0;

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
    #endregion

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
            Log($"Loaded asset \'{name}\'.");
#endif
            return c;
        }
        catch (Exception e)
        {
            Log(e.Message);
            throw;
        }
    }
    #endregion

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
        => BTD_Mod_Helper.ModHelper.GetMod<BTDAdventure>().LoggerInstance.Msg($"{(string.IsNullOrEmpty(source) ? "" : $"[{source}]")} {obj ?? "null"}");
    #endregion
}