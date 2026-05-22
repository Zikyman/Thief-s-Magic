using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TagProjectile : MonoBehaviour
{
    private TagType payloadTag;
    private TagShooter shooterReference;
    private float speed;

    // NOVÉ PROMĚNNÉ PRO NAVÁDĚNÍ
    private Transform target;
    private Vector3 lastKnownPosition;

    public void Initialize(TagType tagToCarry, TagShooter shooter, float bulletSpeed, Transform targetTransform)
    {
        payloadTag = tagToCarry;
        shooterReference = shooter;
        speed = bulletSpeed;
        
        // Uložíme si cíl a jeho první pozici
        target = targetTransform;
        lastKnownPosition = target.position;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (payloadTag == TagType.None)
                sr.color = new Color(1f, 1f, 1f, 0.5f);
            else
                sr.color = TagManager.Instance.GetColorForTag(payloadTag);
        }

        Destroy(gameObject, 5f); // 5 sekund na dolet, pak se zničí sám
    }

    void Update()
    {
        // 1. Aktualizace pozice cíle
        if (target != null)
        {
            lastKnownPosition = target.position;
        }

        // 2. VÝPOČET ROTACE (Otáčení za cílem)
        Vector3 direction = (lastKnownPosition - transform.position).normalized;
        
        // Zabráníme chybě, když už jsme dorazili na přesné souřadnice cíle
        if (direction != Vector3.zero) 
        {
            // Vypočítáme úhel v radiánech a převedeme na stupně
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

            // Plynule otočíme projektil směrem k cíli (speed * 2f zajišťuje, že rotace stíhá rychlost letu)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * 2f * Time.deltaTime);
        }

        // 3. Fyzický let k cíli
        transform.position = Vector3.MoveTowards(transform.position, lastKnownPosition, speed * Time.deltaTime);

        // 4. Sebedestrukce, pokud cíl zmizel a my už jsme na místě
        if (target == null && Vector3.Distance(transform.position, lastKnownPosition) < 0.1f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (shooterReference != null && collision.transform.root == shooterReference.transform.root) return;
        if (collision.gameObject.name.Contains("Camera") || collision.gameObject.name.Contains("Bound")) return;

        TaggableObject hitTarget = collision.GetComponent<TaggableObject>();

        // POZNÁMKA: Může se stát, že cestou k cíli omylem trefíme JINOU bednu. 
        // Většinou je fajn to nechat (hráč musí mít čistý výhled). 
        // Takže aplikujeme tag na cokoliv, co trefíme cestou jako první.
        if (hitTarget != null)
        {
            TagType objectTag = hitTarget.CurrentTag;

            if (payloadTag == TagType.None && objectTag != TagType.None)
            {
                GiveAmmoToShooter(objectTag);
                hitTarget.RemoveTag();
            }
            else if (payloadTag != TagType.None && objectTag == TagType.None)
            {
                hitTarget.ReceiveTag(payloadTag);
            }
            else if (payloadTag != TagType.None && objectTag != TagType.None && payloadTag != objectTag)
            {
                GiveAmmoToShooter(objectTag);
                hitTarget.RemoveTag(); 
                hitTarget.ReceiveTag(payloadTag); 
            }
            else if (payloadTag != TagType.None && payloadTag == objectTag)
            {
                GiveAmmoToShooter(payloadTag);
            }

            Destroy(gameObject);
        }
        else
        {
            if (!collision.isTrigger) Destroy(gameObject);
        }
    }

    private void GiveAmmoToShooter(TagType ammoToGive)
    {
        if (shooterReference != null) shooterReference.ReceiveAmmo(ammoToGive);
    }
}