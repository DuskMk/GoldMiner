using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIReady: UIWindow, IPointerClickHandler
{
    public TMP_Text textTraget; 
    public TMP_Text textTime;
    public void OnPointerClick(PointerEventData eventData)
    {
        // 点击时执行的逻辑
        Debug.Log("点击了UIReady面板");
        GameManager.Instance.StartGame();
        OnClickClose();
    }
    public void SetInfo(int targetScore, int time, int level)
    {
        textTraget.text = $"第{level}关目标:{targetScore}";
        textTime.text = $"本关时间:{time}秒";
    }
}

