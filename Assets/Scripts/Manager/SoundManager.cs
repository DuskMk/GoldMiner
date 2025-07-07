using UnityEngine.Audio;
using UnityEngine;


public class SoundManager : MonoSingleton<SoundManager>
{
    public string music_AudioMixerGroupName = "MusicVolume";
    public string sound_AudioMixerGroupName = "SoundVolume";
    public AudioMixer audioMixer;
    public AudioSource musicAidioSource;
    public AudioSource soundAudioSource;

    const string MusicPath = "Music/";
    const string SoundPath = "Sound/";

    bool musicOn;
    public bool MusicOn
    {
        get { return musicOn; }
        set
        {
            musicOn = value;
            this.MusicMute(!musicOn);
        }
    }
    bool soundOn;
    public bool SoundOn
    {
        get { return soundOn; }
        set
        {
            soundOn = value;
            this.SoundMute(!soundOn);
        }
    }

    int musicVolume;
    public int MusicVolume
    {
        get { return musicVolume; }
        set
        {
            musicVolume = value;
            if (musicOn) this.SetVolume(music_AudioMixerGroupName, musicVolume);
        }
    }

    int soundVolume;
    public int SoundVolume
    {
        get { return soundVolume; }
        set
        {
            soundVolume = value;
            if (soundOn) this.SetVolume(sound_AudioMixerGroupName, soundVolume);
        }
    }
    protected override void OnStart()
    {
        this.MusicVolume = Config.MusicVolume;
        this.SoundVolume = Config.SoundVolume;
        this.MusicOn = Config.MusicOn;
        this.SoundOn = Config.SoundOn;
    }

    /// <summary>
    /// 音乐静音
    /// </summary>
    /// <param name="mute"></param>
    private void MusicMute(bool mute)
    {
        this.SetVolume("MusicVolume", mute ? 0 : MusicVolume);
    }
    /// <summary>
    /// 音效静音
    /// </summary>
    /// <param name="mute"></param>
    private void SoundMute(bool mute)
    {
        this.SetVolume("SoundVolume", mute ? 0 : SoundVolume);
    }
    /// <summary>
    /// 设置音量 ，通过AudioMixer设置name对应的音量，AudioMixer
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value">范围0-100</param>
    private void SetVolume(string name, int value)
    {
        float volume = value * 0.5f - 50f;
        this.audioMixer.SetFloat(name, volume);
    }

    /// <summary>
    /// 播放音乐
    /// </summary>
    /// <param name="musicName"></param>
    internal void PlayMusic(string musicName)
    {
        AudioClip clip = ResLoader.Load<AudioClip>(MusicPath + musicName);
        if (clip == null)
        {
            Debug.LogWarningFormat("PlayMusic: {0} not existed", musicName);
            return;
        }
        if (musicAidioSource.clip == clip)
        {
            if (!musicAidioSource.isPlaying)
            {
                musicAidioSource.Play();
            }
            return;
        }
        
        musicAidioSource.clip = clip;
        musicAidioSource.Play();
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="soundName"></param>
    internal void PlaySound(string soundName)
    {
        AudioClip clip = ResLoader.Load<AudioClip>(SoundPath + soundName);
        if (clip == null)
        {
            Debug.LogWarningFormat("PlaySound: {0} not existed", soundName);
            return;
        }
        soundAudioSource.PlayOneShot(clip);
    }

    public void StopMusic()
    {
        if (musicAidioSource.isPlaying)
        {
            musicAidioSource.Stop();
        }
    }
    public AudioMixerGroup[] GetSoundGroup()
    {
        return this.audioMixer.FindMatchingGroups(sound_AudioMixerGroupName);
    }
    public AudioMixerGroup[] GetMusicGroup()
    {
        return this.audioMixer.FindMatchingGroups(music_AudioMixerGroupName);
    }

}

