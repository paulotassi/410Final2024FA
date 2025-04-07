using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Score & Lives Text")]
    [SerializeField] private TMP_Text player1ScoreText;      // Displays Player 1’s score or lives
    [SerializeField] private TMP_Text player2ScoreText;      // Displays Player 2’s score or lives

    [Header("Timer & Round‑End Message")]
    [SerializeField] private TMP_Text gameTimerText;         // Displays the countdown timer
    [SerializeField] private TMP_Text roundEndText;          // Displays the end‑of‑round message

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuUI;         // The pause‑menu panel

    [Header("Cooldown Displays")]
    [SerializeField] public Image Player1shootCDDisplay;     // P1 shoot cooldown fill
    [SerializeField] public Image Player1shieldCDDisplay;    // P1 shield cooldown fill
    [SerializeField] public Image Player1StunCDDisplay;      // P1 stun cooldown fill

    [SerializeField] public Image Player2shootCDDisplay;     // P2 shoot cooldown fill
    [SerializeField] public Image Player2shieldCDDisplay;    // P2 shield cooldown fill
    [SerializeField] public Image Player2StunCDDisplay;      // P2 stun cooldown fill

    /// <summary>
    /// SetScores: update the two players’ score or life‑count text.
    /// </summary>
    public void SetScores(int p1Value, int p2Value, bool isCompetitive)
    {
        if (!isCompetitive)
        {
            player1ScoreText.text = $"Ingredients Collected: {p1Value}";
            player2ScoreText.text = $"Ingredients Collected: {p2Value}";
        }
        else
        {
            player1ScoreText.text = $"Lives Remaining: {p1Value}";
            player2ScoreText.text = $"Lives Remaining: {p2Value}";
        }
    }

    /// <summary>
    /// SetTimer: update the countdown timer display.
    /// </summary>
    public void SetTimer(string timeString)
    {
        gameTimerText.text = timeString;
    }

    /// <summary>
    /// UpdateCooldown: set a cooldown Image’s fill amount.
    /// </summary>
    public void UpdateCooldown(Image cooldownImage, float fillAmount)
    {
        cooldownImage.fillAmount = fillAmount;
    }

    /// <summary>
    /// ShowRoundEnd: display the round‑end message.
    /// </summary>
    public void ShowRoundEnd(string message)
    {
        roundEndText.text = message;
    }

    /// <summary>
    /// TogglePauseMenu: show or hide the pause menu.
    /// </summary>
    public void TogglePauseMenu(bool show)
    {
        pauseMenuUI.SetActive(show);
    }
}
