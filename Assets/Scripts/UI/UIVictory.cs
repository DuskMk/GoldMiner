using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIVictory :UIWindow, IPointerClickHandler
{
    public TMP_Text textTotalScore;
    public TMP_Text textTip;
    string textv1 = "点击任意位置进入下一关";
    string textv2 = "恭喜你，所有关卡已完成！";

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("点击了UIVictory面板");

        OnClickClose();
        GameManager.Instance.StartReady();
    }

    //
    public void SetInfo(int totalScore, bool isEnd = false)
    {
        textTotalScore.text = $"总计得分:{totalScore}";
        if (isEnd)
        {
            textTip.text = textv2;
        }
        else
        {
            textTip.text = textv1;
        }
    }
}
