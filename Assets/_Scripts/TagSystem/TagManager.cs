using UnityEngine;
public class TagManager : MonoBehaviour
{
    // Zajišťuje, že ke skriptu může přistoupit kdokoliv z jakéhokoliv skriptu přes "TagManager.Instance"
    public static TagManager Instance { get; private set; }
    
    public TagDatabaseSO Database;

    private void Awake()
    {
        // Singleton logika: Ujistíme se, že ve scéně je vždy jen jeden TagManager
        if (Instance == null) 
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Odkomentuj, pokud chceš, aby manažer přežil načítání nových levelů
        }
        else 
        {
            Destroy(gameObject); // Zničí duplikáty, pokud bys omylem dal dva do scény
        }
    }

    // Centrální vyhledávací metoda
    public Color GetColorForTag(TagType tagType)
    {
        // Ochrana: Kdybys zapomněl do Inspectoru databázi přetáhnout
        if (Database == null)
        {
            Debug.LogError("TagManager nemá přiřazenou databázi!");
            // Vrátíme křiklavou magentu, abys ve hře hned vizuálně poznal, že je něco špatně
            return Color.magenta; 
        }

        // Projdeme list a najdeme správnou barvu
        foreach (var tag in Database.AllTags)
        {
            if (tag.Type == tagType) 
            {
                return tag.TagColor;
            }
        }
        
        // Pokud tag v databázi ještě nemáš nastavený, varujeme tě a dáme bílou
        Debug.LogWarning($"Barva pro tag {tagType} nebyla v databázi nalezena. Přidej ji tam!");
        return Color.white; 
    }
}