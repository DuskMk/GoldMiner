using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    //������һ����Ϊ UIElement ���ڲ��࣬���ڴ洢 UI ��Դ����Ϣ��
    class UIElement
    {
        //��Դ·��
        public string Resources;
        //�Ƿ񻺴�(����ر������ػ���ֱ������)
        public bool Cache;
        //����Ϊtrueʱ������Instance,���ڴ洢����� UI ��Դ��ʵ����
        public GameObject Instance;
    }

    //�ֵ䣬�洢�����Ѷ���� UI ��Դ��Ϣ
    private Dictionary<Type, UIElement> UIResources = new Dictionary<Type, UIElement>();

    //���캯�����Ա�һ��ʼ��ִ��,���ڳ�ʼ�� UI ��Դ�ֵ䡣
    public UIManager()
    {   //���ֵ������һ�� UI ��Դ��Ϣ������Ϊ UITest����Դ·��Ϊ UI/UITest�������� Cache Ϊ true����ʾ��Ҫ���档
        //this.UIResources.Add(typeof(UIBag), new UIElement() { Resources = "UI/Bag/UIBag", Cache = false });//false���Ա�ÿ�����ɶ�ִ��һ��Start������ˢ�±�����Ʒ
        
    }
    ~UIManager() { }

    /// <summary>
    /// Show UI�����ͷ�����������ʾָ���� UI, ͨ����UIMain������UI����������
    /// </summary>
    public T Show<T>()
    {   //��������
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Win_Open);

        //��ȡ UI �����ͣ�����Type����type�У��Ա����ֵ�
        Type type = typeof(T);
        // ��� UI ��Դ�ֵ����Ƿ���������͵� UI ��Դ��
        if (this.UIResources.ContainsKey(type))
        {   //������������ȡ UI ��Դ��Ϣ�����ж��Ƿ���Ҫ��������ʾ UI ʵ��
            UIElement info = this.UIResources[type];
            if(info.Instance != null)
            {   //������� UI ʵ�����򼤻�
                info.Instance.SetActive(true);
            }
            else
            {   //�����ڸ�ʵ������ظ�UIԤ����
                UnityEngine.Object prefab = Resources.Load(info.Resources);
                if (prefab == null)
                {   //Ϊ��˵��û�и�Ԥ���壬�������� T ��Ĭ��ֵ��ͨ��Ϊ null������ִ��֮������
                    return default(T);
                }
                //���سɹ���ʵ������UIԤ���壬����Instance�С�
                info.Instance = (GameObject)GameObject.Instantiate(prefab);
            }
            //��UIʵ���ϻ�ȡָ������ T ������������䷵��
            return info.Instance.GetComponent<T>();
        }
        //���޷���ȡ UI ʵ���������޷��� UI ʵ���ϻ�ȡָ�����͵����ʱ���򷵻����� T ��Ĭ��ֵ��ͨ��Ϊ null��
        return default(T);
    }

    /// <summary>
    /// �ر� UI ������ͨ����UIWindow����
    /// </summary>
    public void Close(Type type, bool isplaysound = true)
    {
        //��������
        if(isplaysound) SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Win_Close);

        if (this.UIResources.ContainsKey(type))
        {   //���ֵ��ȡUI��Ϣ
            UIElement info = this.UIResources[type];
            if(info.Cache)
            {   //CacheΪtrue������
                info.Instance.SetActive(false);
            }
            else
            {   //CacheΪfalse�����٣������¸�UI��Ϣ��Instance�ļ�¼
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
