using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TagStruct
{
    public TagType Type;
    public Color TagColor;
}

[CreateAssetMenu(fileName = "TagDatabase", menuName = "Tags/Database")]
public class TagDatabaseSO : ScriptableObject
{
    public List<TagStruct> AllTags = new List<TagStruct>();
}

