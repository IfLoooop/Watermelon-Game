using UnityEngine;

namespace Watermelon_Game.Singletons
{
    /// <summary>
    /// Automatically creates singleton instance for the given type parameter <br/>
    /// <i>Also calls <see cref="Object.DontDestroyOnLoad"/> on the singleton instance</i>
    /// </summary>
    /// <typeparam name="T"><see cref="PersistantMonoBehaviour{T}"/></typeparam>
    internal abstract class PersistantMonoBehaviour<T> : MonoBehaviour where T : PersistantMonoBehaviour<T>
    {
        #region Fields
        /// <summary>
        /// Singleton
        /// </summary>
        protected static T Instance;
        #endregion

        #region Methods
        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Destroy(base.gameObject);
                return;
            }
            
            Instance = this as T;
            DontDestroyOnLoad(Instance);
            this.Init();
        }
        
        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Will only be called once during <see cref="Awake"/>, when <see cref="Instance"/> is being initialized the first time
        /// </summary>
        protected virtual void Init() { }
        #endregion
    }
}