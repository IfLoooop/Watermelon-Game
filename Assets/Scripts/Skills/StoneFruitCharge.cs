using System;
using System.Collections;
using JetBrains.Annotations;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Fruits;
using Watermelon_Game.Networking;
using Watermelon_Game.Singletons;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Skills
{
    /// <summary>
    /// Controls the charge bar for the stone fruit in multiplayer
    /// </summary>
    internal sealed class StoneFruitCharge : PersistantGameModeTransition<StoneFruitCharge>
    {
        #region Inspector Fields
        [Header("References")]
        [ChildGameObjectsOnly][Tooltip("Reference to the network logic")]
        [SerializeField] private NetworkStoneFruitCharge networkStoneFruitCharge;
        [ChildGameObjectsOnly][Tooltip("Reference to the network logic")]
        [SerializeField] private GameObject controls;
        [ChildGameObjectsOnly][Tooltip("Every possible fill color")]
        [SerializeField] private Image[] fillArray;

        [Header("Settings")]
        [Tooltip("Multiplier for the force, the stone fruit is shot with")]
        [SerializeField] private ProtectedFloat shootForceMultiplier;
        
        [Tooltip("Value with which the fill grows, with each step")]
        [SerializeField] private ProtectedFloat fillStep = .0125f;
        [Tooltip("Multiplier for how much a fruit evolution increases the fill")]
        [SerializeField] private ProtectedFloat fruitMultiplier = .01f;
        [Tooltip("Decreases the value that is added to the fill. the higher the fill")]
        [SerializeField] private AnimationCurve fillDecrease;
        
        [Header("Debug")]
        [Tooltip("Index in fillArray that is currently being used")]
        [SerializeField][ReadOnly] private ProtectedInt32 currentFillArrayIndex;
        [Tooltip("Sum of the fill amount of all elements in fillArray")]
        [SerializeField][ReadOnly] private ProtectedFloat currentFillAmount;
        [Tooltip("Can be a value from 0 - fillArray.Length")]
        [ShowInInspector][ReadOnly]private ProtectedFloat TargetFillAmount
        {
            get => this.targetFillAmount;
            set => this.targetFillAmount = Mathf.Clamp(value, 0, this.fillArray.Length);
        }
#if UNITY_EDITOR
        // ReSharper disable NotAccessedField.Local
        [HorizontalGroup("Fruit", Width = 100, Order = 1)][HideLabel]
        [SerializeField][ReadOnly] private Fruit fruit;
        [HorizontalGroup("Fruit")][HideLabel]
        [SerializeField][ReadOnly] private float fill;  
        // ReSharper restore NotAccessedField.Local
#endif
        #endregion

        #region Fields
        /// <summary>
        /// <see cref="TargetFillAmount"/>
        /// </summary>
        private ProtectedFloat targetFillAmount;
        /// <summary>
        /// Stores the <see cref="SmoothFill"/> coroutine
        /// </summary>
        [CanBeNull] private IEnumerator smoothFill;
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="shootForceMultiplier"/>
        /// </summary>
        public ProtectedFloat ShootForceMultiplier => this.shootForceMultiplier;
        /// <summary>
        /// The maximum value in <see cref="Fruit"/> for a stone fruit
        /// </summary>
        public ProtectedInt32 MaxFruitValue => this.fillArray.Length - 1;
        #endregion
        
        #region Methods
        protected override void OnEnable()
        {
            base.OnEnable();
            FruitController.OnEvolve += AddFill;
            GameController.OnResetGameStarted += Reset;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            FruitController.OnEvolve -= AddFill;
            GameController.OnResetGameStarted -= Reset;
        }

        private void Update() // TODO: Needs InputController
        {
            if (GameController.CurrentGameMode == GameMode.MultiPlayer && GameController.ActiveGame)
            {
                if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Mouse2))
                {
                    this.SpawnStoneFruit();
                }   
            }
        }

        /// <summary>
        /// Increases the <see cref="Image.fillAmount"/> based on the given <see cref="Fruit"/>
        /// </summary>
        /// <param name="_Fruit"><see cref="Fruit"/></param>
        [Button][PropertyOrder(2)]
        private void AddFill(Fruit _Fruit)
        {
            if (GameController.CurrentGameMode == GameMode.MultiPlayer)
            {
#if UNITY_EDITOR
                this.fruit = _Fruit;
#endif
                this.SetTargetFill((float)_Fruit + 1);   
            }
        }

        /// <summary>
        /// Subtracts the given value from the <see cref="Image.fillAmount"/>
        /// </summary>
        /// <param name="_Value">Should be a positive value</param>
        [Button][PropertyOrder(3)]
        private void SubtractFill(float _Value)
        {
            if (GameController.CurrentGameMode == GameMode.MultiPlayer)
            {
                this.SetTargetFill(-_Value);   
            }
        }

        /// <summary>
        /// Increases/decreases the <see cref="Image.fillAmount"/> based ón the given value <br/>
        /// <i>Positive increases, negative decreases</i>
        /// </summary>
        /// <param name="_Value">Value to increase/decrease the <see cref="Image.fillAmount"/> with</param>
        private void SetTargetFill(float _Value)
        {
            this.TargetFillAmount += this.GetFillValue(_Value);
            
            if (this.smoothFill == null)
            {
                this.smoothFill = this.SmoothFill();
                base.StartCoroutine(this.smoothFill);
            }
        }

        /// <summary>
        /// Decreases the given value (when it's positive), based on the value of <see cref="TargetFillAmount"/> <br/>
        /// <i>Higher <see cref="TargetFillAmount"/> = higher decrease</i>
        /// </summary>
        /// <param name="_Value">The value to decrease</param>
        /// <returns>If positive, the decreased value, if negative, returns the given value</returns>
        private float GetFillValue(float _Value)
        {
            if (_Value > 0)
            {
#if UNITY_EDITOR
                this.fill = _Value * this.fruitMultiplier * this.fillDecrease.Evaluate(this.targetFillAmount);
#endif
                return _Value * this.fruitMultiplier * this.fillDecrease.Evaluate(this.targetFillAmount);
            }

            return _Value;
        }
        
        /// <summary>
        /// Smoothly sets the <see cref="Image.fillAmount"/> of the elements in <see cref="fillArray"/>, until <see cref="currentFillAmount"/> == <see cref="TargetFillAmount"/>
        /// </summary>
        /// <returns></returns>
        private IEnumerator SmoothFill()
        {
            var _waitTime = new WaitForSeconds(.01f);
            this.currentFillAmount = this.GetCurrentFillAmount();
            
            while (Math.Abs(this.currentFillAmount - this.TargetFillAmount) > .025f)
            {
                if (this.TargetFillAmount > this.currentFillAmount)
                {
                    this.currentFillAmount = this.SetFill(Operation.Add, this.fillStep);
                }
                else if (this.TargetFillAmount < this.currentFillAmount)
                {
                    this.currentFillAmount = this.SetFill(Operation.Subtract, this.fillStep);
                }

                this.controls.SetActive(this.TargetFillAmount >= 1);

                yield return _waitTime;
            }

            this.currentFillAmount = this.TargetFillAmount;
            this.fillArray[this.currentFillArrayIndex].fillAmount = this.TargetFillAmount - (float)this.currentFillArrayIndex;
            
            this.smoothFill = null;
        }
        
        /// <summary>
        /// Sets the <see cref="Image.fillAmount"/> of the <see cref="Image"/> in <see cref="fillArray"/> at <see cref="currentFillArrayIndex"/> <br/>
        /// <i>Will add the remaining amount to the next element if the "<see cref="_FillStep"/>" exceeds the max/min value (Next element depends on the given <see cref="Operation"/>)</i>
        /// </summary>
        /// <param name="_Operation">Allowed values are <see cref="Operation.Add"/> and <see cref="Operation.Subtract"/></param>
        /// <param name="_FillStep">Amount to add/subtract to/from the <see cref="Image.fillAmount"/></param>
        /// <returns><see cref="GetCurrentFillAmount"/></returns>
        private float SetFill(Operation _Operation, float _FillStep)
        {
            var _currentFill = this.fillArray[this.currentFillArrayIndex].fillAmount;
            var _fillAmount = _currentFill;
            var _remainder = 0f;
            var _nextIndex = this.currentFillArrayIndex;
            
            switch (_Operation)
            {
                case Operation.Add:
                    _fillAmount = _currentFill + _FillStep;
                    _remainder = _fillAmount - 1;
                    _nextIndex = _remainder > 0 && this.currentFillArrayIndex < this.fillArray.Length - 1? this.currentFillArrayIndex + 1 : this.currentFillArrayIndex;
                    break;
                case Operation.Subtract:
                    _fillAmount = _currentFill - _FillStep;
                    _remainder = Mathf.Abs(_fillAmount);
                    _nextIndex = _fillAmount < 0 && this.currentFillArrayIndex > 0 ? this.currentFillArrayIndex -1 : this.currentFillArrayIndex;
                    break;
            }
            
            this.fillArray[this.currentFillArrayIndex].fillAmount = _fillAmount;

            if (_nextIndex != this.currentFillArrayIndex)
            {
                this.currentFillArrayIndex = _nextIndex;
                this.SetFill(_Operation, _remainder);
            }

            return this.GetCurrentFillAmount();
        }

        /// <summary>
        /// Returns the <see cref="Image.fillAmount"/> of the element in <see cref="fillArray"/> at <see cref="currentFillArrayIndex"/>
        /// </summary>
        /// <returns>The <see cref="Image.fillAmount"/> of the element in <see cref="fillArray"/> at <see cref="currentFillArrayIndex"/></returns>
        private float GetCurrentFillAmount()
        {
            return this.currentFillArrayIndex + this.fillArray[this.currentFillArrayIndex].fillAmount;
        }

        /// <summary>
        /// Spawns a stone <see cref="Fruit"/> based on <see cref="TargetFillAmount"/>
        /// </summary>
        private void SpawnStoneFruit()
        {
            if (this.TargetFillAmount >= 1)
            {
                var _fruit = (Fruit)this.TargetFillAmount.Value - 1;
                this.SubtractFill((float)_fruit + 1);
                this.networkStoneFruitCharge.SpawnStoneFruit(_fruit);
            }
        }

        /// <summary>
        /// Reset the <see cref="Image.fillAmount"/>
        /// </summary>
        private void Reset()
        {
            this.SubtractFill(this.fillArray.Length);
        }

#if DEBUG || DEVELOPMENT_BUILD
        /// <summary>
        /// Adds/subtracts the <see cref="Image.fillAmount"/>
        /// </summary>
        /// <param name="_Amount">Amount to add/subtract</param>
        public static void SetFill_DEVELOPMENT(float _Amount)
        {
            Instance.TargetFillAmount += _Amount;
            
            if (Instance.smoothFill == null)
            {
                Instance.smoothFill = Instance.SmoothFill();
                Instance.StartCoroutine(Instance.smoothFill);
            }
        }
#endif
        #endregion
    }
}