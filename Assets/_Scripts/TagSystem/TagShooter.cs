using UnityEngine;

public class TagShooter : MonoBehaviour
{
    [Header("Munice")]
    public TagType CurrentAmmo = TagType.None;

    [Header("Nastavení střelby")]
    public GameObject ProjectilePrefab;
    public Transform FirePoint;
    public float ProjectileSpeed = 10f;

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main; // Uložíme si kameru pro převod myši
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            TryTargetAndShoot();
        }
    }

    private void TryTargetAndShoot()
    {
        // 1. Zjistíme, kde je myš v herním 2D světě
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        // 2. Uložíme si globální nastavení Unity a dočasně v kódu povolíme trefování Triggerů
        bool originalSetting = Physics2D.queriesHitTriggers;
        Physics2D.queriesHitTriggers = true;

        // 3. OverlapPointAll je na 2D klikání nejlepší. 
        // Získáme pole ÚPLNĚ VŠEHO (normálních colliderů i triggerů), co zrovna leží pod kurzorem.
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePos2D);

        // 4. Okamžitě vrátíme nastavení do původního stavu
        Physics2D.queriesHitTriggers = originalSetting;

        // 5. Projdeme všechny objekty pod myší
        foreach (Collider2D hitCollider in hits)
        {
            TaggableObject targetObj = hitCollider.GetComponent<TaggableObject>();

            if (targetObj != null)
            {
                // Bingo! Vystřelíme projektil na cíl
                Shoot(targetObj.transform);
                
                // Return ukončí metodu, takže nevystřelíme dvě střely, 
                // pokud by se dva Taggable objekty překrývaly přes sebe
                return; 
            }
        }
    }

    private void Shoot(Transform targetTransform)
    {
        if (ProjectilePrefab == null || FirePoint == null) return;

        GameObject bullet = Instantiate(ProjectilePrefab, FirePoint.position, Quaternion.identity);
        
        TagProjectile projectileScript = bullet.GetComponent<TagProjectile>();
        if (projectileScript != null)
        {
            // Nyní posíláme projektilu i informaci o tom, co má trefit
            projectileScript.Initialize(CurrentAmmo, this, ProjectileSpeed, targetTransform);
        }

        CurrentAmmo = TagType.None;
    }

    public void ReceiveAmmo(TagType newAmmo)
    {
        CurrentAmmo = newAmmo;
        Debug.Log($"[Hráč] Mám nabito: {CurrentAmmo}");
    }
}