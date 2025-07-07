using TMPro;


public class UIVictory :UIWindow
{
    public TMP_Text textTotalScore;
    //public TMP_Text textTip;
    protected override void OnEnable()
    {
        base.OnEnable();
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_End);
    }
    public void OnNextLevelButtonClick()
    {
        OnClickClose();
        GameManager.Instance.StartReady(emLoadLevelType.LoadNextLevel);
    }

    public void OnShopButtonClick()
    {
        OnClickClose();
        GameManager.Instance.CurrentGameState = GameManager.GameState.Store;
    }
    
    public void SetInfo(int totalScore)
    {
        textTotalScore.text = $"总计得分:{totalScore}";
    }
}
