using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public bool global = true; //控制单例是否在场景加载后保留
    static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType<T>();
            }
            return instance;
        }
    }

    void Start()
    {
        if (global)
        {   //如果场景内已经存在一个这样的单例，并且和当前脚本不是同一个(说明当前脚本是重复出现的多余脚本，需要销毁)
            //instance是静态变量，全局共享，所以多余的脚本在可以Start中通过instance访问到之前已经有的单例，从而知道是该脚本多余的
            if (instance != null && instance != this.gameObject.GetComponent<T>())
            {   //销毁
                Destroy(this.gameObject);
                //返回，不再执行之后的语句
                return;
            }
            DontDestroyOnLoad(this.gameObject);
            //如果不存在，则将自己设为单例
            instance = this.gameObject.GetComponent<T>();
        }

        this.OnStart();
    }
    protected virtual void OnStart()
    {

    }
}

