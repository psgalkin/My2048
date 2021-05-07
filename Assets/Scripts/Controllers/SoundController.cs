using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] AudioClip _swapSound;
    [SerializeField] AudioClip[] _matchSounds;
    
    private List<AudioSource> _sources;

    private void Awake()
    {
        _sources = new List<AudioSource>();
    }

    private void Start()
    {
        _sources = new List<AudioSource>();

        for (int i = 0; i < 10; ++i)
        {
            AudioSource audioSource = Resources.Load<AudioSource>("AudioSource");
            AudioSource newSource = Instantiate<AudioSource>(audioSource, transform);
            newSource.transform.position = transform.position;
            _sources.Add(newSource);
        }
    }

    public void PlayMatchSound(int level)
    {
        Play(_matchSounds[level]);
    }

    public void PlaySwapSound()
    {
        Play(_swapSound);
    }

    private void Play(AudioClip clip)
    {
        if (TryRestartPlayingSound(clip)) 
            return;

        foreach (AudioSource source in _sources)
        {
            if (!source.isPlaying)
            {
                source.clip = clip;
                source.Play();
                return;
            }
        }  
    }

    private bool TryRestartPlayingSound(AudioClip clip)
    {
        foreach (var source in _sources)
        {
            if (source.clip == clip && !source.isPlaying)
            {
                source.Stop();
                source.Play();
                return true;
            }
        }

        return false;
    }

    private void OnDestroy()
    {
        _sources.Clear();
    }
}
