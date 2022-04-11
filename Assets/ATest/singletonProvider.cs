using UnityEngine;
using System.Collections;


public class singleton<T>  where T : new()
    {
        static readonly object padlock = new object();
        private static T _Instance;
        public static T Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (padlock)
                    {
                        if(_Instance == null)
                        {
                            _Instance = new T();
                        }
                    }
                }
                return _Instance;
            }
        }
    }

