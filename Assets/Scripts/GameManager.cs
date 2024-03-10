using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject upgradeUIPrefab; // Assign a prefab with TMP text, image for icon, etc.
    public Transform upgradeOptionsParent; // UI panel to parent upgrade options
    public List<Upgrade> allUpgrades; // All possible upgrades, populate in the inspector

    private List<GameObject> currentUpgradeOptions = new List<GameObject>();
    public int currentPlayerID;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void PauseGameForUpgrades(int playerID)
    {
        currentPlayerID = playerID; // Store the player ID
        Time.timeScale = 0f; // Pause the game  
        ShowUpgradeOptions(playerID);
    }

    void ShowUpgradeOptions(int playerID)
    {
        // Randomly pick 3 upgrades
        for (int i = 0; i < 3; i++)
        {
            Upgrade upgrade = allUpgrades[Random.Range(0, allUpgrades.Count)];
            GameObject option = Instantiate(upgradeUIPrefab, upgradeOptionsParent);

            // Set the UI elements based on the upgrade
            option.GetComponentInChildren<TMP_Text>().text = upgrade.name + "\n" + upgrade.description;
            option.GetComponentInChildren<Image>().sprite = upgrade.icon;

            // Add click listener to apply upgrade
            option.GetComponent<Button>().onClick.AddListener(() => ApplyUpgrade(upgrade, playerID));

            currentUpgradeOptions.Add(option);
        }
    }

    void ApplyUpgrade(Upgrade upgrade, int playerID)
    {
        // Find the player's ball and apply the upgrade
        BallController ball = FindPlayerBall(playerID); // Implement this based on your player/ball management
        if (ball != null)
        {
            switch (upgrade.effectType)
            {
                case Upgrade.EffectType.Speed:
                    ball.speed += upgrade.additiveValue;
                    // Implement multiplier logic if needed
                    break;
                case Upgrade.EffectType.Size:
                    ball.transform.localScale += Vector3.one * upgrade.additiveValue;
                    break;
                    // Handle other cases
            }
        }

        // Cleanup and resume game
        CleanupUpgradeOptions();
        Time.timeScale = 1f; // Unpause the game
    }

    void CleanupUpgradeOptions()
    {
        foreach (GameObject option in currentUpgradeOptions)
        {
            Destroy(option);
        }
        currentUpgradeOptions.Clear();
    }

    BallController FindPlayerBall(int playerID)
    {
        // Implement based on your game's logic for finding the appropriate ball
        return null;
    }

    public void ResetUpgradesAndBalls()
    {
        // Reset upgrades and ball states
        // This function needs to find all balls and reset their properties to default
        // Also consider resetting any global effects applied from upgrades
    }
}