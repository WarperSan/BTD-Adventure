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
            UiManager?.GameUI?.SetActive(false);
            UiManager?.VictoryUI?.SetActive(false);

            // Generate rewards
            _rewards = GenerateRewards();

            for (int i = 0; i < _rewards.Length; i++)
            {
                SetUpReward(i, _rewards[i]);
            }

            UiManager?.RewardUI?.SetActive(true);
            GameManager.Instance.Player?.RemoveAllEffects();
        }));
    }

    // 3 cards + 1 bjms reward
    private Reward?[]? _rewards;
    private static Reward?[] GenerateRewards()
    {
        int amountOfRewards = 4;

        Reward?[] rewards = new Reward?[amountOfRewards];

        // Randomize the rewards
        var allCards = ModContent.GetContent<HeroCard>().Where(x => x.CanBeReward).ToList();

        for (int i = 0; i < 3; i++)
        {
            int rdmIndex = UnityEngine.Random.Range(0, allCards.Count);
            (allCards[rdmIndex], allCards[^1]) = (allCards[^1], allCards[rdmIndex]);
            rewards[i] = new Reward()
            {
                HeroCard = allCards[^1]
            };

            allCards.RemoveAt(allCards.Count - 1);
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
                UIManager.CreatePopupReward(reward);
            });
        }

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

            // Open map
            UiManager.MapGenerator?.ProgressLayer();
            UiManager.MapGenerator?.MapObjectsParent?.gameObject.SetActive(true);
        }));
    }

    internal struct Reward
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
}
