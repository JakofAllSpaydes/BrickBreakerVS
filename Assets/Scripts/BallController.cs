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
    public float pierceCooldown = 0.5f;
    private float pierceCooldownTimer = 0f;
    private int availablePierce = 0;
    private bool isPierceCooldown = false;

    public float boostMult = 1f;
    public float boostDuration = 0f;

    public float chargeMult = 1f;
    public float chargeRate = 1f;

    public float bounceAngle;

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
        availablePierce = pierce; // Initialize availablePierce
        pierceCooldownTimer = 0f;
    }

    void FixedUpdate()
    {
        if (isPierceCooldown && Time.timeScale > 0)
        {
            pierceCooldownTimer += Time.deltaTime;
            if (pierceCooldownTimer >= pierceCooldown)
            {
                // Cooldown has finished
                isPierceCooldown = false;
                availablePierce = pierce; // Refresh available pierce count
                pierceCooldownTimer = 0f; // Reset the cooldown timer
            }
        }
    }

    public void CalculateStats()
    {
        if (baseSize <= 0.1f) { baseSize = 0.1f; }
        if (baseSpeed <= 0.1f) { baseSpeed = 0.1f; }
        if (sizeMult <= 0.1f) { sizeMult = 0.1f; }
        if (speedMult <= 0.1f) { speedMult = 0.1f; }
        if (pierceCooldown <= 0.2f) { pierceCooldown = 0.2f; }

        speed = baseSpeed * speedMult;
        size = baseSize * sizeMult;
        availablePierce = pierce;

        transform.localScale = new Vector3 (size,size,size);
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
        if (collisionNormal.HasValue)
        {
            // Reflect the ball's direction based on the collision normal
            direction = Vector3.Reflect(direction, collisionNormal.Value);
        }
        else
        {
            // For triggers, since we don't have a normal, just reverse the direction
            direction = -direction;
        }

        // Apply randomness to the direction. Increase the range if it's a wall to ensure it doesn't get stuck.
        float angleRandomness = isWall ? randomness * 2 : randomness;
        direction = Quaternion.Euler(0, Random.Range(-angleRandomness, angleRandomness), 0) * direction;

        rb.velocity = direction.normalized * speed;
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

}