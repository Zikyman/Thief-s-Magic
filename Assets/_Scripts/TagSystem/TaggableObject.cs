using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class TaggableObject : MonoBehaviour
{
    [Header("Nastavení objektu")]
    public ObjectMaterial MaterialType; 
    
    [Tooltip("Jaký tag má objekt už od začátku levelu?")]
    public TagType StartingTag = TagType.None; 
    
    // HideInInspector, aby tě to nelákalo měnit ručně za běhu v editoru, 
    // od toho teď slouží StartingTag a samotná herní logika
    //[HideInInspector] 
    public TagType CurrentTag = TagType.None; 
    
    private SpriteRenderer sr;
    private Material outlineMaterial;
    private MonoBehaviour activeBehaviorComponent; 
    private bool isInitialized = false;

    private void Start()
    {
        InitializeMaterial();
        
        // Pokud jsme nastavili, že má objekt už od začátku tag, rovnou mu ho dáme
        if (StartingTag != TagType.None)
        {
            ReceiveTag(StartingTag);
        }
    }

    // Bezpečně nastaví materiál POUZE JEDNOU
    private void InitializeMaterial()
    {
        if (isInitialized) return;

        sr = GetComponent<SpriteRenderer>();
        outlineMaterial = sr.material; // Vytvoří instanci jen jednou

        if (outlineMaterial.HasProperty("_OutlineWidth"))
            outlineMaterial.SetFloat("_OutlineWidth", 0f);
        else
            Debug.LogWarning($"[TaggableObject] Objekt {gameObject.name} nemá náš Outline shader!");

        isInitialized = true;
    }

    public void ReceiveTag(TagType incomingTag)
    {
        if (incomingTag == CurrentTag) return; 

        // Pro jistotu (např. když projektil zasáhne objekt v 1. framu dřív, než proběhl Start)
        InitializeMaterial(); 

        CurrentTag = incomingTag;
        
        // 1. Změna barvy a zapnutí obrysu
        if (outlineMaterial != null && outlineMaterial.HasProperty("_OutlineWidth"))
        {
            Color tagColor = TagManager.Instance.GetColorForTag(CurrentTag);
            outlineMaterial.SetColor("_OutlineColor", tagColor);
            outlineMaterial.SetFloat("_OutlineWidth", 2f); 
        }

        // 2. Úklid starého chování
        if (activeBehaviorComponent != null)
        {
            Destroy(activeBehaviorComponent);
        }

        // 3. Přidání nového chování
        switch (CurrentTag)
        {
            case TagType.Fire:
                activeBehaviorComponent = gameObject.AddComponent<FireBehavior>();
                break;
            case TagType.Bouncy:
                activeBehaviorComponent = gameObject.AddComponent<BouncyBehavior>();
                break;
        }
    }

    public void RemoveTag()
    {
        CurrentTag = TagType.None;

        if (outlineMaterial != null && outlineMaterial.HasProperty("_OutlineWidth"))
        {
            outlineMaterial.SetFloat("_OutlineWidth", 0f);
        }

        if (activeBehaviorComponent != null)
        {
            Destroy(activeBehaviorComponent);
            activeBehaviorComponent = null; 
        }
    }
}