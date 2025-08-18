using UnityEngine;

[CreateAssetMenu(fileName = "RegionSO", menuName = "SO/Region")]
public class RegionSO : ScriptableObject
{
    public string regionName;
    public int testValue;

    private void OnEnable()
    {
        regionName = this.name;
    }
}
