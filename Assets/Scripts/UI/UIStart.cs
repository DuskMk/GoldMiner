using UnityEngine.UI;

public class UIStart : UIWindow
{
    public override void OnClickClose()
    {
        base.OnClickClose();
        GameManager.Instance.StartReady();
    }
}
