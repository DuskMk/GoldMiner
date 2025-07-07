using UnityEngine.UI;

public class UIStart : UIWindow
{
    protected override void OnEnable()
    {
        base.OnEnable();
        SoundManager.Instance.PlayMusic(SoundDefine.BGM_Main);

    }
    public override void OnClickClose()
    {
        base.OnClickClose();
        GameManager.Instance.StartReady(emLoadLevelType.LoadFirstLevel);
    }
    public void OnClickSettig()
    {
        UIManager.Instance.Show<UISetting>();
    }
}
