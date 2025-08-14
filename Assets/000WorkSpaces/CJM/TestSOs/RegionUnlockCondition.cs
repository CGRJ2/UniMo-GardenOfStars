using UnityEngine;

[CreateAssetMenu(fileName = "RegionUnlockCondition", menuName = "RegionUnlockCondition")]
public class RegionUnlockCondition : ScriptableObject
{
    public string regionName;
    public int testValue;

    private void OnEnable()
    {
        regionName = this.name;
    }
}
