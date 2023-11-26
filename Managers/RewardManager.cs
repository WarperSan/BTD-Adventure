using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BTDAdventure.Managers;

internal abstract class RewardManager
{
    #region Constants

    public const int REWARD_AMOUNT_CARDS = 3;
    public const int REWARD_AMOUNT_BLOONJAMINS = 2;

    #endregion Constants

#nullable disable
    private static string _encounterType;
#nullable restore

    internal static List<GameObject?> _rewardsObj = new();

    private static int rewardIndex = -1;

    internal static void OpenRewardUI(string encounterType)
    {
        _encounterType = encounterType;

        UIManager.UseLoading(postLoad: new Action(() =>
        {
            UIManager.SetGameUIActive(false);
            UIManager.SetVictoryUIActive(false);

            // Generate rewards
            _rewards = GenerateRewards(REWARD_AMOUNT_CARDS, REWARD_AMOUNT_BLOONJAMINS);

            for (int i = 0; i < _rewards.Length; i++)
                SetUpReward(i, _rewards[i]);

            UIManager.SetRewardUIActive(true);
            GameManager.Instance.Player?.RemoveAllEffects();
        }));
    }

    // 3 cards + 1 bjms reward
    private static Reward?[]? _rewards;

    private static Reward?[] GenerateRewards(int cardRewards, int gemsRewards)
    {
        Reward?[] rewards = new Reward?[cardRewards + gemsRewards];

        // Randomize the rewards
        var allCards = ModContent.GetContent<HeroCard>().Where(x => x.CanBeReward).ToList();

        FilterCards(ref allCards);

        for (int i = 0; i < cardRewards; i++)
        {
            int rdmIndex = UnityEngine.Random.Range(0, allCards.Count);
            rewards[i] = new Reward()
            {
                HeroCard = allCards[rdmIndex]
            };

            if (allCards.Count + i < cardRewards)
                continue;

            (allCards[rdmIndex], allCards[^1]) = (allCards[^1], allCards[rdmIndex]);
            allCards.RemoveAt(allCards.Count - 1);
        }

        for (int i = 0; i < gemsRewards; i++)
        {
            rewards[cardRewards + i] = new Reward()
            {
                Bloonjamins = 3
            };
        }

        return rewards;
    }

    /// <summary>
    /// Removes every card that is not allowed in the current world.
    /// </summary>
    private static void FilterCards(ref List<HeroCard> cards)
    {
        World world = GameManager.Instance.GetWorld();

        for (int i = cards.Count - 1; i >= 0; i--)
        {
            if (!cards[i].CanBeReward || world.RewardCardAllowed(cards[i], _encounterType))
                continue;

            (cards[^1], cards[i]) = (cards[i], cards[^1]);
            cards.RemoveAt(cards.Count - 1);
        }
    }

    private static void SetUpReward(int index, Reward? reward)
    {
        if (!reward.HasValue)
            return;

        GameObject? r = UIManager.GetRewardCard();
        r?.transform.Find("Button")?.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (rewardIndex != -1)
                return;

            rewardIndex = index;
            CloseRewardUI();
        });

        var rwdBtn = r?.transform.Find("InfoBtn")?.GetComponent<Button>();
        var isRwdBtnActive = reward.Value.HeroCard != null;
        rwdBtn?.gameObject.SetActive(isRwdBtnActive);

        if (isRwdBtnActive)
        {
            rwdBtn?.onClick.AddListener(() =>
            {
                if (reward.Value.HeroCard == null)
                    return;

                UIManager.CreatePopupCard(reward.Value.HeroCard, true);
            });
        }

        _rewardsObj.Add(r);

        if (r == null)
            return;

        reward.Value.SetUpReward(out string? portrait, out string? title);

        r.transform.Find("Portrait").GetComponent<Image>().SetSprite(portrait);
        r.transform.Find("Banner/TextHolder/Text").GetComponent<NK_TextMeshProUGUI>().text = title;
    }

    private static void CloseRewardUI()
    {
        UIManager.UseLoading(postLoad: new Action(() =>
        {
            if (_rewards != null)
            {
                Reward? reward = _rewards[rewardIndex];

                if (reward.HasValue)
                    reward.Value.CollectReward(); // Give in the black screen
            }

            // Delete all rewards
            foreach (var item in _rewardsObj) { GameObject.Destroy(item); }

            // Reset
            _rewardsObj.Clear();
            rewardIndex = -1;

            // Disable UI
            UIManager.SetRewardUIActive(false);

            // Open map
            UIManager.AdvanceLayer();
        }));
    }
}

public struct Reward
{
    // Rewards can be a card, bloonjamins, nothing (or cash)
    public HeroCard? HeroCard;

    public uint? Bloonjamins;
    public uint? Cash;

    internal readonly void CollectReward()
    {
        if (Bloonjamins != null)
            GameManager.Instance.Player?.AddBloonjamins(Bloonjamins.Value);
        else if (Cash != null)
            GameManager.Instance.Player?.AddCoins(Cash.Value);
        else if (HeroCard != null)
            GameManager.Instance.AddCardPermanent(HeroCard);
#if DEBUG
        else
            Log("A reward with no valid content was claimed.");
#endif
    }

    internal readonly void SetUpReward(out string? portrait, out string? title)
    {
        portrait = null;
        title = null;

        if (Bloonjamins != null)
        {
            portrait = VanillaSprites.BundledBloonjaminsIcon;
            title = Bloonjamins + " Bloonjamins";
        }
        else if (Cash != null)
        {
            portrait = VanillaSprites.CoinIcon;
            title = Cash + " Coins";
        }
        else if (HeroCard != null)
        {
            portrait = HeroCard.Portrait;
            title = HeroCard.DisplayName;
        }
    }
}