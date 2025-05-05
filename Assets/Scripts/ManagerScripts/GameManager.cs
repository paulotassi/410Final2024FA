using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIManager uiManager;            // Handles every text/image update

    [Header("General Settings")]
    [SerializeField] public int playerStartingLifeCount = 3;
    [SerializeField] public bool isPaused = false;
    [SerializeField] public bool singlePlayerMode = false;
    [SerializeField] public bool competetiveMode = false;
    [SerializeField] public bool arcadeMode = false;

    [Header("Single‑Player UI Tweaks")]
    [SerializeField] public RectTransform sourceUI;
    [SerializeField] public RectTransform targetUI;
    [SerializeField] public GameObject p1HealthObject;
    [SerializeField] public GameObject p2HealthObject;
    [SerializeField] public GameObject p1Indicator;
    [SerializeField] public GameObject p2Indicator;
    [SerializeField] public GameObject splitBarObject;
    [SerializeField] public GameObject p1Camera;
    [SerializeField] public Camera p2Camera;
    [SerializeField] public GameObject p1BossIndicator;

    //======================================================
    // Core Game State
    //======================================================
    [Header("Score, GameState & Timer")]
    [SerializeField] public int player1IngredientCount = 0;
    [SerializeField] public int player2IngredientCount = 0;
    private int totalScore;
    [SerializeField] public float remainingTime = 30f;
    private bool winStateMet = false;


    //======================================================
    // Player & Boss References
    //======================================================
    [Header("Player & Boss References")]
    [SerializeField] public GameObject player1GameObject;
    [SerializeField] public GameObject player2GameObject;
    private CapsuleCollider2D playerCollider1;
    private CapsuleCollider2D playerCollider2;
    private int playerLayer1;
    private int playerLayer2;
    private BossHP bossHp;

    private PlayerController player1Controller;
    private PlayerController player2Controller;
    private PlayerHealth player1Health;
    private PlayerHealth player2Health;

    //======================================================
    // Ingredient Tracking
    //======================================================
    private Dictionary<IngredientType, int> player1Ingredients = new Dictionary<IngredientType, int>();
    private Dictionary<IngredientType, int> player2Ingredients = new Dictionary<IngredientType, int>();

    //======================================================
    // Cooldown Timers
    //======================================================
    private float p1ShootCdMax, p1ShootCdTimer;
    private bool p1ShootCdEnabled;
    private float p1ShieldCdMax, p1ShieldCdTimer;
    private bool p1ShieldCdEnabled;
    private float p1StunCdMax, p1StunCdTimer;
    private bool p1StunCdEnabled;

    private float p2ShootCdMax, p2ShootCdTimer;
    private bool p2ShootCdEnabled;
    private float p2ShieldCdMax, p2ShieldCdTimer;
    private bool p2ShieldCdEnabled;
    private float p2StunCdMax, p2StunCdTimer;
    private bool p2StunCdEnabled;

    //======================================================
    // Initialization
    //======================================================
    


    private void Start()
    {
        singlePlayerMode = GameSettings.singlePlayerMode;
        competetiveMode = GameSettings.competetiveMode;
        arcadeMode = GameSettings.arcadeMode;

        // Find players by tag
        player1GameObject = GameObject.FindWithTag("Player");
        player2GameObject = GameObject.FindWithTag("Player2");

        // Single‑player layout adjustments
        if (singlePlayerMode)
        {
            CopyRectTransform(sourceUI, targetUI);
            Debug.Log("Single Player Mode Active");
            p1Camera.SetActive(false);
            p2Camera.rect = new Rect(0f, 0f, 1f, 1f);
            player1GameObject.SetActive(false);
            p1Indicator.SetActive(false);
            p2Indicator.SetActive(false);
            splitBarObject.SetActive(false);
            p1HealthObject.SetActive(false);
            p1BossIndicator.SetActive(false);
        }

        // Cache components
        player1Controller = player1GameObject.GetComponent<PlayerController>();
        player2Controller = player2GameObject.GetComponent<PlayerController>();
        player1Health = player1GameObject.GetComponent<PlayerHealth>();
        player2Health = player2GameObject.GetComponent<PlayerHealth>();

        playerCollider1 = player1GameObject.GetComponent<CapsuleCollider2D>();
        playerLayer1 = playerCollider1.gameObject.layer;
        playerCollider2 = player2GameObject.GetComponent<CapsuleCollider2D>();
        playerLayer2 = playerCollider2.gameObject.layer;

        bossHp = FindFirstObjectByType<BossHP>();

        InitializeCooldowns();
    }



    // Copy all RectTransform properties
    private void CopyRectTransform(RectTransform source, RectTransform target)
    {
        target.position = source.position;
        target.rotation = source.rotation;
        target.localScale = source.localScale;
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.anchoredPosition = source.anchoredPosition;
        target.sizeDelta = source.sizeDelta;
        target.pivot = source.pivot;
    }

    // Set initial cooldown values from PlayerController
    private void InitializeCooldowns()
    {
        p1ShootCdMax = player1Controller.shootCoolDown;
        p1ShootCdTimer = p1ShootCdMax;
        p1ShieldCdMax = player1Controller.shieldCooldown + player1Controller.shieldDuration;
        p1ShieldCdTimer = p1ShieldCdMax;
        p1StunCdMax = player1Controller.altShootCoolDown;
        p1StunCdTimer = p1StunCdMax;

        p2ShootCdMax = player2Controller.shootCoolDown;
        p2ShootCdTimer = p2ShootCdMax;
        p2ShieldCdMax = player2Controller.shieldCooldown + player2Controller.shieldDuration;
        p2ShieldCdTimer = p2ShieldCdMax;
        p2StunCdMax = player2Controller.altShootCoolDown;
        p2StunCdTimer = p2StunCdMax;
    }

    //======================================================
    // Main Loop
    //======================================================
    private void Update()
    {
        HandleModeAndScores();
        HandleCooldowns();
        HandleTimer();
        CheckEndConditions();
    }

    // Toggle physics layers and update score/lives text
    private void HandleModeAndScores()
    {
        if (!competetiveMode)
        {
            Physics2D.IgnoreLayerCollision(playerLayer1, playerLayer2, true);
            Physics2D.IgnoreLayerCollision(playerLayer2, playerLayer1, true);
            Physics2D.IgnoreLayerCollision(playerLayer1, playerLayer1, true);
            Physics2D.IgnoreLayerCollision(playerLayer2, playerLayer2, true);

            uiManager.SetScores(player1IngredientCount, player2IngredientCount, false);
        }
        else
        {
            Physics2D.IgnoreLayerCollision(playerLayer1, playerLayer2, false);
            Physics2D.IgnoreLayerCollision(playerLayer2, playerLayer1, false);
            Physics2D.IgnoreLayerCollision(playerLayer1, playerLayer1, false);
            Physics2D.IgnoreLayerCollision(playerLayer2, playerLayer2, false);

            uiManager.SetScores(player1Health.playerLifeCountRemaining,
                                player2Health.playerLifeCountRemaining,
                                true);
        }
    }

    // Advance each player’s cooldown and notify UIManager
    private void HandleCooldowns()
    {

        if (!singlePlayerMode)
        {
            UpdateCooldown(ref p1ShootCdTimer, p1ShootCdMax, player1Controller.fired, ref p1ShootCdEnabled, player1ShootCdTrigger, uiManager.Player1shootCDDisplay);
            UpdateCooldown(ref p1ShieldCdTimer, p1ShieldCdMax, player1Controller.shielded, ref p1ShieldCdEnabled, player1ShieldCdTrigger, uiManager.Player1shieldCDDisplay);
            UpdateCooldown(ref p1StunCdTimer, p1StunCdMax, player1Controller.altFired, ref p1StunCdEnabled, player1StunCdTrigger, uiManager.Player1StunCDDisplay);

            UpdateCooldown(ref p2ShootCdTimer, p2ShootCdMax, player2Controller.fired, ref p2ShootCdEnabled, player2ShootCdTrigger, uiManager.Player2shootCDDisplay);
            UpdateCooldown(ref p2ShieldCdTimer, p2ShieldCdMax, player2Controller.shielded, ref p2ShieldCdEnabled, player2ShieldCdTrigger, uiManager.Player2shieldCDDisplay);
            UpdateCooldown(ref p2StunCdTimer, p2StunCdMax, player2Controller.altFired, ref p2StunCdEnabled, player2StunCdTrigger, uiManager.Player2StunCDDisplay);
        }
        else
        {
            UpdateCooldown(ref p2ShootCdTimer, p2ShootCdMax, player2Controller.fired, ref p2ShootCdEnabled, player2ShootCdTrigger, uiManager.Player2shootCDDisplay);
            UpdateCooldown(ref p2ShieldCdTimer, p2ShieldCdMax, player2Controller.shielded, ref p2ShieldCdEnabled, player2ShieldCdTrigger, uiManager.Player2shieldCDDisplay);
            UpdateCooldown(ref p2StunCdTimer, p2StunCdMax, player2Controller.altFired, ref p2StunCdEnabled, player2StunCdTrigger, uiManager.Player2StunCDDisplay);
        }
    }

    // Generic cooldown helper
    private void UpdateCooldown(ref float timer,
                                float maxValue,
                                bool triggered,
                                ref bool cooldownEnabled,
                                System.Func<IEnumerator> coroutineFunction,
                                Image display)
    {
        if (triggered && !cooldownEnabled)
        {
            StartCoroutine(coroutineFunction());
        }

        float fillAmount = timer / maxValue;   // compute fill
        uiManager.UpdateCooldown(display, fillAmount);
        timer += Time.deltaTime;
    }

    // Decrement game timer and update UI
    private void HandleTimer()
    {
        remainingTime -= Time.deltaTime;

        int minutes = Mathf.FloorToInt(remainingTime / 60);   // local var
        int seconds = Mathf.FloorToInt(remainingTime % 60);   // local var

        uiManager.SetTimer(string.Format("{0:00}:{1:00}", minutes, seconds));
    }

    // Check for end‑of‑round and trigger sequence
    private void CheckEndConditions()
    {
        if (remainingTime <= 0 ||
            (bossHp != null && bossHp.bossDead) ||
            player1Health.playerLifeCountRemaining == 0 ||
            player2Health.playerLifeCountRemaining == 0)
        {
            uiManager.SetTimer("00:00");
            string msg = ComputeGameStateMessage();
            uiManager.ShowRoundEnd(msg);
            StartCoroutine(GameEnd());
        }
    }

    // Decide which win/lose message to show
    private string ComputeGameStateMessage()
    {
        if (competetiveMode && (player1Health.playerLifeCountRemaining > player2Health.playerLifeCountRemaining))
        {
            return "Player 1 Win";
        }
        else if (competetiveMode && (player1Health.playerLifeCountRemaining < player2Health.playerLifeCountRemaining))
        {
            return "Player 2 Win";
        }
        else if (bossHp != null && bossHp.bossDead)
        {
            return "Some Bad Witches Killed the Boss!";
        }
        else if (!competetiveMode && winStateMet)
        {
            return "Witches have collected " + (player1IngredientCount + player2IngredientCount) + " Ingredients for the brews.";
        }
        else
        {
            return "Weak Witches";
        }
    }

    // Called by trigger when players enter the end zone
    public void EndZoneEntry(int roundRequiredScore)
    {
        if (player1IngredientCount + player2IngredientCount < roundRequiredScore)
        {
            winStateMet = false;
            uiManager.ShowRoundEnd("Not Enough Ingredients");
            return;
        }

        remainingTime = 5f;
        winStateMet = true;

        string msg = ComputeGameStateMessage();
        uiManager.ShowRoundEnd(msg);
    }

    //======================================================
    // Cooldown Coroutines
    //======================================================
    private IEnumerator player1ShootCdTrigger()
    {
        p1ShootCdEnabled = true;
        p1ShootCdTimer = 0f;
        yield return new WaitForSeconds(p1ShootCdMax);
        p1ShootCdEnabled = false;
    }

    private IEnumerator player1ShieldCdTrigger()
    {
        p1ShieldCdEnabled = true;
        p1ShieldCdTimer = 0f;
        yield return new WaitForSeconds(p1ShieldCdMax);
        p1ShieldCdEnabled = false;
    }

    private IEnumerator player1StunCdTrigger()
    {
        p1StunCdEnabled = true;
        p1StunCdTimer = 0f;
        yield return new WaitForSeconds(p1StunCdMax);
        p1StunCdEnabled = false;
    }

    private IEnumerator player2ShootCdTrigger()
    {
        p2ShootCdEnabled = true;
        p2ShootCdTimer = 0f;
        yield return new WaitForSeconds(p2ShootCdMax);
        p2ShootCdEnabled = false;
    }

    private IEnumerator player2ShieldCdTrigger()
    {
        p2ShieldCdEnabled = true;
        p2ShieldCdTimer = 0f;
        yield return new WaitForSeconds(p2ShieldCdMax);
        p2ShieldCdEnabled = false;
    }

    private IEnumerator player2StunCdTrigger()
    {
        p2StunCdEnabled = true;
        p2StunCdTimer = 0f;
        yield return new WaitForSeconds(p2StunCdMax);
        p2StunCdEnabled = false;
    }

    //======================================================
    // End‑Game & Scene Management
    //======================================================
    private IEnumerator GameEnd()
    {
        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene("TitleScreen");
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        uiManager.TogglePauseMenu(isPaused);
        
    }



    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        uiManager.TogglePauseMenu(false);
    }

    public void ExitGame()
    {
        ResumeGame();
        SceneManager.LoadScene("TitleScreen");
    }

    public void IncreaseGameTime(float increaseAmount)
    {
        remainingTime += increaseAmount;
    }

    //======================================================
    // Ingredient Handling & Buffs
    //======================================================
    public void Player1IncreaseIngredient(IngredientType type, GameObject player)
    {
        if (!player1Ingredients.ContainsKey(type))
            player1Ingredients[type] = 0;

        player1Ingredients[type]++;
        player1IngredientCount++;

        if (player1Ingredients[type] >= 5)
        {
            ApplyBuff(player, type);
            player1Ingredients[type] = 0;
        }
    }

    public void Player1DecreaseIngredient()
    {
        if (player1Ingredients.Count > 0)
            RemoveRandomIngredient(player1Ingredients);
    }

    public void Player2IncreaseIngredient(IngredientType type, GameObject player)
    {
        if (!player2Ingredients.ContainsKey(type))
            player2Ingredients[type] = 0;

        player2Ingredients[type]++;
        player2IngredientCount++;

        if (player2Ingredients[type] >= 5)
        {
            ApplyBuff(player, type);
            player2Ingredients[type] = 0;
        }
    }

    public void Player2DecreaseIngredient()
    {
        if (player2Ingredients.Count > 0)
            RemoveRandomIngredient(player2Ingredients);
    }

    private void RemoveRandomIngredient(Dictionary<IngredientType, int> ingredients)
    {
        List<IngredientType> keys = new List<IngredientType>(ingredients.Keys);
        IngredientType randomType = keys[Random.Range(0, keys.Count)];
        ingredients[randomType] -= keys.Count / 2;

        if (ingredients[randomType] <= 0)
            ingredients.Remove(randomType);
    }

    private void ApplyBuff(GameObject player, IngredientType type)
    {
        PlayerController controller = player.GetComponent<PlayerController>();

        if (controller == null)
            return;

        BuffType buffToApply;

        switch (type)
        {
            case IngredientType.Herb:
                if (controller.SpeedBuff) return;
                buffToApply = BuffType.SpeedBoost;
                controller.SpeedBuff = true;
                break;

            case IngredientType.Finger:
                if (controller.ShootBuff) return;
                buffToApply = BuffType.FireRateIncrease;
                controller.ShootBuff = true;
                break;

            case IngredientType.FrogLeg:
                if (controller.StunBuff) return;
                buffToApply = BuffType.StunMultiplier;
                controller.StunBuff = true;
                break;

            case IngredientType.Spider:
                if (controller.ShieldBuff) return;
                buffToApply = BuffType.ShieldExtension;
                controller.ShieldBuff = true;
                break;

            default:
                return;
        }

        controller.ApplyBuff(buffToApply);
        InitializeCooldowns();
    }
}
