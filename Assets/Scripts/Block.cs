using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int owner = 0; // 0 for neutral, 1 for P1, 2 for P2
    public Material[] materials; // Assign in the inspector, 0 for P1, 1 for P2
    private bool isUpdating = false;

    // Call this method to change the block's ownership
    public void SetOwnership(int newOwner)
    {
        if (isUpdating) return; // Prevent concurrent updates
        isUpdating = true;
        owner = newOwner;
        Renderer renderer = GetComponent<Renderer>();

        // Assuming the first material is for P1 and the second for P2
        if (newOwner == 1)
        {
            renderer.material = materials[0];
            gameObject.layer = LayerMask.NameToLayer("P1");
        }
        else if (newOwner == 2)
        {
            renderer.material = materials[1];
            gameObject.layer = LayerMask.NameToLayer("P2");
        }
        isUpdating = false; // Reset flag after update
    }
}
