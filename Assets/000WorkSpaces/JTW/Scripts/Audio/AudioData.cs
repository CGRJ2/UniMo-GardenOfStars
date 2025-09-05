using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
[CreateAssetMenu(fileName = "AudioData", menuName = "Audio/Data")]
public class AudioData : ScriptableObject
{
    public AudioClip Clip;
    [Range(0f, 1f)]
    public float Volume = 1.0f;
}
