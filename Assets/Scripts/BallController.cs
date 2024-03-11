using UnityEngine;
using System.Collections;
using System.Linq;


public class BallController : MonoBehaviour
{
    public float baseSpeed = 5f;
    public float speed;
    public float speedMult = 1f;

    public float baseSize = 0.5f;
    public float size;
    public float sizeMult = 1f;

    public int pierce = 0;
    public float pierceCooldown = 1f;
    public float pierceCooldownTimer = 0f;
    public int availablePierce = 0;
    private bool isPierceCooldown = false;

    public float boostMult = 1f;
    public float boostDuration = 0f;

    public float chargeMult = 1f;
    private float localChargeMult = 1f;
    private bool resetCharge = false;
    public float chargeRate = 0.1f;
    private float chargeTimer = 0f; // Timer to track charge rate timing
    private float cumulativeChargeMult = 1f; // Initialize outside Update

    public float bounceAngle = 0f;

    public float fluxSizeTimer = 0f;
    public float fluxSizeMin = 0f;
    public float fluxSizeMax = 0f;

    public float fluxSpeedTimer = 0f;
    public float fluxSpeedMin = 0f;
    public float fluxSpeedMax = 0f;

    private Rigidbody rb;
    private Vector3 direction;
    public LayerMask collideWithLayer; // Set this in the inspector
    public Material material1;
    public Material material2;

    public int playerID;
    private PlayerManager playerManager;
    private CameraController cameraController;
    public GameManager gameManager;

    public float randomness = 10f; // Control the amount of randomness in bounce
    public float shakeIntensity = 0.05f;
    public float shakeDuration = 0.05f;

    public GameObject visualRepresentation; // Assign the child GameObject with MeshRenderer here
    private Vector3 originalScale;
    public ParticleSystem collisionEffectPrefab;
    private Collider triggerCollider; // The trigger collider for blocks
    private Collider nonTriggerCollider;    


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerManager = FindObjectOfType<PlayerManager>();
        gameManager = FindObjectOfType<GameManager>();
        cameraController = FindObjectOfType<CameraController>();

        if (visualRepresentation != null)
        {
            originalScale = visualRepresentation.transform.localScale;
        }

