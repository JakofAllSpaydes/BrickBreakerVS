using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyParticleSystem : MonoBehaviour
{
    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        // Check if the particle system has stopped playing
        if (ps != null && !ps.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
