
public class UIFailure : UIWindow
{
    protected override void OnEnable()
    {
        base.OnEnable();
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_End);
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
}
