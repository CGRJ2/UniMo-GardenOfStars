using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AudioManager : Singleton<AudioManager>
{
    public ObjectPool<SfxController> SfxPool { get; private set; }

    public float MasterVolume = 1f;
    private float _bgmVolume = 1f;
    public float BgmVolume
    {
        get => _bgmVolume;
        set
        {
            _bgmVolume = value;
            _bgmSource.volume = MasterVolume * _bgmVolume * _bgmLocalVolume;
        }
    }
    private float _bgmLocalVolume;
    public float SfxVolume = 1f;

    private AudioSource _bgmSource;

    //루프사운드 관리하는 딕셔너리
    private Dictionary<string, SfxController> _loopingSfxDict = new Dictionary<string, SfxController>();
    private Dictionary<string, AudioData> _loopingSfxDataDict = new Dictionary<string, AudioData>();

    private void Awake()
    {
        _bgmSource = gameObject.GetOrAddComponent<AudioSource>();
        _bgmSource.loop = true;

        SfxPool = new ObjectPool<SfxController>(CreateSfx, GetSfx, ReleaseSfx, DestroySfx);
    }

    public void BgmPlay(string clipName, float fadeDuration = 0)
    {
        if (clipName == null)
        {
            _bgmSource.Stop();
            _bgmSource.clip = null;
            return;
        }

        Addressables.LoadAssetAsync<AudioData>($"Audio/{clipName}.asset").Completed += data =>
        {
            if (data.Result == null)
            {
                Debug.Log($"[AudioManager] {clipName} AudioData를 찾을 수 없습니다.");
                return;
            }

            AudioClip clip = data.Result.Clip;

            if (_bgmSource.clip == clip) return;

            _bgmLocalVolume = data.Result.Volume;
            _bgmSource.DOKill();
            _bgmSource.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                _bgmSource.Stop();
                _bgmSource.clip = clip;
                _bgmSource.Play();

                _bgmSource.DOFade(MasterVolume * BgmVolume * data.Result.Volume, fadeDuration);
                Addressables.Release(data);
            });
        };
    }

    public void SfxPlay(string clipName, Transform target = null)
    {
        if(target == null)
        {
            target = Camera.main.transform;
        }

        Addressables.LoadAssetAsync<AudioData>($"Audio/{clipName}.asset").Completed += data =>
        {
            if (data.Result == null)
            {
                Debug.Log($"[AudioManager] {clipName} AudioData를 찾을 수 없습니다.");
                return;
            }

            SfxController sfx = SfxPool.Get();
            sfx.Target = target;
            sfx.SfxPlay(data.Result, Mathf.Clamp01(MasterVolume * SfxVolume * data.Result.Volume));
            Addressables.Release(data);
        };
    }

    private SfxController CreateSfx()
    {
        GameObject obj = new GameObject("SfxController");
        obj.transform.parent = transform;
        AudioSource audioSource = obj.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1.0f;       
        audioSource.minDistance = 5f;        
        audioSource.maxDistance = 30f;      
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;

        return obj.AddComponent<SfxController>();
    }

    private void GetSfx(SfxController sfx)
    {
        sfx.gameObject.SetActive(true);
    }

    private void ReleaseSfx(SfxController sfx)
    {
        sfx.gameObject.SetActive(false);
    }

    private void DestroySfx(SfxController sfx)
    {
        Destroy(sfx.gameObject);
    }

    public void SfxPlayLoop(string key, string clipName, Transform target)
    {
        if (_loopingSfxDict.ContainsKey(key))
            return;

        Addressables.LoadAssetAsync<AudioData>($"Audio/{clipName}").Completed += data =>
        {
            if (data.Result == null)
            {
                Debug.Log($"[AudioManager] {clipName} AudioData를 찾을 수 없습니다.");
                return;
            }

            SfxController sfx = SfxPool.Get();
            sfx.Target = target;

            AudioSource source = sfx.GetComponent<AudioSource>();
            source.clip = data.Result.Clip;
            source.volume = Mathf.Clamp01(MasterVolume * SfxVolume * data.Result.Volume);
            source.loop = true;
            source.Play();

            _loopingSfxDict[key] = sfx;
            _loopingSfxDataDict[key] = data.Result;
            Addressables.Release(data);
        };
    }

    public void SetVolumeLoopSfx(string key, float volume, float minDistance = 5f, float maxDistance = 30f)
    {
        if (!_loopingSfxDict.ContainsKey(key))
        {
            Debug.Log($"해당 {key} 의 LoopSound가 없습니다.");
            return;
        }

        AudioSource source = _loopingSfxDict[key].GetComponent<AudioSource>();

        source.volume = volume;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
    }

    public void SfxStopLoop(string key)
    {
        if (!_loopingSfxDict.TryGetValue(key, out SfxController sfx))
            return; // 이미 Release된 상태

        if(sfx != null)
        {
            AudioSource source = sfx.GetComponent<AudioSource>();

            source.Stop();
            source.loop = false;
            source.volume = Mathf.Clamp01(MasterVolume * SfxVolume);
            SfxPool.Release(sfx);
        }

        _loopingSfxDict.Remove(key);
        Resources.UnloadAsset(_loopingSfxDataDict[key]);
        _loopingSfxDataDict.Remove(key);
    }
}
