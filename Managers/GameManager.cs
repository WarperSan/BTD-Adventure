using BTDAdventure.Abstract_Classes;
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

    public static GameManager Instance { get; set; } = new();


    internal void Initialize()
    {
        HeroTypes = InitializeAirport<HeroCard>();
        EffectsType = InitializeAirport<Effect>();

        this.EnemyManager = new();

        RegisterActions();

        MainBundle = BTD_Mod_Helper.Api.ModContent.GetBundle<BTDAdventure>("btdadventure");
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

    private Type[]? HeroTypes;
    private HeroCard? GetHero(int position)
    {
        if (HeroTypes == null)
            return null;

        if (position < 0 || position >= HeroTypes.Length)
        {
            Log($"The given index ({position}) is outside the valid range [0; {HeroTypes.Length - 1}].");
            return null;
        }

        HeroCard? hero = (HeroCard?)Activator.CreateInstance(HeroTypes[position]);

        if (hero == null)
        {
            Log($"Failed to create an instance of type \'{HeroTypes[position].Name}\'");
        }
        return hero;
    }

    internal Type[]? EffectsType;
    public bool IsTypeAnEffect(Type type) => IsGivenTypeInArray<Effect>(EffectsType, type, true);

    private readonly Dictionary<string, EnemyAction> EnemyActions = new();
    private void RegisterActions()
    {
        Type[] actions = InitializeAirport<EnemyAction>();
        foreach (var item in actions)
        {
            if (!CreateInstance<EnemyAction>(item, out var c))
                continue;

            if (c == null) // To make VS happy
                continue;

            if (EnemyActions.ContainsKey(c.Tag))
            {
                Log($"An action is already registered as \'{c.Tag}\'");
                continue;
            }

            EnemyActions.Add(c.Tag, c);
        }
    }
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

        Player = new(mainUI.gameObject, 50);

        List<HeroCard> c = new WarriorClass().InitialCards();

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

        // Play card
        Player?.RemoveMana(1);

        _hand[tS]?.PlayCard();
        FillHand(1, tS, true);

        tS = -1;
        eS = -1;
    }

    #region Coroutines
    IEnumerator ExecuteActionCoroutine()
    {
        // Lock all cards
        UiManager?.SetLockState(true);

        if (Player != null)
            Player.AreCardsLocked = true;

        foreach (var item in _enemies)
        {
            item?.ClearShield();
        }

        float value = (float)(double)BTDAdventure.EnemySpeed.GetValue();

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
                yield return new WaitForSeconds(item.ExecuteAction()); // Wait for animation
        }

        StartPlayerTurn();
    }
    #endregion

    #region Enemy
    private readonly EnemyEntity?[] _enemies = new EnemyEntity?[MaxEnemiesCount];

    internal void AddEnemy(Type enemyType)
    {
        // If the give enemy type is not valid (null if EnemyManager was not defined or false if invalid)
        if (EnemyManager?.IsEnemyTypeValid(enemyType) != true)
            return;

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
            return;
        }

        _enemies[position] = UiManager?.AddEnemy(enemyType, position);
    }
    public void KillEnemy(int position)
    {
        if (position < 0 || position >= _enemies.Length)
            return;

        EnemyEntity? enemyKilled = _enemies[position];

        if (enemyKilled != null)
            Player?.AddCoins(enemyKilled.GetCoinsGiven());

        _enemies[position] = null;
        UiManager?.KillEnemy(position);

        // OnDeathAbility

        if (GetEnemyCount() > 0)
            return;

        EndFight();
    }

    internal EnemyAction? GetIntentAction(string? intent)
    {
        if (intent == null)
            return null;
        return EnemyActions.TryGetValue(intent, out var action) ? action : null;
    }

    /// <summary>
    /// Attacks the selected enemy with the given damage
    /// </summary>
    internal void AttackEnemy(Damage damage) => AttackEnemy(damage, _enemies[eS]);

    /// <summary>
    /// Attacks all enemies the given damage
    /// </summary>
    internal void AttackAllEnemies(Damage damage) { foreach (var item in _enemies) AttackEnemy(damage, item); }

    private void AttackEnemy(Damage damage, Entity? target)
    {
        Player?.SetDamage(damage.Amount);
        damage.Amount = Player?.GetAttack() ?? 0;
        Player?.AttackTarget(damage, target);
    }

    // To player
    internal void AddLevelPlayer(Type type, int amount) => Player?.AddLevel(type, amount);

    // To enemy target
    internal void AddLevelEnemy(Type type, int amount) => _enemies[eS]?.AddLevel(type, amount);

    // To all enemies
    internal void AddLevelToAll(Type type, int amount) { foreach (var item in _enemies) item?.AddLevel(type, amount); }

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
    public static bool LogNull<T>(T obj, string nameOfObj, [System.Runtime.CompilerServices.CallerMemberName] string source = "")
    {
        bool result = obj == null;
        if (result)
            Log($"The object \'{nameOfObj}\' of type \'{typeof(T).Name}\' cannot be null.", source);
        return result;
    }
    public static void Log(object? obj, [System.Runtime.CompilerServices.CallerMemberName] string source = "") => BTD_Mod_Helper.ModHelper.GetMod<BTDAdventure>().LoggerInstance.Msg($"[{source}] {obj ?? "null"}");
    #endregion
}