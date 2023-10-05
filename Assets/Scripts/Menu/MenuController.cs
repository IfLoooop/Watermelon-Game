using JetBrains.Annotations;
using UnityEngine;

namespace Watermelon_Game.Menu
{
    internal sealed class MenuController : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private StatsMenu statsMenu;
        [SerializeField] private GameOverMenu gameOverMenu;
        [SerializeField] private float audioClipStartTime = .1f;
        #endregion

        #region Fields
        private AudioSource audioSource;
        [CanBeNull] private MenuBase previousActiveMenu;
        #endregion

        #region Properties
        public static MenuController Instance { get; private set; }

        public AudioSource AudioSource => this.audioSource;
        public float AudioClipStartTime => this.audioClipStartTime;
        #endregion
        
        #region Methods
        private void Awake()
        {
            Instance = this;

            this.audioSource = this.GetComponent<AudioSource>();
            this.InitializeMenu(this.statsMenu);
            this.InitializeMenu(this.gameOverMenu);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                this.OpenMenu(this.statsMenu);
            }
            if (this.previousActiveMenu is { Menu: Menu.GameOver } && Input.GetKeyDown(KeyCode.Escape))
            {
                this.OpenMenu(this.gameOverMenu);
            }
        }

        private void OnApplicationQuit()
        {
            this.statsMenu.Save();
        }

        /// <summary>
        /// Needed to set the "Instance" properties in the Menu class
        /// </summary>
        /// <param name="_Menu">The menu to initialize</param>
        private void InitializeMenu(MenuBase _Menu)
        {
            _Menu.gameObject.SetActive(true);
            _Menu.gameObject.SetActive(false);
        }
        
        private void OpenMenu(MenuBase _Menu)
        {
            this.previousActiveMenu = _Menu.Open_Close(this.previousActiveMenu);
        }

        public void GameOver()
        {
            this.OpenMenu(this.gameOverMenu);
        }
        #endregion
    }
}