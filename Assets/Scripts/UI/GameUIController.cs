using BoardDefence.Core;
using BoardDefence.Core.Enums;
using BoardDefence.Core.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoardDefence.UI
{
    /// <summary>
    /// Main UI controller for the game
    /// Listens to game events and updates UI accordingly
    /// </summary>
    public class GameUIController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _gamePanel;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private GameObject _defeatPanel;
	        [SerializeField] private GameObject _levelCompletedPanel;

        [Header("Game Info")]
        [SerializeField] private TextMeshProUGUI _livesText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _enemiesText;

        [Header("Defence Item Buttons")]
        [SerializeField] private Button _defenceItem1Button;
        [SerializeField] private Button _defenceItem2Button;
        [SerializeField] private Button _defenceItem3Button;
        [SerializeField] private TextMeshProUGUI _item1CountText;
        [SerializeField] private TextMeshProUGUI _item2CountText;
        [SerializeField] private TextMeshProUGUI _item3CountText;

        [Header("Control Buttons")]
        [SerializeField] private Button _startBattleButton;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _mainMenuButton;

        private void Start()
        {
            SubscribeToEvents();
            SetupButtons();
            ShowMainMenu();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnDefenceItemPlaced += HandleDefenceItemPlaced;
            GameEvents.OnDefenceItemRemoved += HandleDefenceItemRemoved;
            GameEvents.OnEnemyDied += HandleEnemyDied;
            GameEvents.OnEnemyReachedBase += HandleEnemyReachedBase;
	            GameEvents.OnLevelCompleted += HandleLevelCompleted;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnDefenceItemPlaced -= HandleDefenceItemPlaced;
            GameEvents.OnDefenceItemRemoved -= HandleDefenceItemRemoved;
            GameEvents.OnEnemyDied -= HandleEnemyDied;
            GameEvents.OnEnemyReachedBase -= HandleEnemyReachedBase;
	            GameEvents.OnLevelCompleted -= HandleLevelCompleted;
        }

        private void SetupButtons()
        {
            _defenceItem1Button?.onClick.AddListener(() => SelectDefenceItem(DefenceItemType.Type1));
            _defenceItem2Button?.onClick.AddListener(() => SelectDefenceItem(DefenceItemType.Type2));
            _defenceItem3Button?.onClick.AddListener(() => SelectDefenceItem(DefenceItemType.Type3));
            
            _startBattleButton?.onClick.AddListener(OnStartBattleClicked);
            _pauseButton?.onClick.AddListener(OnPauseClicked);
            _resumeButton?.onClick.AddListener(OnResumeClicked);
            _restartButton?.onClick.AddListener(OnRestartClicked);
            _mainMenuButton?.onClick.AddListener(OnMainMenuClicked);
        }

        private void HandleGameStateChanged(GameState state)
        {
            HideAllPanels();
            
            switch (state)
            {
                case GameState.MainMenu:
                    _mainMenuPanel?.SetActive(true);
                    break;
                case GameState.Preparation:
                case GameState.Battle:
                    _gamePanel?.SetActive(true);
                    // _startBattleButton?.gameObject.SetActive(state == GameState.Preparation);
                    break;
                case GameState.Paused:
                    _gamePanel?.SetActive(true);
                    _pausePanel?.SetActive(true);
                    break;
                case GameState.Victory:
                    _victoryPanel?.SetActive(true);
                    break;
                case GameState.Defeat:
                    _defeatPanel?.SetActive(true);
                    break;
            }
            
            UpdateUI();
        }

        private void HideAllPanels()
        {
            _mainMenuPanel?.SetActive(false);
            _gamePanel?.SetActive(false);
            _pausePanel?.SetActive(false);
            _victoryPanel?.SetActive(false);
            _defeatPanel?.SetActive(false);
	            _levelCompletedPanel?.SetActive(false);
        }

        public void UpdateUI()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            if (_livesText != null)
                _livesText.text = $"Base Health: {gm.CurrentLives}";
            
            if (_levelText != null && gm.LevelManager != null)
                _levelText.text = $"Level: {gm.LevelManager.CurrentLevel}";
            
            if (_enemiesText != null && gm.LevelManager != null)
                _enemiesText.text = $"Enemies: {gm.LevelManager.RemainingEnemies}";

            UpdateItemCounts();
        }

        private void UpdateItemCounts()
        {
            var bm = GameManager.Instance?.BoardManager;
            if (bm == null) return;

	    	    if (_item1CountText != null)
	    	        _item1CountText.text = $"Remaining: {bm.GetAvailableCount(DefenceItemType.Type1)}";
	    	    if (_item2CountText != null)
	    	        _item2CountText.text = $"Remaining: {bm.GetAvailableCount(DefenceItemType.Type2)}";
	    	    if (_item3CountText != null)
	    	        _item3CountText.text = $"Remaining: {bm.GetAvailableCount(DefenceItemType.Type3)}";
        }

        private void SelectDefenceItem(DefenceItemType type)
        {
            GameManager.Instance?.BoardManager?.SelectItemType(type);
        }

        private void HandleDefenceItemPlaced(Vector2Int pos, DefenceItemType type) => UpdateItemCounts();
        private void HandleDefenceItemRemoved(Vector2Int pos) => UpdateItemCounts();
	        private void HandleEnemyDied(Vector2Int pos) => UpdateUI();
	        private void HandleEnemyReachedBase() => UpdateUI();
	        
	        private void HandleLevelCompleted(int level)
	        {
	            Debug.Log($"[GameUI] HandleLevelCompleted called for level {level}");
	            
	            var gm = GameManager.Instance;
	            var levelManager = gm?.LevelManager;
	
	            // Eger bu olay son level iin geldiyse, ayrca "Level Completed"
	            // paneli gstermeyelim; zaten GameState.Victory ile Victory paneli alacak.
	            if (levelManager != null)
	            {
	                int totalLevels = levelManager.TotalLevels;
	                Debug.Log($"[GameUI] TotalLevels={totalLevels}");
	                
	                if (totalLevels > 0 && level >= totalLevels)
	                {
	                    Debug.Log("[GameUI] Last level completed, skipping LevelCompleted panel (Victory will be shown).");
	                    return; // son level tamamland; Victory paneli devreye girecek
	                }
	            }
	
	            if (_levelCompletedPanel == null)
	            {
	                Debug.LogWarning("[GameUI] _levelCompletedPanel is NULL! Inspector'da atanmam.");
	                return;
	            }
	            
	            // Dier tm level'ler bittikten sonra bu panel alsn
	            _levelCompletedPanel.SetActive(true);
	            Debug.Log("[GameUI] LevelCompleted panel set active.");
	        }

        // Button callbacks
        public void OnStartGameClicked() => GameManager.Instance?.StartGame();
        private void OnStartBattleClicked() => GameManager.Instance?.StartBattle();
        private void OnPauseClicked() => GameManager.Instance?.PauseGame();
        private void OnResumeClicked() => GameManager.Instance?.ResumeGame();
        private void OnRestartClicked() => GameManager.Instance?.RestartLevel();
        private void OnMainMenuClicked() => ShowMainMenu();
        
        private void ShowMainMenu()
        {
            HideAllPanels();
            _mainMenuPanel?.SetActive(true);
        }
    }
}

