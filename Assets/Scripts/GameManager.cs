using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public float horizontalDistance = 200f; // Distance from the center for left and right options
    public float yOffset = 20f; // Vertical offset for all options
    public GameObject upgradeUIPrefab; // Assign a prefab with TMP text, image for icon, etc.
    public Transform upgradeOptionsParent; // UI panel to parent upgrade options
    public List<Upgrade> allUpgrades; // All possible upgrades, populate in the inspector

    private List<GameObject> currentUpgradeOptions = new List<GameObject>();
    public int currentPlayerID;

    public GameObject pauseOverlay;
    public TextMeshProUGUI levelUpText;

    public TextMeshProUGUI timerText; // Assign in the inspector
    private float elapsedTime = 0f;

    public List<RarityWeighting> weights1;
    public List<RarityWeighting> weights2;
    public List<RarityWeighting> weights3;
    public List<RarityWeighting> weights4;
    public List<RarityWeighting> weights5;

    public List<int> weightThresholds;

    public List<Upgrade> filteredUpgrades;
    public TextMeshProUGUI countdownText;

    public float winPercentage = 75f; // Percentage to trigger win condition
    public float countdownDuration = 5f; // Duration for the countdown
    public bool countdownActive = false;
    float currentCountdownTime;

    private int totalBlocks;
    public int player1Blocks = 0;
    public int player2Blocks = 0;
    private int currentLeadingPlayer = 0;
    public TextMeshProUGUI p1BlocksText;
    public TextMeshProUGUI p2BlocksText;
    private float previousTimeScale = 1f; // Default value is normal time scale

    public static GameManager Instance { get; private set; }
    private bool winState = false;
    private bool gamePaused = false;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        levelUpText.gameObject.SetActive(false);
        pauseOverlay.SetActive(false);
        countdownText.gameObject.SetActive(false);

        winState = false;
    }

    public void SetTotalBlocks(int count)
    {
        totalBlocks = count;
    }

    private void Update()
    {
        if (!winState && !gamePaused) {
            if (Time.timeScale > 0) // Ensuring the timer only runs when the game isn't paused
            {
                elapsedTime += Time.deltaTime;
                DisplayTime(elapsedTime);
            }

            if (!countdownActive)
            {
                // Check for both players
                for (int playerID = 1; playerID <= 2; playerID++)
                {
                    if (CalculateOwnershipPercentage(playerID) > winPercentage)
                    {
                        StartWinCountdown(playerID);
                        break; // Exit loop early if a win condition is found
                    }
                }
            }
            else
            {
                HandleCountdown();
            }

            // Update ownership UI
            UpdateOwnershipUI();
        }
    }

    void UpdateOwnershipUI()
    {
        int p1Percentage = Mathf.RoundToInt((float)player1Blocks / totalBlocks * 100);
        int p2Percentage = Mathf.RoundToInt((float)player2Blocks / totalBlocks * 100);

        p1BlocksText.text = $"Owned: {p1Percentage}%";
        p2BlocksText.text = $"Owned: {p2Percentage}%";
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void PauseGameForUpgrades(int playerID)
    {
        gamePaused = true;
        currentPlayerID = playerID; // Store the player ID
        previousTimeScale = Time.timeScale; // Save the current time scale
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

        // Determine the current phase of the game based on elapsedTime
        List<RarityWeighting> currentWeights = DetermineCurrentWeights();
        List<Upgrade> selectedUpgrades = new List<Upgrade>(); // Track selected upgrades

        for (int i = 0; i < 3; i++)
        {
            Upgrade upgrade = WeightedUpgradeSelection(currentWeights, selectedUpgrades);
            if (upgrade == null)
            {
                Debug.LogWarning("Failed to find a unique upgrade; list may be exhausted.");
                break;
            }

            selectedUpgrades.Add(upgrade); // Ensure this upgrade isn't picked again
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

    List<RarityWeighting> DetermineCurrentWeights()
    {
        if (elapsedTime <= weightThresholds[0])
        {
            return weights1;
        }
        else if (elapsedTime <= weightThresholds[1])
        {
            return weights2;
        }
        else if (elapsedTime <= weightThresholds[2])
        {
            return weights3;
        }
        else if (elapsedTime <= weightThresholds[3])
        {
            return weights4;
        }
        else if (elapsedTime > weightThresholds[3])
        {
            return weights5;
        }

        return null; // Default, consider handling this case
    }

    Upgrade WeightedUpgradeSelection(List<RarityWeighting> weights, List<Upgrade> excludeList)
    {
        // Calculate total weight
        float totalWeight = weights.Sum(weight => weight.weight);
        // Randomly select a point within this total weight
        float randomPoint = Random.Range(0, totalWeight);
        float currentSum = 0;
        Upgrade.Rarity selectedRarity = Upgrade.Rarity.Common; // Default if not found

        // Determine selected rarity based on weighted random selection
        foreach (var weight in weights)
        {
            currentSum += weight.weight;
            if (currentSum >= randomPoint)
            {
                selectedRarity = weight.rarity;
                break;
            }
        }

        // Filter allUpgrades based on the selected rarity and exclude those already selected
        List<Upgrade> availableUpgrades = allUpgrades.Where(upgrade => upgrade.rarity == selectedRarity && !excludeList.Contains(upgrade)).ToList();

        if (availableUpgrades.Count > 0)
        {
            return availableUpgrades[Random.Range(0, availableUpgrades.Count)]; // Select randomly from available upgrades
        }
        else
        {
            Debug.LogWarning("No available upgrades found for the selected rarity and exclude list.");
            return null; // or handle as needed
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
        Time.timeScale = previousTimeScale;
        gamePaused = false;
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

    public void ChangeBlockOwnership(int oldOwner, int newOwner)
    {
        // Ensure thread-safe update to block counters
        lock (this)
        {
            if (oldOwner == 1)
            {
                player1Blocks--;
            }
            else if (oldOwner == 2)
            {
                player2Blocks--;
            }

            if (newOwner == 1)
            {
                player1Blocks++;
            }
            else if (newOwner == 2)
            {
                player2Blocks++;
            }
        }
    }

    float CalculateOwnershipPercentage(int playerID)
    {
        int ownedBlocks = playerID == 1 ? player1Blocks : player2Blocks;
        return (float)ownedBlocks / totalBlocks * 100f;
    }

    public void StartWinCountdown(int winningPlayer)
    {
        countdownActive = true;
        currentLeadingPlayer = winningPlayer; // Store the current leading player
        currentCountdownTime = countdownDuration;
        countdownText.gameObject.SetActive(true);
    }

    public void HandleCountdown()
    {
        // Check if the current leading player still meets the win condition
        if (CalculateOwnershipPercentage(currentLeadingPlayer) < winPercentage)
        {
            ResetWinCondition();
            return;
        }

        currentCountdownTime -= Time.deltaTime;
        float lerpFactor = 1f - (currentCountdownTime / countdownDuration);
        Time.timeScale = Mathf.Lerp(1f, 0.2f, lerpFactor);
        countdownText.text = Mathf.Ceil(currentCountdownTime).ToString();


        if (currentCountdownTime <= 0)
        {
            WinGame(currentLeadingPlayer); // Pass the winning player ID
        }
    }

    void ResetWinCondition()
    {
        countdownActive = false;
        countdownText.gameObject.SetActive(false);
        Time.timeScale = 1f; // Reset game speed to normal
        currentLeadingPlayer = 0; // Reset leading player
    }

    void WinGame(int playerID)
    {
        winState = true;
        countdownText.text = $"PLAYER {playerID} WINS!";
        Time.timeScale = 0f; // Further slow down game speed, if desired
        currentLeadingPlayer = 0; // Reset leading player after declaring a winner
    }


    public void ResetUpgradesAndBalls()
    {
        // Reset upgrades and ball states
        // This function needs to find all balls and reset their properties to default
        // Also consider resetting any global effects applied from upgrades
    }
}

[System.Serializable]
public class RarityWeighting
{
    public Upgrade.Rarity rarity;
    public float weight;
}
