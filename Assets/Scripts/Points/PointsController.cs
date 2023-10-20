using System.Collections;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using Watermelon_Game.Menu;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Points
{
    internal sealed class PointsController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [SerializeField] private TextMeshProUGUI points;
        [SerializeField] private TextMeshProUGUI highScore;
        [SerializeField] private Multiplier multiplier;
        [Header("Settings")]
        [SerializeField] private float pointsWaitTime = .05f;
        #endregion
        
        #region Fields
        private uint currentPoints;
        private uint pointsDelta;
        
        [CanBeNull] private IEnumerator pointsCoroutine;
        private WaitForSeconds pointsWaitForSeconds;
        #endregion

        #region Properties
        public static PointsController Instance { get; private set; }
        public uint CurrentPoints => this.currentPoints;
        #endregion

        #region Methods
        private void Awake()
        {
            Instance = this;
            
            this.pointsWaitForSeconds = new WaitForSeconds(this.pointsWaitTime);
        }

        private void Start()
        {
            this.highScore.text = PlayerPrefs.GetInt(StatsMenu.BEST_SCORE_KEY).ToString();
        }

        public void AddPoints(Fruit.Fruit _Fruit)
        {
            this.multiplier.StartMultiplier();

            var _points = (int)((int)_Fruit + this.multiplier.CurrentMultiplier);
            this.SetPoints(_points);
        }

        public void SubtractPoints(uint _PointsToSubtract) 
        {
            this.SetPoints(-(int)_PointsToSubtract);
        }

        public void ResetPoints()
        {
            this.SetPoints(0);
        }
        
        /// <summary>
        /// Adds the given points to <see cref="currentPoints"/> <br/>
        /// <i>Set to 0, to reset <see cref="currentPoints"/> and <see cref="pointsDelta"/></i>
        /// </summary>
        /// <param name="_Points">The points to add/subtract to <see cref="currentPoints"/></param>
        private void SetPoints(int _Points)
        {
            if (_Points == 0)
            {
                this.currentPoints = 0;
                this.pointsDelta = 1;
            }
            else
            {
                this.currentPoints = (uint)Mathf.Clamp(this.currentPoints + _Points, 0, uint.MaxValue);
            }

            if (this.pointsCoroutine == null)
            {
                this.pointsCoroutine = SetPoints();
                StartCoroutine(this.pointsCoroutine);
            }
            
            SkillController.Instance.PointsChanged(this.currentPoints);
        }

        /// <summary>
        /// Gradually increase/decreases the points over time
        /// </summary>
        /// <returns></returns>
        private IEnumerator SetPoints()
        {
            while (this.pointsDelta != this.currentPoints)
            {
                if (this.pointsDelta < this.currentPoints)
                {
                    this.pointsDelta++;
                }
                else if (this.pointsDelta > this.currentPoints)
                {
                    this.pointsDelta--;
                }
                
                this.SetPointsText(this.pointsDelta);
                
                yield return this.pointsWaitForSeconds;
            }
            
            StopCoroutine(this.pointsCoroutine);
            this.pointsCoroutine = null;
        }
        
        private void SetPointsText(uint _Points)
        {
            this.points.text = string.Concat(_Points, 'P');
        }
        
        public void SavePoints()
        {
            GameOverMenu.Instance.Score = this.currentPoints;
            var _newHighScore = StatsMenu.Instance.NewBestScore(this.currentPoints);
            if (_newHighScore)
            {
                this.highScore.text = this.currentPoints.ToString();
            }
        }
        #endregion
    }
}