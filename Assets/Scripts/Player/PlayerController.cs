using Cinemachine;
using UnityEngine;
using System.Collections;
using UnityEngine.U2D.Animation; // For sprite animation, unused in this script but necessary for Player Animation
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private enum State { Idle, Walking, Flying }
    private State currentState = State.Idle;

    // Player movement variables
    [Header("Movement Settings" +
        "")]
    public Vector2 movementInput = Vector2.zero; //input vector

    public float initialMoveSpeed = 1f; // Initial movement speed before any acceleration
    public float moveSpeed = 5f; // Current movement speed (changes with input)
    public float moveHorizontalFlightSpeed = 1f; // Speed when moving horizontally during flight
    public float flightSpeed = 1f; // Vertical flight speed
    public float topSpeed = 100f; // Maximum allowed speed
    public float jumpForce = 10f; // Force applied when jumping
    public float transitionThreshold = 50f; // Speed threshold for flight mode transition
    public float liftChangeRate = 0.1f; // Rate at which gravity changes when transitioning between flight and grounded states
    public float fallRate = 3f; // Rate at which gravity changes when transitioning from flight to grounded states
    public float liftForce = 0f;
    public float flightThreshold = 5f;
    public float inactivityTime = 0f; // Time player has been inactive
    public float inactivityThreshold = 2f; // Time threshold for considering inactivity (in seconds)
    private bool isStunned = false;
    public bool stunnable = true;
    private bool slowed = false;
    
    //Player Stun Variables
    public float stunDR = 3.5f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip shootSFX;
    [SerializeField] private AudioClip altShootSFX;
    [SerializeField] private AudioClip shieldSFX;
    [SerializeField] private AudioClip stunSFX;
    [SerializeField] private AudioClip jumpSFX;
    private AudioSource audioSource;


    //Player Shoot variables
    [Header("Shoot Settings" +
        "")]
    public Vector2 aimInput = Vector2.zero; //input vector
    public bool canShoot = true; //Shooting bool
    public bool canAltShoot = true; //Shooting bool
    public float shootCoolDown; //How long until players can shoot again
    public float altShootCoolDown; //How long until players can shoot again
    public GameObject projectilePrefab; //The projectile prefab holding the motion for projectiles
    public GameObject altProjectilePrefab; //The projectile prefab holding the motion for projectiles
    public GameObject altProjectileBuffPrefab;
    public GameObject projectileSpawnLocation; //spawnLocation of the rotating familiar
    public GameObject projectileSpawnRotation; //spawn rotation to follow the Familiar direction
    public GameObject flightParticles;


    //Player Shield
    [Header("Shield Settings" +
        "")]
    public bool isShielded = false;
    public bool canShield = true;
    public float shieldCooldown;
    public float shieldDuration;

    // Player Actions
    private bool jumped = false;
    public bool fired = false;
    public bool shielded = false;
    public bool altFired = false;
    public bool paused = false;
    public bool backShoot = false;


    // Ground check variables
    public Transform groundCheck; // Point used to detect if the player is grounded
    public LayerMask groundLayer; // Layer that represents the ground
    public float groundCheckRadius = 0.2f; // Radius of the overlap circle for ground detection
    public Vector3 groundCheckYOffset; // Offset for ground check position

    // Internal state variables
    public Rigidbody2D rb; // Reference to the player's Rigidbody2D component
    public bool isGrounded; // Whether the player is currently grounded
    public float horizontalInput; // Horizontal input from the player
    public float verticalInput; // Vertical input from the player (used in flight mode)
    public bool flightMode = false; // Tracks if the player is in flight mode
    private bool isFalling = false;
    public bool isPaused = false;
    public GameManager gameManager;
    [SerializeField] public bool StunBuff = false;
    [SerializeField] public bool ShootBuff = false;
    [SerializeField] public bool ShieldBuff = false;
    [SerializeField] public bool SpeedBuff = false;


    // Animation variables
    public Animator animator; // Reference to the Animator for controlling animations

    //PlayerCam
    [Header("Camera Settings" +
        "")]
    public CinemachineVirtualCamera virtualCameraLeft;
    public CinemachineVirtualCamera virtualCameraRight;
    [SerializeField] protected CinemachineBasicMultiChannelPerlin rightNoise;
    [SerializeField] protected CinemachineBasicMultiChannelPerlin leftNoise;
    [SerializeField] protected float screenShakeValue = 1f;
    [SerializeField] protected float screenShakeDuration = 0.5f;


    // Start is called before the first frame update
    void Start()
    {
        // Get components on start
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>(); // Retrieve Rigidbody2D component for physics
        animator = GetComponent<Animator>(); // Retrieve Animator component for animations
        leftNoise = virtualCameraLeft.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        rightNoise = virtualCameraRight.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        gameManager = FindFirstObjectByType<GameManager>();
       if (gameManager.arcadeMode)
        {
            if (this.gameObject.CompareTag("Player2"))
            {
                gameObject.GetComponent<PlayerInput>().SwitchCurrentControlScheme("Arcade", Keyboard.current);
            }
            else
            {
                gameObject.GetComponent<PlayerInput>().SwitchCurrentControlScheme("P2Arcade", Keyboard.current);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
 
        // Capture player input
        horizontalInput = movementInput.x;
        verticalInput = movementInput.y;

        // Check if player is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position - groundCheckYOffset, groundCheckRadius, groundLayer);

        // Update animator parameters
        animator.SetFloat("WalkSpeed", moveSpeed);
        animator.SetFloat("FlightY", verticalInput);
        animator.SetBool("Flying", flightMode);
        animator.SetBool("Falling", isFalling);
        animator.SetBool("Grounded", isGrounded);

        // Handle sprite flipping and camera priority
        HandleSpriteFlipAndCamera();

        // Handle player actions
        if (jumped && isGrounded && !isStunned) Jump();
        if (fired && canShoot && !isStunned) StartCoroutine(Shoot());
        if (backShoot && canShoot && !isStunned) StartCoroutine(BackShoot());
        if (altFired && canAltShoot && !isStunned) StartCoroutine(AltShoot());
        if (shielded && canShield && !isStunned) StartCoroutine(Shield());
        if (paused) gameManager.TogglePause();

        // Reset move speed if no horizontal input
        moveSpeed = (horizontalInput == 0) ? initialMoveSpeed : moveSpeed;

        //swaps flightmode
        if (!isFalling && rb.linearVelocity.y <= 0 && verticalInput != 0)
        {
            EnterFlightMode();
        }

        // Handle state transitions
        UpdatePlayerState();
        ParticleSystem flightParticleSystem = flightParticles.GetComponent<ParticleSystem>();
        var emission = flightParticleSystem.emission; // Get the emission module
        emission.enabled = flightMode; // Set emission based on flightMode

        // Track player inactivity
        TrackInactivity();
    }

    void EnterFlightMode()
    {
        flightMode = true;
        currentState = State.Flying;
        isFalling = false; // Reset falling state
        
    }

    void HandleSpriteFlipAndCamera()
    {
        if (horizontalInput < 0)
        {
            //GetComponent<SpriteRenderer>().flipX = true;
            transform.eulerAngles = new Vector3(0, 180, 0);
            virtualCameraLeft.Priority = 10;
            virtualCameraRight.Priority = 9;
        }
        else if (horizontalInput > 0)
        {
            //GetComponent<SpriteRenderer>().flipX = false;
            transform.eulerAngles = new Vector3(0, 0, 0);
            virtualCameraLeft.Priority = 9;
            virtualCameraRight.Priority = 10;
        }
    }

    void UpdatePlayerState()
    {
        bool isMovingFast = Mathf.Abs(rb.linearVelocity.x) >= transitionThreshold;

        if (!isGrounded && isMovingFast)
        {
            flightMode = true;
            currentState = State.Flying;
        }
        else if (isGrounded)
        {
            flightMode = false;
            currentState = (horizontalInput != 0) ? State.Walking : State.Idle;
        }
    }

    public void ApplyBuff(BuffType buffType)
    {
        switch (buffType)
        {
            case BuffType.SpeedBoost:
                topSpeed *= 1.5f;
                moveHorizontalFlightSpeed *= 1.5f;
                flightSpeed *= 1.5f;// Increase speed by 50%
                Debug.Log("Speed Boost Applied!");
                break;

            case BuffType.FireRateIncrease:
                shootCoolDown /= 2; // Decrease fire rate cooldown by 25% (higher fire rate)
                Debug.Log("Fire Rate Increased!");
                break;

            case BuffType.ShieldExtension:
                shieldDuration += 2f; // Extend shield duration by 2 seconds
                Debug.Log("Shield Duration Extended!");
                break;
            case BuffType.StunMultiplier:
                altProjectilePrefab = altProjectileBuffPrefab; // Extend shield duration by 2 seconds
                Debug.Log("Stun size increased!!");
                break;
        }
    }

    void TrackInactivity()
    {
        inactivityTime = (horizontalInput == 0 && verticalInput == 0) ? inactivityTime + Time.deltaTime : 0f;

        if (inactivityTime > inactivityThreshold && currentState == State.Flying || isStunned)
        {
            fallRate = Mathf.Clamp(fallRate + (liftChangeRate / 3), 0, 15f);
            isFalling = true;
            flightMode = false ;
        }
        else
        {
            fallRate = 0f;
            isFalling = false;

        }
    }


    // FixedUpdate is called at fixed intervals, used for physics-based calculations
    void FixedUpdate()
    {
        if (isStunned) return;
        Move();

        // Reset move speed to initial value when no horizontal input
        if (horizontalInput == 0)
        {
            moveSpeed = initialMoveSpeed;
        }
        // Accelerate the player until they reach top speed
        if (moveSpeed <= topSpeed)
        {
            moveSpeed += 0.5f; // Increase speed gradually
        }
        else if (moveSpeed >= topSpeed)
        {
            moveSpeed = topSpeed;//Caps moveSpeed
        }
    }

    // Move the player horizontally or in flight mode
    void Move()
    {

        if (currentState == State.Flying)
        {
            // In flight mode, move based on both horizontal and vertical input
            rb.linearVelocity = new Vector2(horizontalInput * moveHorizontalFlightSpeed, verticalInput * flightSpeed - fallRate);
        }
        else
        {
            // On the ground, move only horizontally
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    // Handle the jump action by applying a vertical force
    void Jump()
    {
        isFalling = true;
        
        rb.AddForce(new Vector2(rb.linearVelocity.x, jumpForce), ForceMode2D.Impulse); // Apply jump force
        
    }

    private void PlaySound(AudioClip clip, bool randomizePitch = false)
    {
        if (audioSource == null || clip == null) return;

        audioSource.pitch = randomizePitch ? Random.Range(0.9f, 1.2f) : 1f;
        audioSource.PlayOneShot(clip);
    }


    private IEnumerator Shoot()
    {
        canShoot = false;
        Instantiate(projectilePrefab, projectileSpawnLocation.transform.position , projectileSpawnRotation.transform.rotation);
        PlaySound(shootSFX, true); // Random pitch for shoot

        StartCoroutine(createScreenShake(2));
        yield return new WaitForSeconds(shootCoolDown);
        canShoot = true;
    }

    private IEnumerator BackShoot()
    {
        canShoot = false;
        Instantiate(projectilePrefab, projectileSpawnLocation.transform.position, projectileSpawnRotation.transform.rotation);
        PlaySound(shootSFX, true); // Random pitch for shoot

        StartCoroutine(createScreenShake(2));
        yield return new WaitForSeconds(shootCoolDown);
        canShoot = true;
    }

    private IEnumerator AltShoot()
    {
        canAltShoot = false;
        Instantiate(altProjectilePrefab, projectileSpawnLocation.transform.position, projectileSpawnRotation.transform.rotation);
        PlaySound(altShootSFX, true);
        StartCoroutine(createScreenShake(2));
        yield return new WaitForSeconds(altShootCoolDown);
        canAltShoot = true;
    }

    private IEnumerator Shield()
    {
        canShield = false;
        isShielded = true;
        this.gameObject.GetComponent<PlayerHealth>().isInvincible = true;
        PlaySound(shieldSFX, true);

        yield return new WaitForSeconds(shieldDuration);
        isShielded = false;
        this.gameObject.GetComponent<PlayerHealth>().isInvincible = false;

        yield return new WaitForSeconds(shieldCooldown);
        canShield = true;
    }

    public virtual IEnumerator Stunned(float stunDuration)
    {
        isStunned = true;
        stunnable = false;
        isFalling = true;
        PlaySound(stunSFX, true);
        Debug.Log(this.gameObject.name + " is Stunned for " + stunDuration);
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
        isFalling = false;
        Debug.Log(this.gameObject.name + "no longer Stunned. Cannot be stunned for " + (stunDuration * stunDR));
        yield return new WaitForSeconds(stunDuration * stunDR);
        stunnable = true;
    }

    public IEnumerator createScreenShake(float screenShakeIntensity)
    {

  
        leftNoise.m_AmplitudeGain = screenShakeIntensity;
        leftNoise.m_FrequencyGain = screenShakeIntensity;
        rightNoise.m_AmplitudeGain = screenShakeIntensity;
        rightNoise.m_FrequencyGain = screenShakeIntensity;



        yield return new WaitForSeconds (screenShakeDuration); 
        leftNoise.m_AmplitudeGain = 0;
        leftNoise.m_FrequencyGain = 0;
        rightNoise.m_AmplitudeGain = 0;
        rightNoise.m_FrequencyGain = 0;

    }

    public void playerSlowed(float slowAmount)
    {
        if (slowed) return;
        slowed = true;
        topSpeed /= slowAmount;
        flightSpeed /= slowAmount;
        moveHorizontalFlightSpeed /= slowAmount;
    }

    public void playerUnslowed(float slowAmount)
    {
        if (slowed)
        {
            slowed = false;
            topSpeed *= slowAmount;
            flightSpeed *= slowAmount;
            moveHorizontalFlightSpeed *= slowAmount;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
        Debug.Log("Trying to move");
    }
    public void OnJump(InputAction.CallbackContext context) 
    {
        jumped = context.action.triggered;
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        aimInput = context.ReadValue<Vector2>();
    }
    public void OnShoot(InputAction.CallbackContext context)
    {
        fired = context.action.triggered;
    }
    public void OnShield(InputAction.CallbackContext context)
    {
        shielded = context.action.triggered;
    }

    public void OnAltShoot(InputAction.CallbackContext context)
    {
        altFired = context.action.triggered;
    }

    public void OnBackShoot(InputAction.CallbackContext context)
    {
        backShoot = context.action.triggered;
    }

    public void OnPaused(InputAction.CallbackContext context)
    {
        paused = context.action.triggered;
    }
}
