using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIReady: UIWindow, IPointerClickHandler
{
    public TMP_Text textTraget; 
    public TMP_Text textTime;
    public void OnPointerClick(PointerEventData eventData)
    {
        // 处理点击事件
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 左键点击时执行的逻辑
            Debug.Log("左键点击了UIReady面板");
            GameManager.Instance.StartGame();
            OnClickClose();
        }
    }
    public void SetInfo(int targetScore, int time)
    {
        textTraget.text = $"本关目标:{targetScore}";
        textTime.text = $"本关时间:{time}秒";
    }
}

