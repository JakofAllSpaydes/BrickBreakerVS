using UnityEngine;

public class BallController : MonoBehaviour
{
    public float baseSpeed = 5f;
    public float speed;
    public float speedMult = 1f;
    public float baseSize = 0.5f;
    public float size;
    public float sizeMult = 1f;
    public int pierce = 0;
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

    public float randomness = 10f; // Control the amount of randomness in bounce

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerManager = FindObjectOfType<PlayerManager>();
        ChooseRandomDirection();
    }

    public void CalculateStats()
    {
        if (baseSize <= 0.1f) { baseSize = 0.1f; }
        if (baseSpeed <= 0.1f) { baseSpeed = 0.1f; }
        if (sizeMult <= 0.05f) { sizeMult = 0.05f; }
        if (speedMult <= 0.05f) { speedMult = 0.05f; }

        speed = baseSpeed * speedMult;
        size = baseSize * sizeMult;

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

            Renderer renderer = other.gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = renderer.material.name.StartsWith(material1.name) ? material2 : material1;
                other.gameObject.layer = other.gameObject.layer == LayerMask.NameToLayer("P1") ? LayerMask.NameToLayer("P2") : LayerMask.NameToLayer("P1");

                if (playerManager != null)
                {
                    playerManager.AddXP(playerID);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
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
}