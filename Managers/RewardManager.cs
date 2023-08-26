using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2Cpp;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTDAdventure.Managers;

internal class RewardManager(UIManager? uIManager)
{
    private readonly UIManager? UiManager = uIManager;
    internal List<GameObject?> _rewardsObj = new();

    int rewardIndex = -1;

    internal void OpenRewardUI()
    {
        // Removes the possibility of nulls
        if (LogNull(UiManager, nameof(UiManager)))
            return;

        // Removes the possibility of nulls
        if (LogNull(UiManager?.RewardUI, nameof(UiManager.RewardUI)))
            return;

        UiManager?.UseLoading(postLoad: new Action(() =>
        {
            UiManager?.VictoryUI?.SetActive(false);

            // Generate rewards
            _rewards = GenerateRewards();

            for (int i = 0; i < _rewards.Length; i++)
            {
                SetUpReward(i, _rewards[i]);
            }

            UiManager?.RewardUI?.SetActive(true);
        }));
    }

    // 3 cards + 1 bjms reward
    private Reward?[]? _rewards;
    private static Reward?[] GenerateRewards()
    {
        int amountOfRewards = 4;

        Reward?[] rewards = new Reward?[amountOfRewards];

        // Randomize the rewards
        var allCards = ModContent.GetContent<HeroCard>();

        List<int> positions = new();

        for (int i = 0; i < allCards.Count; i++) positions.Add(i);

        for (int i = 0; i < 3; i++)
        {
            int rdmIndex = UnityEngine.Random.Range(0, positions.Count);
            rewards[i] = new Reward()
            {
                HeroCard = allCards[positions[rdmIndex]]
            };

            positions.RemoveAt(rdmIndex);
        }

        rewards[^1] = new Reward()
        {
            Bloonjamins = 3
        };

        return rewards;
    }

    private void SetUpReward(int index, Reward? reward)
    {
        if (!reward.HasValue)
            return;

        GameObject? r = GameObject.Instantiate(GameManager.Instance.RewardCardPrefab, UiManager?.RewardHolder);
        r?.GetComponentInChildren<Button>().onClick.AddListener(new Function(() =>
        {
            if (rewardIndex != -1)
                return;

            rewardIndex = index;
            CloseRewardUI();
        }));

        _rewardsObj.Add(r);

        if (r == null)
            return;

        reward.Value.SetUpReward(out string? portrait, out string? title);

        r.transform.Find("Portrait").GetComponent<Image>().SetSprite(portrait);
        r.transform.Find("Banner/TextHolder/Text").GetComponent<NK_TextMeshProUGUI>().text = title;
    }

    private void CloseRewardUI()
    {
        // Removes the possibility of nulls
        if (LogNull(UiManager, nameof(UiManager)))
            return;

        UiManager?.UseLoading(postLoad: new Action(() =>
        {
            if (_rewards != null)
            {
                Reward? reward = _rewards[rewardIndex];

                if (reward.HasValue)
                    reward.Value.CollectReward(); // Give in the black screen
            }

            // Delete all rewards
            foreach (var item in _rewardsObj) { GameObject.Destroy(item); }
            _rewardsObj.Clear();

            UiManager.RewardUI?.SetActive(false);

            // Start new fight
            GameManager.Instance.StartFight();
        }));
    }

    struct Reward
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
                GameManager.Instance.AddCard(HeroCard);
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
}
