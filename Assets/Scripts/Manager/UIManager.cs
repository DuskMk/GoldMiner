using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    //定义了一个名为 UIElement 的内部类，用于存储 UI 资源的信息。
    class UIElement
    {
        //资源路径
        public string Resources;
        //是否缓存(点击关闭是隐藏还是直接销毁)
        public bool Cache;
        //缓存为true时储存在Instance,用于存储缓存的 UI 资源的实例。
        public GameObject Instance;
    }

    //字典，存储所有已定义的 UI 资源信息
    private Dictionary<Type, UIElement> UIResources = new Dictionary<Type, UIElement>();

    //构造函数，以便一开始就执行,用于初始化 UI 资源字典。
    public UIManager()
    {   //在字典中添加一个 UI 资源信息，类型为 UITest，资源路径为 UI/UITest，并设置 Cache 为 true，表示需要缓存。
        //this.UIResources.Add(typeof(UIBag), new UIElement() { Resources = "UI/Bag/UIBag", Cache = false });//false，以便每次生成都执行一次Start方法，刷新背包物品
        
    }
    ~UIManager() { }

    /// <summary>
    /// Show UI，泛型方法，用于显示指定的 UI, 通常由UIMain和其他UI管理器调用
    /// </summary>
    public T Show<T>()
    {   //播放声音
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Win_Open);

        //获取 UI 的类型，存在Type变量type中，以便检查字典
        Type type = typeof(T);
        // 检查 UI 资源字典中是否包含该类型的 UI 资源。
        if (this.UIResources.ContainsKey(type))
        {   //如果包含，则获取 UI 资源信息，并判断是否需要创建或显示 UI 实例
            UIElement info = this.UIResources[type];
            if(info.Instance != null)
            {   //如果存在 UI 实例，则激活
                info.Instance.SetActive(true);
            }
            else
            {   //不存在该实例则加载该UI预制体
                UnityEngine.Object prefab = Resources.Load(info.Resources);
                if (prefab == null)
                {   //为空说明没有该预制体，返回类型 T 的默认值，通常为 null，不再执行之后的语句
                    return default(T);
                }
                //加载成功则实例化该UI预制体，存在Instance中。
                info.Instance = (GameObject)GameObject.Instantiate(prefab);
            }
            //从UI实例上获取指定类型 T 的组件，并将其返回
            return info.Instance.GetComponent<T>();
        }
        //当无法获取 UI 实例，或者无法从 UI 实例上获取指定类型的组件时，则返回类型 T 的默认值，通常为 null。
        return default(T);
    }

    /// <summary>
    /// 关闭 UI 方法，通常由UIWindow调用
    /// </summary>
    public void Close(Type type, bool isplaysound = true)
    {
        //播放声音
        if(isplaysound) SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Win_Close);

        if (this.UIResources.ContainsKey(type))
        {   //从字典获取UI信息
            UIElement info = this.UIResources[type];
            if(info.Cache)
            {   //Cache为true则隐藏
                info.Instance.SetActive(false);
            }
            else
            {   //Cache为false则销毁，并更新该UI信息的Instance的记录
                GameObject.Destroy(info.Instance);
                info.Instance = null;
            }
        }
    }

    public void Close<T>(bool isplaysound = true)
    {
        this.Close(typeof(T), isplaysound);
    }
}
