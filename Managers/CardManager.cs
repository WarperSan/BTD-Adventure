using System.Collections.Generic;
using UnityEngine;

namespace BTDAdventure.Managers;

internal class CardManager
{
    #region Piles

    /// <summary>
    /// List of the cards that the player has
    /// </summary>
    private readonly List<HeroCard> _globalCardList;

    /// <summary>
    /// List of the cards that are in the exile pile
    /// </summary>
    private readonly List<HeroCard> _exileCardList = new();

    /// <summary>
    /// List of the cards that are in the discard pile
    /// </summary>
    private readonly List<HeroCard> _discardCardList = new();
    public int DiscardPileCount => _discardCardList.Count;

    /// <summary>
    /// List of the cards that are in the draw pile
    /// </summary>
    private List<HeroCard> _drawCardList = new();
    public int DrawPileCount => _drawCardList.Count;

    internal void ResetPiles()
    {
        EmptyHand(false);
        _discardCardList.Clear(); // Clear discard pile
        _exileCardList.Clear(); // Clear exile pile

        _drawCardList = new(_globalCardList); // Set local deck to global deck
    }

    internal void AddCard(HeroCard card, bool isPermanent)
    {
        if (isPermanent)
            _globalCardList.Add(card);

        _drawCardList.Add(card);
    }

    private void FillDrawBack()
    {
        if (_discardCardList.Count == 0)
            return;

        _drawCardList = new(_discardCardList);
        _discardCardList.Clear();

        // Shuffle
        for (int i = 0; i < _drawCardList.Count; i++)
        {
            int rdmIndex = Random.Range(0, _drawCardList.Count);

            (_drawCardList[i], _drawCardList[rdmIndex]) = (_drawCardList[rdmIndex], _drawCardList[i]);
        }

        SoundManager.PlaySound(SoundManager.SOUND_SHUFFLE, SoundManager.GeneralGroup);
    }

    #endregion Piles

    #region Player

    private int SelectedCardIndex = -1;

    /// <returns>Selected card</returns>
    internal HeroCard? GetCard() => SelectedCardIndex == -1 ? null : _hand[SelectedCardIndex];

    internal void SelectCard(int index)
    {
        // Deselect the current selected card
        UIManager.SetCardCursorState(SelectedCardIndex, false);

        // Update selected card
        SelectedCardIndex = index;

        // Select the current selected card
        if (Settings.GetSettingValue(Settings.SETTING_SHOW_CARD_CURSOR, true))
            UIManager.SetCardCursorState(SelectedCardIndex, true);
    }

    internal void PlayCard()
    {
        if (GetCard() == null) return;

        if (GameManager.Instance.Player == null)
        {
            Log("No player instance was assigned.");
            return;
        }

        HeroCard? card = GetCard();

        // Play card
        GameManager.Instance.Player.RemoveMana(1);

        bool toExile = false;

        if (card != null)
        {
            GameManager.Instance.Player.PreAction(card);

            if (!GameManager.Instance.Player.BlockCardOnPlay(card))
            {
                card.PlayCard();
                toExile = card.ExileOnPlay;
            }

            GameManager.Instance.Player.PostAction(card);
        }

        FillHand(1, SelectedCardIndex, true);

        SelectCard(-1);
    }

    #endregion Player

    #region Counters

    private readonly Dictionary<string, int> CardCounters = new();

    /// <returns>Value of the counter associated with the <paramref name="name"/></returns>
    internal int GetCounter(string name) => CardCounters.TryGetValue(name, out int value) ? value : 0;

    /// <summary>
    /// Adds <paramref name="value"/> to the value of the counter with the name <paramref name="name"/>
    /// </summary>
    /// <returns>The value of the counter after the modification.</returns>
    internal int AddCounter(string name, int value)
    {
        int v = GetCounter(name) + value;
        CardCounters[name] = v;
        return v;
    }

    /// <summary>
    /// Clears the registered counters
    /// </summary>
    internal void ClearCounters() => CardCounters.Clear();

    #endregion Counters

    #region Hand

    /// <summary>
    /// Cards that make the player's hand
    /// </summary>
    private readonly HeroCard?[] _hand;

    internal void ShuffleHand(bool swingAnimation) => FillHand(_hand.Length, swingAnimation: swingAnimation);

    private void FillHand(int amount, int position = -1, bool swingAnimation = false)
    {
        if (amount > _hand.Length)
        {
            Log($"Cannot fill a hand bigger than the hand itself. ({amount} > {_hand.Length})");
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            // Find the position of the new card in the hand
            int pos = position < 0 ? i : position;

            HeroCard? handCard = _hand[pos];

            if (handCard != null)
            {
                //_discardCardList.Add(handCard);
                (handCard.ExileOnPlay ? _exileCardList : _discardCardList).Add(handCard);
            }

            // Empty discard pile if needed
            if (_drawCardList.Count == 0)
            {
                FillDrawBack();
            }

            int rdmIndex = Random.Range(0, _drawCardList.Count);

            bool hasEnoughCards = _drawCardList.Count != 0;

            // Select the card to draw
            HeroCard? selectedCard = hasEnoughCards ?
                _drawCardList[rdmIndex] :
                null;

            _hand[pos] = selectedCard;
            UIManager.SetUpPlayerCard(pos, selectedCard, swingAnimation);

            if (hasEnoughCards)
            {
                (_drawCardList[rdmIndex], _drawCardList[^1]) = (_drawCardList[^1], _drawCardList[rdmIndex]);
                _drawCardList.RemoveAt(_drawCardList.Count - 1);
            }
        }

        GameManager.Instance.Player?.UpdatePiles(this);
    }

    internal void EmptyHand(bool updatePiles = true)
    {
        for (int i = 0; i < _hand.Length; i++)
        {
            HeroCard? handCard = _hand[i];

            if (handCard != null)
                _discardCardList.Add(handCard);

            _hand[i] = null;
        }

        UIManager.SetLockState(true);

        if (updatePiles)
            GameManager.Instance.Player?.UpdatePiles(this);
    }

    #endregion

    internal CardManager(RogueClass rogueClass, int handSize)
    {
        _globalCardList = rogueClass.InitialCards();
        _hand = new HeroCard?[handSize];
    }
}