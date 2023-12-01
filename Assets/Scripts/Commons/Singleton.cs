using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public bool IsPersistence;
    protected static T m_Instance;

    public static T Instance
    {
        get
        {
            return Singleton<T>.m_Instance;
        }
        set
        {
            m_Instance = value;
        }
    }

    protected virtual void Awake()
    {
        if (this.IsPersistence)
        {
            if (object.ReferenceEquals((object)Singleton<T>.m_Instance, (object)null))
            {
                Singleton<T>.m_Instance = (object)this as T;
                Object.DontDestroyOnLoad((Object)this.gameObject);
            }
            else
            {
                if (object.ReferenceEquals((object)Singleton<T>.m_Instance, (object)((object)this as T)))
                    return;

                Object.Destroy((Object)this.gameObject);
            }
        }
        else
            Singleton<T>.m_Instance = (object)this as T;
    }

    protected virtual void OnDestroy()
    {
        Singleton<T>.m_Instance = (T)null;
    }
}