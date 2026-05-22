using UnityEngine;

public class BouncyBehavior : MonoBehaviour
{
    private Collider2D col;
    private Rigidbody2D rb;
    
    private PhysicsMaterial2D originalMaterial;
    private PhysicsMaterial2D bouncyMaterial;

    private void Start()
    {
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        
        if (col != null)
        {
            // 1. Uložíme si původní materiál (aby ho RemoveTag mohl vrátit)
            originalMaterial = col.sharedMaterial;

            // 2. Vytvoříme dynamicky nový, extrémně skákavý materiál
            bouncyMaterial = new PhysicsMaterial2D("SuperBouncy");
            bouncyMaterial.bounciness = 0.9f; // 1.0f je perpetuum mobile, 0.9f je zábavné
            bouncyMaterial.friction = 0.2f;

            // 3. Nasadíme ho na objekt
            col.sharedMaterial = bouncyMaterial;
        }

        // 4. GAME JUICE: Hned po tagnutí objekt lehce nadskočí!
        if (rb != null)
        {
            // Přepíšeme rychlost Y, aby to udělalo malý "hop"
            rb.linearVelocityY = 5f; 
        }
    }

    private void OnDestroy()
    {
        // Tato metoda se zavolá automaticky, když se zavolá RemoveTag() nebo Destroy()
        
        // 1. Vrátíme objektu jeho původní tření a odrazivost
        if (col != null)
        {
            col.sharedMaterial = originalMaterial;
        }

        // 2. Smažeme náš dynamicky vytvořený materiál z RAMky (Memory Management)
        if (bouncyMaterial != null)
        {
            Destroy(bouncyMaterial);
        }
    }
}