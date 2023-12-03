using System.Globalization;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Background;

namespace Watermelon_Game.Menus.MenuContainers
{
    /// <summary>
    /// TODO
    /// </summary>
    internal sealed class Controls : ContainerMenuBase
    {
        #region Inspector Fields
        [Tooltip("Slider that sets the background fruit frequency")]
        [PropertyOrder(1)][SerializeField] private Slider slider;
        #endregion
        
        #region Constants
        /// <summary>
        /// PlayerPrefs key for the <see cref="Slider.value"/> of the <see cref="slider"/>
        /// </summary>
        private const string SLIDER_VALUE = "BackgroundFruitSliderValue";
        #endregion
        
        #region Methods
        private void Start()
        {
            this.LoadSettings();
        }

        private void OnDisable()
        {
            this.SaveSettings();
        }
        
        /// <summary>
        /// Saves the <see cref="Slider.value"/> for <see cref="slider"/>
        /// </summary>
        private void SaveSettings()
        {
            PlayerPrefs.SetString(SLIDER_VALUE, slider.value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Loads the <see cref="Slider.value"/> for <see cref="slider"/>
        /// </summary>
        private void LoadSettings()
        {
            this.slider.value = float.Parse(PlayerPrefs.GetString(SLIDER_VALUE, .375f.ToString(CultureInfo.InvariantCulture)));
            this.SetFruitSpawnDelay();
        }
        
        /// <summary>
        /// Sets the delay of the fruits in <see cref="BackgroundFruitController"/>
        /// </summary>
        public void SetFruitSpawnDelay()
        {
            BackgroundFruitController.SetDelay(this.slider.value);
        }
        #endregion
    }
}