using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AudioManager : Singleton<AudioManager>
{
    private const int MIN_DB = -80;

    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private AudioMixerGroup _sfxGroup;
    public ObjectPool<SfxController> SfxPool { get; private set; }

    public float MasterVolume
    {
        get
        {
            _audioMixer.GetFloat("MasterVolume", out float db);
            return Mathf.Pow(10f, db / 20f);
        }

        set
        {
            if (value <= 0)
            {
                _audioMixer.SetFloat("MasterVolume", MIN_DB);
            }
            else
            {
                float db = Mathf.Log10(value) * 20f;
                _audioMixer.SetFloat("MasterVolume", db);
            }
        }
    }
    public float BgmVolume
    {
        get 
        {
            _audioMixer.GetFloat("BgmVolume", out float db);
            return Mathf.Pow(10f, db / 20f);
        }
            
        set
        {
            if(value <= 0)
            {
                _audioMixer.SetFloat("BgmVolume", MIN_DB);
            }
            else
            {
                float db = Mathf.Log10(value) * 20f;
                _audioMixer.SetFloat("BgmVolume", db);
            }
        }
    }

    public float SfxVolume
    {
        get
        {
            _audioMixer.GetFloat("SfxVolume", out float db);
            return Mathf.Pow(10f, db / 20f);
        }

        set
        {
            if (value <= 0)
            {
                _audioMixer.SetFloat("SfxVolume", MIN_DB);
            }
            else
            {
                float db = Mathf.Log10(value) * 20f;
                _audioMixer.SetFloat("SfxVolume", db);
            }
        }
    }

    private AudioSource _bgmSource;

    //루프사운드 관리하는 딕셔너리
    private Dictionary<string, SfxController> _loopingSfxDict = new Dictionary<string, SfxController>();
    private Dictionary<string, AudioData> _loopingSfxDataDict = new Dictionary<string, AudioData>();

    private void Awake()
    {
        _bgmSource = gameObject.GetOrAddComponent<AudioSource>();
        _bgmSource.loop = true;

        SfxPool = new ObjectPool<SfxController>(CreateSfx, GetSfx, ReleaseSfx, DestroySfx);

        for(int i = 0; i < 10; i++)
        {
            GameObject obj = new GameObject("SfxController");
            obj.transform.parent = transform;
            AudioSource audioSource = obj.AddComponent<AudioSource>();

            audioSource.outputAudioMixerGroup = _sfxGroup;
            audioSource.spatialBlend = 1.0f;
            audioSource.minDistance = 5f;
            audioSource.maxDistance = 30f;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;

            SfxPool.Release(obj.AddComponent<SfxController>());
        }
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

            _bgmSource.DOKill();
            _bgmSource.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                _bgmSource.Stop();
                _bgmSource.clip = clip;
                _bgmSource.Play();

                _bgmSource.DOFade(data.Result.Volume, fadeDuration);
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
                Debug.LogWarning($"[AudioManager] {clipName} AudioData를 찾을 수 없습니다.");
                return;
            }

            if(data.Result.Clip == null)
            {
                Debug.LogWarning($"[AudioManager] {clipName} 오디오 클립이 null입니다.");
                return;
            }


            SfxController sfx = SfxPool.Get();
            sfx.Target = target;
            sfx.SfxPlay(data.Result, Mathf.Clamp01(data.Result.Volume));
            Addressables.Release(data);
        };
    }

    private SfxController CreateSfx()
    {
        GameObject obj = new GameObject("SfxController");
        obj.transform.parent = transform;
        AudioSource audioSource = obj.AddComponent<AudioSource>();

        audioSource.outputAudioMixerGroup = _sfxGroup;
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

        if (target == null)
        {
            target = Camera.main.transform;
        }

        Addressables.LoadAssetAsync<AudioData>($"Audio/{clipName}.asset").Completed += data =>
        {
            if (data.Result == null)
            {
                Debug.LogWarning($"[AudioManager] {clipName} AudioData를 찾을 수 없습니다.");
                return;
            }

            if (data.Result.Clip == null)
            {
                Debug.LogWarning($"[AudioManager] {clipName} 오디오 클립이 null입니다.");
                return;
            }


            SfxController sfx = SfxPool.Get();
            sfx.Target = target;

            AudioSource source = sfx.GetComponent<AudioSource>();
            source.clip = data.Result.Clip;
            source.volume = Mathf.Clamp01(data.Result.Volume);
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

    public void SfxStopLoop(string key, float fadeDuration = 0)
    {
        if (!_loopingSfxDict.TryGetValue(key, out SfxController sfx))
            return; // 이미 Release된 상태

        if(sfx != null)
        {
            AudioSource source = sfx.GetComponent<AudioSource>();

            source.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                source.Stop();
                source.loop = false;
                source.volume = Mathf.Clamp01(MasterVolume * SfxVolume);
                SfxPool.Release(sfx);
            });
        }

        _loopingSfxDict.Remove(key);
        Resources.UnloadAsset(_loopingSfxDataDict[key]);
        _loopingSfxDataDict.Remove(key);
    }
}
