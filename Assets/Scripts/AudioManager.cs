using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public struct GameAudio
{
    public enum AudioType
    {
        SFX,
        Music
    }

    public AudioClip Clip;
    public AudioType Type;
    public string Name;
}

[RequireComponent(typeof(AudioSource), typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] bool _playMusicOnLoop = true;
    [SerializeField] AudioMixer mixer;
    [SerializeField] AudioSource _musicSource;
    [SerializeField] AudioSource _sfxSource;
    [SerializeField] GameAudio[] _soundsLibrary;

    public float MusicVolume { get { return GetChannelVolume("Music"); } set { SetChannelVolume("Music", value); } }

    private void Awake()
    {
        Instance = this;
        CheckMusicSource();
    }

    void CheckMusicSource() {
        if (_musicSource == null) {
            _musicSource = gameObject.GetComponent<AudioSource>();
        }
        else if (gameObject.GetComponent<AudioSource>() == null)
        {
            Debug.LogWarning("AudioManager: No AudioSource component found on the GameObject. Music playback may not work correctly.");
        }

        _musicSource.loop = _playMusicOnLoop;
    }
    GameAudio? Search(string name)
    {
        foreach (var sound in _soundsLibrary)
        {
            if (sound.Name == name) return sound;
        }
        return null;
    }

    public void PlaySoundByName(string name, Transform obj)
    {
        GameAudio? sound = Search(name);
        if (sound == null)
        {
            Debug.LogWarning($"Sound '{name}' not found!");
            return;
        }

        GameAudio s = sound.Value;
        switch (s.Type)
        {
            case GameAudio.AudioType.SFX:
                if (obj != null)
                {
                    AudioSource.PlayClipAtPoint(s.Clip, obj.position);
                }
                else
                {
                    _sfxSource.Stop();
                    _sfxSource.clip = s.Clip;
                    _sfxSource.Play();
                }
                break;

            case GameAudio.AudioType.Music:
                _musicSource.Stop();
                _musicSource.clip = s.Clip;
                _musicSource.Play();
                break;
        }
    }

    public void TransitionToSong(string songName, float time = 1f)
    {
        GameAudio? sound = Search(songName);
        if (sound == null)
        {
            Debug.LogWarning($"Sound '{songName}' not found!");
            return;
        }

        GameAudio s = sound.Value;

        StartCoroutine(Transition(s.Clip, 1f));
    }

    public void SetChannelVolume(string mixerChannel, float linearVolume) // valor 0.0 a 1.0
    {
        float volumeInDb = Mathf.Log10(Mathf.Clamp(linearVolume, 0.0001f, 1f)) * 20f;
        mixer.SetFloat(mixerChannel, volumeInDb);
    }
    public float GetChannelVolume(string mixerChannel)
    {
        float volumeInDb;
        mixer.GetFloat(mixerChannel, out volumeInDb);
        float linearVolume = Mathf.Pow(10f, volumeInDb / 20f);
        return linearVolume;
    }

    // --- Coroutines ---
    IEnumerator Transition(AudioClip song, float time)
    {
        float halfTime = time / 2f;
        float startVolume = _musicSource.volume;

        float t = 0;
        while (t < halfTime)
        {
            float step = t / halfTime;
            t += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(startVolume, 0, step);
            yield return null;
        }
        _musicSource.volume = 0;

        _musicSource.clip = song;
        _musicSource.Play();

        t = 0;
        while (t < halfTime)
        {
            float step1 = t / halfTime;
            t += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(0, startVolume, step1);
            yield return null;
        }
        _musicSource.volume = 1f;
    }
}
