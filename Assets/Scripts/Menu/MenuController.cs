using JetBrains.Annotations;
using UnityEngine;
using Watermelon_Game.Points;

namespace Watermelon_Game.Menu
{
    internal sealed class MenuController : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private StatsMenu statsMenu;
        [SerializeField] private GameOverMenu gameOverMenu;
        [SerializeField] private ExitMenu exitMenu;
        [SerializeField] private float audioClipStartTime = .1f;
        #endregion

        #region Fields
        [CanBeNull] private MenuBase currentActiveMenu;
        #endregion

        #region Properties
        public static MenuController Instance { get; private set; }
        public AudioSource AudioSource { get; private set; }
        public float AudioClipStartTime => this.audioClipStartTime;
        #endregion
        
        #region Methods
        private void Awake()
        {
            Instance = this;

            this.AudioSource = this.GetComponent<AudioSource>();
            this.InitializeMenu(this.statsMenu);
            this.InitializeMenu(this.gameOverMenu);
            this.InitializeMenu(this.exitMenu);
        }
        
        // Won't get called in "StatsMenu.cs" for some reason
        private void OnApplicationQuit()
        {
            StatsMenu.Instance.NewBestScore(PointsController.Instance.CurrentPoints);
            StatsMenu.Instance.Save();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (this.currentActiveMenu is { Menu: Menu.Exit })
                {
                    this.exitMenu.ExitGame();
                }
                if (this.currentActiveMenu == null)
                {
                    this.Open_CloseMenu(this.exitMenu);
                }
                else
                {
                    this.Open_CloseMenu(this.currentActiveMenu, true);
                }
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                this.Open_CloseMenu(this.statsMenu);
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                if (this.currentActiveMenu is { Menu: Menu.Exit })
                {
                    this.Open_CloseMenu(this.exitMenu);
                }
            }
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
        
        private void Open_CloseMenu(MenuBase _Menu, bool _ForceClose = false)
        {
            this.currentActiveMenu = _Menu.Open_Close(this.currentActiveMenu, _ForceClose);
        }

        public void GameOver()
        {
            this.Open_CloseMenu(this.gameOverMenu);
        }
        #endregion
    }
}