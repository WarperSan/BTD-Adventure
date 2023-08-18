using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2Cpp;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BTDAdventure.Managers;

internal class RewardManager
{
    private readonly UIManager? UiManager;

    public RewardManager(UIManager? uIManager)
    {
        this.UiManager = uIManager;
    }

    internal System.Collections.Generic.List<GameObject?> _rewardsObj = new();

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
    private Reward?[] GenerateRewards()
    {
        int amountOfRewards = 4;

        Reward?[] rewards = new Reward?[amountOfRewards];

        // Randomize the rewards

        rewards[0] = new Reward()
        {
            HeroCard = typeof(DartMonkey000)
        };
        rewards[1] = new Reward()
        {
            HeroCard = typeof(MonkeyVillage000)
        };
        rewards[2] = new Reward()
        {
            HeroCard = typeof(WizardMonkey000)
        };
        rewards[3] = new Reward()
        {
            Bloonjamins = 3
        };

        return rewards;
    }

    /*
     * 

    */

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
        public Type? HeroCard;
        public uint? Bloonjamins;
        public uint? Cash;

        internal void CollectReward()
        {
            if (Bloonjamins != null)
                GameManager.Instance.Player?.AddBloonjamins(Bloonjamins.Value);
            else if (Cash != null)
                GameManager.Instance.Player?.AddCoins(Cash.Value);
            else if (HeroCard != null)
            {
                // Add card
            }
        }

        internal void SetUpReward(out string? portrait, out string? title)
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
                HeroCard? card = Activator.CreateInstance(HeroCard) as HeroCard;

                portrait = card?.Portrait;
                title = card?.DisplayName;
            }
        }
    }
}
