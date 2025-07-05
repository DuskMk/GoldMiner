
public class UIFailure : UIWindow
{
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
