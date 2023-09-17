using System.Collections.Generic;
using UnityEngine;

namespace BTDAdventure.Managers;

internal class CardManager
{
    #region Piles
    private readonly List<HeroCard> _globalCardList;
    private readonly List<HeroCard> _exileCardList = new();
    private readonly List<HeroCard> _discardCardList = new();
    private List<HeroCard> _drawCardList = new();

    public int DiscardPileCount => _discardCardList.Count;
    public int DrawPileCount => _drawCardList.Count;

    private readonly HeroCard?[] _hand = new HeroCard?[MaxPlayerCardCount];
    #endregion

    #region Player
    private int SelectedCardIndex = -1;
    internal HeroCard? GetCard() => SelectedCardIndex == -1 ? null : _hand[SelectedCardIndex];
    internal void SelectCard(int index)
    {
        GameManager.Instance.UiManager?.SetCardCursorState(SelectedCardIndex, false);

        SelectedCardIndex = index;

        if ((bool)Main.ShowCardCursor.GetValue())
            GameManager.Instance.UiManager?.SetCardCursorState(SelectedCardIndex, true);
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
    #endregion

    #region Counters
    private readonly Dictionary<string, int> CardCounters = new();

    internal int GetCounter(string name) => CardCounters.ContainsKey(name) ? CardCounters[name] : 0;
    internal int AddCounter(string name, int value)
    {
        int v = GetCounter(name) + value;
        CardCounters[name] = v;
        return v;
    }

    internal void ClearCounters() => CardCounters.Clear();
    #endregion

    internal void FillHand(int amount, int position = -1, bool swingAnimation = false)
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
                FillDrawBack();
            }

            int rdmIndex = Random.Range(0, _drawCardList.Count);

            // Select the card to draw
            HeroCard selectedCard = _drawCardList[rdmIndex];

            // Find the position of the new card in the hand
            int pos = position < 0 ? i : position;

            HeroCard? handCard = _hand[pos];

            if (handCard != null)
            {
                _discardCardList.Add(handCard);
                //(toExile ? _exileCardList : _discardCardList).Add(handCard);
            }

            _hand[pos] = selectedCard;
            GameManager.Instance.UiManager?.SetUpPlayerCard(pos, selectedCard, swingAnimation);

            (_drawCardList[rdmIndex], _drawCardList[^1]) = (_drawCardList[^1], _drawCardList[rdmIndex]);
            _drawCardList.RemoveAt(_drawCardList.Count - 1);
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

        GameManager.Instance.UiManager?.SetLockState(true);

        if (updatePiles)
            GameManager.Instance.Player?.UpdatePiles(this);
    }

    /// <summary>
    /// Adds a card to the local deck
    /// </summary>
    /// <param name="card"></param>
    internal void AddCard(HeroCard card)
    {
        _drawCardList.Add(card);
    }

    /// <summary>
    /// Adds a card to the deck
    /// </summary>
    /// <param name="card"></param>
    internal void AddCardPermanent(HeroCard card)
    {
        _globalCardList.Add(card);
        AddCard(card);
    }

    internal void ResetPiles()
    {
#if DEBUG
        Log("Reseting piles");
#endif

        EmptyHand(false);
        _discardCardList.Clear();
        _exileCardList.Clear();

        _drawCardList = new(_globalCardList);
    }

    private void FillDrawBack()
    {
        _drawCardList = new(_discardCardList);
        _discardCardList.Clear();

        // Shuffle
        for (int i = 0; i < _drawCardList.Count; i++)
        {
            int rdmIndex = Random.Range(0, _drawCardList.Count);

            (_drawCardList[0], _drawCardList[rdmIndex]) = (_drawCardList[rdmIndex], _drawCardList[i]);
        }

        SoundManager.PlaySound("shuffle", SoundManager.GeneralGroup);
    }

    internal CardManager(RogueClass rogueClass)
    {
        _globalCardList = rogueClass.InitialCards();
    }
}