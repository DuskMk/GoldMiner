
using UnityEngine;
using UnityEngine.UI;

internal class UISetting: UIWindow
{
    public Image musicOff;
    public Image soundOff;

    public Toggle toggleMusic;
    public Toggle toggleSound;

    public Slider sliderMusic;  //范围设为0-100，以便SoundManager设置
    public Slider sliderSound;
    private bool canPlaySound = false;

    private void Start()
    {
        this.toggleMusic.isOn = Config.MusicOn;
        this.toggleSound.isOn = Config.SoundOn;
        this.sliderMusic.value = Config.MusicVolume;
        this.sliderSound.value = Config.SoundVolume;
        this.toggleMusic.onValueChanged.AddListener(this.MusicToggle);
        this.toggleSound.onValueChanged.AddListener(this.SoundToogle);
        canPlaySound = true;

    }
    public override void OnClickYes()
    {
        base.OnClickYes();
        if (canPlaySound) SoundManager.Instance.PlaySound(SoundDefine.SFX_Normal);
        PlayerPrefs.Save();
    }
    /// <summary>
    /// 音乐开关；绑定Toggle，点击Toggle，调用Config的属性MusicOn赋值on，Config的属性MusicOn 调用SoundManager的 属性MusicOn 赋值on，SoundManager的属性MusicOn调用MusicMute传递参数on，MusicMute参数on转化为0或SoundVolume（当前音量），调用SetVolume，SetVolume根据传递的参数0或SoundVolume调用SoundManager挂载的AudioMixer设置音量
    /// </summary>
    /// <param name="on"></param>
    public void MusicToggle(bool on)
    {
        musicOff.enabled = !on;
        Config.MusicOn = on;
        if (canPlaySound) SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_other);
    }
    //音效开关
    public void SoundToogle(bool on)
    {
        soundOff.enabled = !on;
        Config.SoundOn = on;
        if (canPlaySound) SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_other);
    }
    /// <summary>
    /// 滑动条设置音量。绑定Slider，根据滑动条传递的值vol设置Config的属性MusicVolume，Config的属性MusicVolume设置SoundManager的属性MusicVolume，SoundManager的属性MusicVolume调用SetVolume传递vol，SetVolume调用SoundManager挂载的AudioMixer设置音量
    /// </summary>
    /// <param name="vol"></param>
    public void MusicVolume(float vol)
    {
        Config.MusicVolume = (int)vol;
        //PlaySound();    //移动滑动条时播放声音
    }
    public void SoundVolume(float vol)
    {
        Config.SoundVolume = (int)vol;
        if (canPlaySound) PlaySound();
    }

    float lastPlay = 0;
    private void PlaySound()
    {
        if (Time.realtimeSinceStartup - lastPlay > 0.1)
        {
            lastPlay = Time.realtimeSinceStartup;
            if (canPlaySound) SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_other);
        }
    }
}