        ChooseRandomDirection();
        pierceCooldownTimer = 0f;
        resetCharge = false;
    }

    void Update()
    {
        if (isPierceCooldown && Time.timeScale > 0)
        {
            pierceCooldownTimer += Time.deltaTime;
            if (pierceCooldownTimer >= pierceCooldown)
            {
                // Cooldown has finished
                isPierceCooldown = false;
                int newPierce = pierce;
                availablePierce = newPierce; // Refresh available pierce count
                pierceCooldownTimer = 0f; // Reset the cooldown timer
            }
        }

        // Start with base speed calculation
        float currentBaseSpeed = baseSpeed * speedMult;

        if (!resetCharge && chargeTimer >= chargeRate)
        {
            cumulativeChargeMult *= chargeMult; // Stack the multiplier
            chargeTimer = 0f; // Reset timer for next application
        }
        else if (resetCharge)
        {
            resetCharge = false;
            cumulativeChargeMult = 1f; // Reset cumulative multiplier
        }
        chargeTimer += Time.deltaTime;

        // Calculate the charged speed as the new baseline
        float chargedSpeed = baseSpeed * speedMult * cumulativeChargeMult;

        // Apply flux speed effect to the charged speed
        if (fluxSpeedTimer > 0)
        {
            float cycleProgress = (Time.time % fluxSpeedTimer) / fluxSpeedTimer;
            float sineWave = Mathf.Sin(cycleProgress * Mathf.PI * 2);
            float normalizedSine = (sineWave + 1) / 2; // Normalize sine wave output to [0, 1]
            float fluxMultiplier = Mathf.Lerp(fluxSpeedMin, fluxSpeedMax, normalizedSine);
            chargedSpeed *= fluxMultiplier; // Apply flux effect to charged speed
        }

        // Use chargedSpeed, which now incorporates both charge and flux effects
        rb.velocity = direction.normalized * chargedSpeed;
        speed = chargedSpeed; // Update visible/debugging speed value

        // Flux Size Effect
        if (fluxSizeTimer > 0)
        {
            float sizeCycleProgress = (Time.time % fluxSizeTimer) / fluxSizeTimer;
            float sizeMultiplier = CalculateFluxMultiplier(sizeCycleProgress, fluxSizeMin, fluxSizeMax);
            size = baseSize * sizeMultiplier * sizeMult;
            transform.localScale = new Vector3(size, size, size);
        }
    }

    float CalculateFluxMultiplier(float progress, float minMultiplier, float maxMultiplier)
    {
        bool isScalingUp = progress <= 0.5f;
        progress = isScalingUp ? progress * 2 : (progress - 0.5f) * 2;
        float easedProgress = EaseOutQuart(progress);
        return isScalingUp ? Mathf.Lerp(1f, maxMultiplier, easedProgress) : Mathf.Lerp(maxMultiplier, minMultiplier, easedProgress);
    }

    public void CalculateStats()
    {
        if (baseSize <= 0.1f) { baseSize = 0.1f; }
        if (baseSpeed <= 0.1f) { baseSpeed = 0.1f; }
        if (sizeMult <= 0.1f) { sizeMult = 0.1f; }
        if (speedMult <= 0.1f) { speedMult = 0.1f; }
        if (pierceCooldown <= 0.2f) { pierceCooldown = 0.2f; }
        if (chargeRate <= 0.02f) { chargeRate = 0.02f; }

        if (fluxSizeTimer != 0)
        {
            if (fluxSizeTimer <= 0.1f) { fluxSizeTimer = 0.1f; }
        }
        if (fluxSpeedTimer != 0)
        {
            if (fluxSpeedTimer <= 0.1f) { fluxSpeedTimer = 0.1f; }
        }

        speed = baseSpeed * speedMult;
        size = baseSize * sizeMult;

        int newPierce = pierce;
        availablePierce = newPierce;

        transform.localScale = new Vector3 (size,size,size);
    }

    void ResetCharge()
    {
        resetCharge = true;
        chargeTimer = 0f;
    }

    private void ChooseRandomDirection()
    {
        direction = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f)).normalized;
        rb.velocity = direction * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & collideWithLayer) != 0)
        {
            ResetCharge();
            AdjustDirectionAndVelocity();

            StartCoroutine(SquashEffect());
            if (collisionEffectPrefab != null)
            {
                ParticleSystem effect = Instantiate(collisionEffectPrefab, transform.position, Quaternion.identity);
            }

            Block collidedBlock = other.gameObject.GetComponent<Block>();
            if (collidedBlock != null && collidedBlock.owner != playerID)
            {
                ChangeBlockOwnershipAndPierce(collidedBlock);
            }
        }
    }

    void ChangeBlockOwnershipAndPierce(Block initialBlock)
    {
        // Notify GameManager of the ownership change for the initial block
        gameManager.ChangeBlockOwnership(initialBlock.owner, playerID);
        initialBlock.SetOwnership(playerID);

        // Trigger hitstop effect
        gameManager.TriggerHitstop();

        cameraController.TriggerShake(shakeIntensity, shakeDuration);

        // Additional logic for XP adjustment
        if (playerManager != null)
        {
            playerManager.AddXP(playerID);
        }

        // Apply pierce effect if available
        if (availablePierce > 0)
        {
            // Find blocks in front of the collided block based on the ball's trajectory
            PierceAdditionalBlocks(initialBlock.transform.position, direction);
            availablePierce--;
        }
    }

    void PierceAdditionalBlocks(Vector3 position, Vector3 trajectory)
    {
        // Get all blocks in the scene
        Block[] allBlocks = FindObjectsOfType<Block>();
        // Exclude the block already hit
        allBlocks = allBlocks.Where(block => block.transform.position != position).ToArray();

        // Calculate the "forward" direction relative to the ball's trajectory
        Vector3 forward = trajectory.normalized;

        // Sort blocks by distance from the initial block and by alignment with the trajectory
        var sortedBlocks = allBlocks
            .Select(block => new { block, distance = Vector3.Distance(position, block.transform.position) })
            .OrderBy(item => item.distance)
            .ThenBy(item => Vector3.Dot(forward, (item.block.transform.position - position).normalized))
            .ToList();

        Debug.Log(sortedBlocks.ToList());

        // Apply pierce effect to the closest block(s) based on available pierce count
        int blocksToPierce = Mathf.Min(availablePierce, sortedBlocks.Count);
        for (int i = 0; i < blocksToPierce; i++)
        {
            var blockInfo = sortedBlocks[i];
            gameManager.ChangeBlockOwnership(blockInfo.block.owner, playerID);
            blockInfo.block.SetOwnership(playerID);
            Debug.Log("Pierce applied");
        }
        isPierceCooldown = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            StartCoroutine(SquashEffect());
            AdjustDirectionAndVelocity(collision.contacts[0].normal, true);
        }
    }

    private void AdjustDirectionAndVelocity(Vector3? collisionNormal = null, bool isWall = false)
    {
        if (bounceAngle != 0 && isWall)
        {
            // Calculate new direction based on the bounce angle
            Vector3 bounceDirection = CalculateBounceDirection(collisionNormal.HasValue ? collisionNormal.Value : -direction);
            direction = bounceDirection.normalized;
        }
        else if (collisionNormal.HasValue)
        {
            // Reflect the ball's direction based on the collision normal
            direction = Vector3.Reflect(direction, collisionNormal.Value);
        }
        else
        {
            // For triggers, since we don't have a normal, just reverse the direction
            direction = -direction;
        }

        rb.velocity = direction * speed;
    }

    private Vector3 CalculateBounceDirection(Vector3 collisionNormal)
    {
        // Convert bounce angle to radians since Unity's math functions use radians
        float angleRadians = bounceAngle * Mathf.Deg2Rad;

        // Assuming the bounce angle is relative to the surface normal, calculate the new direction
        // This simplistic approach might need adjustments based on your game's specific mechanics
        Vector3 newDirection;

        if (bounceAngle == 180)
        {
            // Perfect reflection
            newDirection = -direction;
        }
        else
        {
            // Calculate rotation around the axis perpendicular to the collision normal and the initial direction
            // This example assumes a 2D plane calculation; 3D might require more complex handling
            Vector3 rotationAxis = Vector3.Cross(direction, collisionNormal).normalized;
            newDirection = Quaternion.AngleAxis(bounceAngle, rotationAxis) * direction;
        }

        return newDirection;
    }

    private IEnumerator SquashEffect()
    {
        float duration = 0.1f; // Duration of the effect
        float time = 0;

        while (time < duration)
        {
            float t = time / duration;
            // Calculate easing value
            float easeValue = EaseInOutBack(t);

            // Calculate scale factors ensuring they never go below a certain threshold to avoid negative/infinite scales
            float scaleX = Mathf.Max(0.1f, originalScale.x * (1 + easeValue * 0.3f)); // Example ease modifier for squash
            float scaleY = Mathf.Max(0.1f, originalScale.y / (1 + easeValue * 0.3f)); // Ensure scaleY doesn't go below 0.1 to avoid division by zero
            float scaleZ = Mathf.Max(0.1f, originalScale.z * (1 + easeValue * 0.3f)); // Adjust Z similarly to X

            visualRepresentation.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

            time += Time.deltaTime;
            yield return null;
        }

        // Reset to original scale
        visualRepresentation.transform.localScale = originalScale;
    }

    private float EaseInOutBack(float t)
    {
        float c1 = 1.70158f;
        float c2 = c1 * 1.525f;

        return t < 0.5
          ? (Mathf.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
          : (Mathf.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
    }

    private float EaseOutQuart(float t)
    {
        return 1 - Mathf.Pow(1 - t, 4);
    }

}