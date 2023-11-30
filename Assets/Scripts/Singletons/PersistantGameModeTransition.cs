using UnityEngine;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Singletons
{
    /// <summary>
    /// Automatically creates singleton instance for the given type parameter <br/>
    /// <i>Also calls <see cref="Object.DontDestroyOnLoad"/> on the singleton instance</i>
    /// </summary>
    /// <typeparam name="T"><see cref="PersistantGameModeTransition{T}"/></typeparam>
    internal abstract class PersistantGameModeTransition<T> : GameModeTransition where T : PersistantGameModeTransition<T>
    {
        #region Fields
        /// <summary>
        /// Singleton
        /// </summary>
        protected static T Instance;
        #endregion

        #region Methods
        protected override void Awake()
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
        /// Will only be called once during <see cref="Awake"/>, when <see cref="Instance"/> is being initialized the first time <br/>
        /// <b>Use this method instead of <see cref="Awake"/></b>
        /// </summary>
        protected virtual void Init()
        {
            base.Awake();
        }
        #endregion
    }
}