using UnityEngine;

public class FireBehavior : MonoBehaviour
{
    private TaggableObject myTaggable;

    void Start()
    {
        myTaggable = GetComponent<TaggableObject>();

        switch (myTaggable.MaterialType)
        {
            case ObjectMaterial.Wood:
                Debug.Log($"{gameObject.name} je dřevo! Hořím a za 3 sekundy zmizím.");
                Invoke("BurnToAsh", 3f); 
                break;

            case ObjectMaterial.Metal:
                Debug.Log($"{gameObject.name} je kov! Jsem žhavý.");
                // Zde můžeš později přidat red tint nebo částice
                break;

            case ObjectMaterial.Stone:
                Debug.Log($"{gameObject.name} je kámen. Oheň se mě netýká.");
                Destroy(this, 1f); // Kámen oheň prostě po vteřině uhasí
                break;
        }
    }

    private void BurnToAsh()
    {
        Destroy(gameObject);
    }

    // Šíření ohně dotykem
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (myTaggable.MaterialType == ObjectMaterial.Stone) return;

        TaggableObject otherObj = collision.gameObject.GetComponent<TaggableObject>();
        
        // Pokud dotyčný objekt nemá FireTag, zapálíme ho!
        if (otherObj != null && otherObj.CurrentTag != TagType.Fire)
        {
            otherObj.ReceiveTag(TagType.Fire);
        }
    }
}