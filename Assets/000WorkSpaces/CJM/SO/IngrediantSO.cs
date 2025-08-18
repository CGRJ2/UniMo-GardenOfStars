using UnityEngine;

[CreateAssetMenu(fileName = "Ingrediant", menuName = "SO/Ingrediant")]
public class IngrediantSO : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public int Code { get; private set; }
    [field: SerializeField] public int MaxStackableCount { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public GameObject InstancePrefab { get; private set; }
}
