using UnityEngine;
using UnityEngine.Events;


public class Config
{
    public static bool isCheckTankByRoot = true;
    //internal static emInputStatus AIInputStatus;
    public static UnityAction OnAllowControlModeChanged;
    public static bool MusicOn
    {
        get
        {
            return PlayerPrefs.GetInt("Music", 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("Music", value ? 1 : 0);
            SoundManager.Instance.MusicOn = value;
        }
    }

    public static bool SoundOn
    {
        get
        {
            return PlayerPrefs.GetInt("Sound", 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("Sound", value ? 1 : 0);
            SoundManager.Instance.SoundOn = value;
        }
    }

    public static int MusicVolume
    {
        get
        {
            return PlayerPrefs.GetInt("MusicVolume", 100);
        }
        set
        {
            PlayerPrefs.SetInt("MusicVolume", value);
            SoundManager.Instance.MusicVolume = value;
        }
    }

    public static int SoundVolume
    {
        get
        {
            return PlayerPrefs.GetInt("SoundVolume", 100);
        }
        set
        {
            PlayerPrefs.SetInt("SoundVolume", value);
            SoundManager.Instance.SoundVolume = value;
        }
    }

    public static string UserName
    {
        get
        {
            return PlayerPrefs.GetString("UserName", "");
        }
        set
        {
            PlayerPrefs.SetString("UserName", value);
        }
    }

    public static string UserPass
    {
        get
        {
            return PlayerPrefs.GetString("UserPass", "");
        }
        set
        {
            PlayerPrefs.SetString("UserPass", value);
        }
    }
    public static int CurrentSaveSlot
    {
        get
        {
            return PlayerPrefs.GetInt("CurrentSaveSlot", 3);
        }
        set
        {
            PlayerPrefs.SetInt("CurrentSaveSlot", value);
        }
    }
    public static bool AllowControlTakeover // 是否允许玩家接管其他坦克 (作弊模式开关)
    {
        get
        {
            return PlayerPrefs.GetInt("AllowControlTakeover", 0) == 1;
        }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt("AllowControlTakeover", 1);
            }
            else
            {
                PlayerPrefs.SetInt("AllowControlTakeover", 0);
            }
        }
    }
    public static bool SpectateNextOnDeath // 观战/接管的坦克死亡时是否自动切换到下一个
    {
        get
        {
            return PlayerPrefs.GetInt("SpectateNextOnDeath", 0) == 1;
        }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt("SpectateNextOnDeath", 1);
            }
            else
            {
                PlayerPrefs.SetInt("SpectateNextOnDeath", 0);
            }
            OnAllowControlModeChanged?.Invoke();
        }
    }
    public static bool AllowSpectateEnemy // 是否允许玩家观战敌方
    {
        get
        {
            return PlayerPrefs.GetInt("AllowSpectateEnemy", 0) == 1;
        }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt("AllowSpectateEnemy", 1);
            }
            else
            {
                PlayerPrefs.SetInt("AllowSpectateEnemy", 0);
            }
        }
    }
    public static int PreferredTurretMode
    {
        get { return PlayerPrefs.GetInt("PreferredTurretMode", 0); }
        set
        {
            if (value < 0 || value > 2)
            {
                Debug.LogError("Invalid turret mode value. Must be between 0 and 2.");
                return;
            }
            PlayerPrefs.SetInt("PreferredTurretMode", value);
            PlayerPrefs.Save();
        }
    }
    public static int ResolutionWidth
    {
        get { return PlayerPrefs.GetInt("ResolutionWidth", Screen.currentResolution.width); }
        set
        {
            PlayerPrefs.SetInt("ResolutionWidth", value);
        }
    }
    public static int ResolutionHeight
    {
        get { return PlayerPrefs.GetInt("ResolutionHeight", Screen.currentResolution.height); }
        set
        {
            PlayerPrefs.SetInt("ResolutionHeight", value);
        }
    }
    public static int FullScreen
    {
        get { return PlayerPrefs.GetInt("FullScreenMode", 0); }
        set
        {
            PlayerPrefs.SetInt("FullScreenMode", value);
        }
    }
    ~Config()
    {
        PlayerPrefs.Save();
    }
}

