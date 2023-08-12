using BTD_Mod_Helper.Extensions;
using System;
using System.Collections.Generic;
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

    internal List<GameObject?> _rewards = new();

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

            // Get amount of rewards
            int amountOfRewards = 4;

            for (int i = 0; i < amountOfRewards; i++)
            {
                int index = i;

                GameObject? r = GameObject.Instantiate(GameManager.Instance.RewardCardPrefab, UiManager?.RewardHolder);
                r?.GetComponentInChildren<Button>().onClick.AddListener(new Function(() =>
                {
                    SelectReward(index);
                }));
                _rewards.Add(r);
            }

            UiManager?.RewardUI?.SetActive(true);
        }));
    }

    private void SelectReward(int index)
    {
        Log(index);
        CloseRewardUI();
    }

    private void CloseRewardUI()
    {
        // Removes the possibility of nulls
        if (LogNull(UiManager, nameof(UiManager)))
            return;

        UiManager?.UseLoading(postLoad: new Action(() =>
        {
            // Delete all rewards
            foreach (var item in _rewards) { item.Destroy(); }
            _rewards.Clear();

            UiManager.RewardUI?.SetActive(false);

            // Start new fight
            GameManager.Instance.StartFight();
        }));
    }
}
