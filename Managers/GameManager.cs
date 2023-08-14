global using static BTDAdventure.Managers.GameManager;
global using IEnumerator = System.Collections.IEnumerator;
global using static BTDAdventure.EnemyAction;

using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTDAdventure.Cards.EnemyCards;
using BTDAdventure.Cards.HeroCard;
using Il2CppInterop.Runtime.InteropTypes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BTDAdventure.Managers;

internal class GameManager
{
    #region Constants
    public const int MaxEnemiesCount = 3;
    public const int MaxPlayerCardCount = 4;
    #endregion

    public static GameManager Instance = new();

    private readonly Dictionary<string, EnemyAction> EnemyActions = new();

    internal void Initialize()
    {
        //EnemiesTypes = InitializeAirport<EnemyCard>();
        HeroTypes = InitializeAirport<HeroCard>();
        this.EnemyManager = new();

        RegisterActions();

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

    private readonly List<HeroCard> _globalCardList = new();
    private readonly List<HeroCard> _exileCardList = new();
    private readonly List<HeroCard> _discardCardList = new();
    private List<HeroCard> _drawCardList = new();
    private readonly HeroCard?[] _hand = new HeroCard?[MaxPlayerCardCount];

    private bool _isFirstFight = true;

    private readonly EnemyCard?[] _enemies = new EnemyCard?[MaxEnemiesCount];

    #region Managers
    private UIManager? UiManager;
    private EnemyManager? EnemyManager;
    #endregion

    internal void StartGame()
    {
        PlayerCardPrefab = LoadAsset<GameObject>("Card");

        EnemyCardPrefab = UIManager.InitializeEnemyPrefab();
        RewardCardPrefab = UIManager.InitializeRewardPrefab();

        UiManager = new();

        // Remove base UI & add bundle UI
        UiManager.SetUpMainUI();

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
            UiManager?.UpdateHealthText();
            UiManager?.UpdateCashText();
            UiManager?.UpdateBloonjaminsText();

            // Clear status
            // Set-up player cards UI
            // Copy owned cards to draw pile
            // Fill hand
            PopulatePlayer();

            // Build skills

            // Get enmies group
            // Build enemies
            PopulateEnemies();

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
        // Clear all status

        // Set-up player cards UI
        if (_isFirstFight)
            this.UiManager?.InitPlayerCards();

        // Copy owned cards to draw pile
        _drawCardList = new(_globalCardList);

        // Fill hand
        FillHand(MaxPlayerCardCount);
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
    }

    private void PopulateEnemies()
    {
        if (this.EnemyManager != null)
        {
            Type?[] enemies = this.EnemyManager.GenerateEnemies("normal", "forest");

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
        else
            Log("Enemy Manager was not defined.");
    }

    private void StartPlayerTurn()
    {
        //this.turnEndedCheck = false;
        //if (this.mFinished)
        //    return;
        //showTurn(R.string.message_your_turn, false);
        // Check skills
        //setBuffIcon();
        // Clear hand

        // Check for skills that changes drawn pile
        // Check skills
        // Enable end turn btn


        foreach (var item in _enemies)
        {
            item?.MoveIntent();
        }

        // Check fore-turn skills
        // Clear hand
        // Check pre-turn skills

        // Restore mana
        SetMana(MaxMana);

        // Fill hand
        FillHand(MaxPlayerCardCount, swingAnimation: _cardsLocked);

        // Unlock all cards
        //UiManager.SetLockState(false); // Handle by UIManager.SwingCard
    }

    public void EndTurn()
    {
        MelonLoader.MelonCoroutines.Start(ExecuteActionCoroutine());

        //GameActivity.this.endTurn.clearAnimation();
        // Disable end turn btn
        //GameActivity.this.checkEndTurnSkill();
    }

    #region Coroutines
    IEnumerator ExecuteActionCoroutine()
    {
        // Lock all cards
        UiManager?.SetLockState(true);
        _cardsLocked = true;

        float value = (float)(double)BTDAdventure.EnemySpeed.GetValue();

        foreach (var item in _enemies)
        {
            if (item == null)
                continue;

            yield return new WaitForSeconds(value);
            yield return new WaitForSeconds(ExecuteAction(item)); // Wait for animation
        }

        StartPlayerTurn();
    }
    #endregion

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

        if (tS == -1 || Mana == 0)
            return;

        // Play card
        _hand[tS]?.PlayCard();
        FillHand(1, tS);

        RemoveMana(1);
    }

    #region Player
    private bool _cardsLocked = false;

    internal uint coins;
    internal uint bjms;

    internal void AddCoins(uint amount)
    {
        coins += amount;
        UiManager?.UpdateCashText();
    }

    internal void AddBloonjamins(uint amount)
    {
        bjms += amount;
        UiManager?.UpdateBloonjaminsText();
    }

    #region Health
    internal int Health = 50;
    internal int MaxHealth = 50;

    public void DamagePlayer(Damage damage)
    {
        Health -= damage.Amount;

        UiManager?.UpdateHealthText();
    }
    #endregion

    #region Mana
    internal uint Mana;
    internal uint MaxMana = 3;

    public void AddMana(uint amount) => SetMana(Mana + amount);
    public void RemoveMana(uint amount) => SetMana(Mana - amount);
    private void SetMana(uint amount)
    {

#if DEBUG
        Log($"Setting mana from {Mana} to {amount}.");
#endif

        Mana = amount;
        UiManager?.UpdateManaText();

        /*if (this.player.player_silence > 0 && i > 0 && !z)
        {
            showMessage(getString(R.string.silenced), false, 0, 0, 0, 0, 800L);
            return;
        }*/
    }
    #endregion
    #endregion

    #region Hero
    private Type[]? HeroTypes;
    #endregion

    #region Enemy
    internal void AddEnemy(Type enemyType)
    {
        if (EnemyManager != null && !EnemyManager.IsEnemyTypeValid(enemyType))
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

        EnemyCard? enemyKilled = _enemies[position];

        if (enemyKilled != null)
            AddCoins(enemyKilled.coinsGiven);

        _enemies[position] = null;
        UiManager?.KillEnemy(position);

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

    private static float ExecuteAction(EnemyCard? enemy)
    {
        if (enemy == null)
        {
#if DEBUG
            Log("Tried to execute an action on an enemy that does not exist.");
#endif
            return 0;
        }

        enemy.ExecuteIntent();

        return 0.5333333f; // 32 frames
    }

    /// <summary>
    /// Attacks the selected enemy with a damage attack dealing <paramref name="amount"/> HP
    /// </summary>
    public void AttackEnemy(int amount) => AttackEnemy(new Damage(amount));

    /// <summary>
    /// Attacks the selected enemy with the given damage
    /// </summary>
    public void AttackEnemy(Damage damage) => AttackEnemy(damage, _enemies[eS]);

    /// <summary>
    /// Attacks all enemies with a default damage dealing <paramref name="amount"/> HP
    /// </summary>
    public void AttackAllEnemies(int amount) => AttackAllEnemies(new Damage(amount));

    /// <summary>
    /// Attacks all enemies the given damage
    /// </summary>
    public void AttackAllEnemies(Damage damage) { foreach (var item in _enemies) AttackEnemy(damage, item); }

    /// <summary>
    /// Attacks the given enemy if the given damage
    /// </summary>
    internal static void AttackEnemy(Damage damage, EnemyCard? enemy)
    {
        // Apply skills
        // Apply abilities

        enemy?.ReceiveDamage(damage);
    }

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
    internal GameObject? EnemyCardPrefab;
    internal GameObject? RewardCardPrefab;

    internal static T? LoadAsset<T>(string name) where T : Il2CppObjectBase
    {
        if (MainBundle == null)
        {
            Log(nameof(MainBundle) + " was not defined.");
            return default;
        }

        Object? asset = MainBundle.LoadAsset(name);

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
    public static bool LogNull<T>(T obj, string nameOfObj, [CallerMemberName] string source = "")
    {
        if (obj == null)
        {
            Log($"The object \'{nameOfObj}\' of type \'{typeof(T).Name}\' cannot be null.", source);
            return true;
        }
        return false;
    }
    public static void Log(object? obj, [CallerMemberName] string source = "")
    {
        ModHelper.GetMod<BTDAdventure>().LoggerInstance.Msg($"[{source}] {obj ?? "null"}");
    }
    #endregion
}