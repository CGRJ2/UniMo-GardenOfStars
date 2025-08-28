using UnityEngine;

[CreateAssetMenu(fileName = "Ingrediant", menuName = "SO/IngrediantData")]
public class IngrediantData : ScriptableObject
{
    [field: SerializeField] public string ID { get; private set; }
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public long Price { get; private set; }
    [field: SerializeField] public GameObject Prefab { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
}
