using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SfxController : MonoBehaviour
{
    public Transform Target;

    private AudioSource _sfxSource;

    private Vector3 curPosition;

    public void Awake()
    {
        _sfxSource = gameObject.GetOrAddComponent<AudioSource>();
    }

    private void Update()
    {
        // 현재 위치와 타겟 위치가 다르면, 위치 이동
        if(Target != null && curPosition != Target.position)
        {
            transform.position = Target.position;
            curPosition = transform.position;
        }
    }


    public void SfxPlay(AudioData data, float volume)
    {
        _sfxSource.Stop();

        _sfxSource.clip = data.Clip;

        _sfxSource.volume = volume;

        _sfxSource.Play();

        StartCoroutine(SfxPlayCoroutine(data.Clip.length, data));
    }

    private IEnumerator SfxPlayCoroutine(float time, AudioData data)
    {
        yield return new WaitForSeconds(time);

        _sfxSource.Stop();
        Resources.UnloadAsset(data);

        Manager.Audio.SfxPool.Release(this);
    }
}
