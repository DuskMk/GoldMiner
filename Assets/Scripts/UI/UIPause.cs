using UnityEditor;
using UnityEngine;

public class UIPause : UIWindow
{
    protected override void OnEnable()
    {
        base.OnEnable();
        GameManager.Instance.PauseGame();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        if(GameManager.Instance) GameManager.Instance.PauseGame();
    }
    public void OnClickContinue()
    {
        OnClickClose();
    }
    public void OnClickRestartThisLevel()
    {
        OnClickClose();
        GameManager.Instance.StartReady(emLoadLevelType.LoadCurrentLevel);
    }
    public void OnClickRestartFromLevel1()
    {
        OnClickClose();
        GameManager.Instance.StartReady(emLoadLevelType.LoadFirstLevel);
    }
    public void OnClickSettig()
    {
        UIManager.Instance.Show<UISetting>();
    }
    public void OnClickExit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
