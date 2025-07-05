using UnityEngine;
using UnityEngine.EventSystems;

public class UICompleted:UIWindow, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("点击了UICompleted面板");
        OnClickClose();
        GameManager.Instance.StartReady(emLoadLevelType.LoadFirstLevel);
    }
}

