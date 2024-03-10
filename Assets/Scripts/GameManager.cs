using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float horizontalDistance = 200f; // Distance from the center for left and right options
    public float yOffset = 20f; // Vertical offset for all options
    public GameObject upgradeUIPrefab; // Assign a prefab with TMP text, image for icon, etc.
    public Transform upgradeOptionsParent; // UI panel to parent upgrade options
    public List<Upgrade> allUpgrades; // All possible upgrades, populate in the inspector

    private List<GameObject> currentUpgradeOptions = new List<GameObject>();
    public int currentPlayerID;

    public GameObject pauseOverlay; 
    public TextMeshProUGUI levelUpText; 

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        levelUpText.gameObject.SetActive(false);
        pauseOverlay.SetActive(false);
    }

    public void PauseGameForUpgrades(int playerID)
    {
        currentPlayerID = playerID; // Store the player ID
        Time.timeScale = 0f; // Pause the game  
        pauseOverlay.SetActive(true); // Show pause overlay
        levelUpText.gameObject.SetActive(true);
        ShowLevelUpMessage(playerID);
        ShowUpgradeOptions(playerID);
    }

    void ShowUpgradeOptions(int playerID)
    {
        // Define positions for the upgrades
        Vector2 centerPosition = new Vector2(0, yOffset); // Apply yOffset to center
        Vector2[] positions = new Vector2[]
        {
            new Vector2(-horizontalDistance, yOffset), // Left with yOffset
            centerPosition, // Center already includes yOffset
            new Vector2(horizontalDistance, yOffset) // Right with yOffset
        };

        for (int i = 0; i < 3; i++)
        {
            Upgrade upgrade = allUpgrades[Random.Range(0, allUpgrades.Count)];
            GameObject option = Instantiate(upgradeUIPrefab, upgradeOptionsParent);

            // Assuming the option GameObject uses a RectTransform
            RectTransform rectTransform = option.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = positions[i];
            }
            else
            {
                Debug.LogError("RectTransform not found on the instantiated upgrade option.");
            }

            // Find and set the Name text
            TMP_Text nameText = option.transform.Find("Name").GetComponent<TMP_Text>();
            if (nameText != null) nameText.text = upgrade.name;

            // Find and set the Rarity text
            TMP_Text rarityText = option.transform.Find("Rarity").GetComponent<TMP_Text>();
            if (rarityText != null) rarityText.text = upgrade.rarity.ToString();

            // Find and set the Description text
            TMP_Text descriptionText = option.transform.Find("Description").GetComponent<TMP_Text>();
            if (descriptionText != null) descriptionText.text = upgrade.description;

            // Find and set the Icon image
            Image iconImage = option.transform.Find("Icon").GetComponent<Image>();
            if (iconImage != null) iconImage.sprite = upgrade.icon;

            Button optionButton = option.GetComponent<Button>();
            if (optionButton != null)
            {
                Upgrade localUpgrade = upgrade; // Local copy for delegate capture
                optionButton.onClick.AddListener(delegate { ApplyUpgrade(localUpgrade, currentPlayerID); });
            }

            currentUpgradeOptions.Add(option);
        }

    }

    void ApplyUpgrade(Upgrade upgrade, int playerID)
    {
        // Find all BallController instances in the scene
        BallController[] allBalls = FindObjectsOfType<BallController>();

        // Iterate through all balls to find those with the matching playerID
        foreach (BallController ball in allBalls)
        {
            if (ball.playerID == playerID)
            {
                // Apply each effect in the upgrade to the matching ball
                foreach (var effect in upgrade.effects)
                {
                    switch (effect.effectType)
                    {
                        case Upgrade.EffectType.Speed:
                            ball.baseSpeed += effect.additiveValue;
                            ball.speedMult += effect.multiplierValue;
                            break;
                        case Upgrade.EffectType.Size:
                            ball.baseSize += effect.additiveValue;
                            ball.sizeMult += effect.multiplierValue;
                            break;
                        case Upgrade.EffectType.Pierce:
                            int amountToPierce = Mathf.RoundToInt(effect.additiveValue);
                            ball.pierce += amountToPierce;
                            break;
                        case Upgrade.EffectType.Split:
                            int numberOfBallsToSplit = Mathf.RoundToInt(effect.additiveValue);
                            for (int i = 0; i < numberOfBallsToSplit; i++)
                            {
                                Instantiate(ball.gameObject, ball.transform.position, ball.transform.rotation);
                            }
                            break;
                        case Upgrade.EffectType.Boost:
                            ball.boostMult += effect.multiplierValue;
                            ball.boostDuration += effect.additiveValue;
                            break;
                        case Upgrade.EffectType.Charge:
                            ball.chargeRate -= effect.additiveValue;
                            ball.chargeMult += effect.multiplierValue;
                            break;
                        case Upgrade.EffectType.BounceAngle:
                            // Ensure your BallController script can handle this property
                            break;
                            // Add cases for other effects as needed
                    }
                }
                // Call any recalculations or refresh methods needed after applying upgrades
                ball.CalculateStats();
            }
        }

        ResumeGame();
    }


    public void ResumeGame()
    {
        levelUpText.gameObject.SetActive(false);
        pauseOverlay.SetActive(false); // Hide pause overlay
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

    public void ShowLevelUpMessage(int playerID)
    {
        levelUpText.gameObject.SetActive(true);
        string playerName = playerID == 1 ? "PLAYER 1" : "PLAYER 2";
        levelUpText.text = $"{playerName} UPGRADE";
        levelUpText.color = playerID == 1 ? Color.cyan : Color.yellow; 
    }

    public void ResetUpgradesAndBalls()
    {
        // Reset upgrades and ball states
        // This function needs to find all balls and reset their properties to default
        // Also consider resetting any global effects applied from upgrades
    }
}