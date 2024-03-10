using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class PlayerManager : MonoBehaviour
{
    public int xpPerBlock = 10; // XP gained per block hit
    public int baseXPForNextLevel = 100; // Base XP needed for the first level up
    public float levelMultiplier = 1.5f; // Multiplier for exponential growth

    private int xpPlayer1 = 0;
    private int levelPlayer1 = 1;
    private int xpPlayer2 = 0;
    private int levelPlayer2 = 1;

    public TextMeshProUGUI player1XPText; 
    public TextMeshProUGUI player2XPText;

    void Start()
    {
        UpdateUI();
    }

    public void AddXP(int playerNumber)
    {
        if (playerNumber == 1)
        {
            xpPlayer1 += xpPerBlock;
            while (xpPlayer1 >= XPForNextLevel(levelPlayer1))
            {
                xpPlayer1 -= XPForNextLevel(levelPlayer1);
                levelPlayer1++;
                PlayerLeveledUp(playerNumber);
            }
        }
        else if (playerNumber == 2)
        {
            xpPlayer2 += xpPerBlock;
            while (xpPlayer2 >= XPForNextLevel(levelPlayer2))
            {
                xpPlayer2 -= XPForNextLevel(levelPlayer2);
                levelPlayer2++;
                PlayerLeveledUp(playerNumber);
            }
        }

        UpdateUI();
    }

    public void PlayerLeveledUp(int playerID)
    {
        GameManager.Instance.PauseGameForUpgrades(playerID);
    }

    int XPForNextLevel(int currentLevel)
    {
        // Calculate the XP required for the next level based on the current level
        return Mathf.RoundToInt(baseXPForNextLevel * Mathf.Pow(levelMultiplier, currentLevel - 1));
    }

    void UpdateUI()
    {
        player1XPText.text = $"Lvl: {levelPlayer1} - {xpPlayer1} / {XPForNextLevel(levelPlayer1)}";
        player2XPText.text = $"Lvl: {levelPlayer2} - {xpPlayer2} / {XPForNextLevel(levelPlayer2)}";
    }
}