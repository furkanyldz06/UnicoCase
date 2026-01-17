using BoardDefence.Core;
using BoardDefence.Core.Enums;
using UnityEngine;

namespace BoardDefence.Input
{
    /// <summary>
    /// Handles player input for the game
    /// Decouples input from game logic
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera _mainCamera;
        
        [Header("Settings")]
        [SerializeField] private KeyCode _pauseKey = KeyCode.Escape;
        [SerializeField] private KeyCode _item1Key = KeyCode.Alpha1;
        [SerializeField] private KeyCode _item2Key = KeyCode.Alpha2;
        [SerializeField] private KeyCode _item3Key = KeyCode.Alpha3;

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            HandlePauseInput();
            HandleItemSelectionInput();
        }

        private void HandlePauseInput()
        {
            if (UnityEngine.Input.GetKeyDown(_pauseKey))
            {
                var gm = GameManager.Instance;
                if (gm == null) return;

                if (gm.CurrentState == GameState.Paused)
                {
                    gm.ResumeGame();
                }
                else if (gm.CurrentState == GameState.Battle || gm.CurrentState == GameState.Preparation)
                {
                    gm.PauseGame();
                }
            }
        }

        private void HandleItemSelectionInput()
        {
            var boardManager = GameManager.Instance?.BoardManager;
            if (boardManager == null || !boardManager.PlacementEnabled) return;

            if (UnityEngine.Input.GetKeyDown(_item1Key))
            {
                boardManager.SelectItemType(DefenceItemType.Type1);
            }
            else if (UnityEngine.Input.GetKeyDown(_item2Key))
            {
                boardManager.SelectItemType(DefenceItemType.Type2);
            }
            else if (UnityEngine.Input.GetKeyDown(_item3Key))
            {
                boardManager.SelectItemType(DefenceItemType.Type3);
            }
        }

        /// <summary>
        /// Get current mouse position in world space
        /// </summary>
        public Vector3 GetMouseWorldPosition()
        {
            if (_mainCamera == null) return Vector3.zero;
            
            var mousePos = UnityEngine.Input.mousePosition;
            mousePos.z = -_mainCamera.transform.position.z;
            return _mainCamera.ScreenToWorldPoint(mousePos);
        }

        /// <summary>
        /// Check if left mouse button was clicked this frame
        /// </summary>
        public bool WasLeftClickPressed()
        {
            return UnityEngine.Input.GetMouseButtonDown(0);
        }

        /// <summary>
        /// Check if right mouse button was clicked this frame
        /// </summary>
        public bool WasRightClickPressed()
        {
            return UnityEngine.Input.GetMouseButtonDown(1);
        }
    }
}

