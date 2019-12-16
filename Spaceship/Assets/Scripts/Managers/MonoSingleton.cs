using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        static private T instance = null;

        private bool stayalive = false;
        private bool isInitialized = false;
        static public T Instance
        {
            get
            {
                if (MonoSingleton<T>.instance == null)
                {
                    MonoSingleton<T>.instance = FindObjectOfType<T>();
                    if (MonoSingleton<T>.instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).Name);
                        MonoSingleton<T>.instance = go.AddComponent<T>();
                    }
                }
                if(MonoSingleton<T>.instance.isInitialized == false)
                {
                    MonoSingleton<T>.instance.OnInitialize();
                }
                GameObject parent = GameObject.Find("Managers");
                if (parent == null)
                {
                    parent = new GameObject("Managers");
                    DontDestroyOnLoad(parent);
                }
                if (MonoSingleton<T>.instance.StayAlive == true)
                {
                    MonoSingleton<T>.instance.gameObject.transform.SetParent(parent.transform);
                }

                return MonoSingleton<T>.instance;
            }
        }

        public virtual void OnInitialize()
        {
            isInitialized = true;
        }

        public bool StayAlive
        {
            get { return this.stayalive; }
            set { this.stayalive = value; }
        }

        public static bool Exists { get { return MonoSingleton<T>.Instance != null; } }
    }
