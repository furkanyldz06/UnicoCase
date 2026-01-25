using System.Collections;
using BoardDefence.Core.Enums;
using BoardDefence.Core.Events;
using UnityEngine;

namespace BoardDefence.Core
{
    /// <summary>
    /// Game state'e göre ana kameranın X pozisyonunu yumuşak geçişle değiştirir.
    /// MainMenu'de X=0.5, Battle'da X=0 olacak şekilde 1 saniyelik lerp yapar.
    /// </summary>
    public class CameraStateMover : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera _camera;

        [Header("Positions")]
        [SerializeField] private float _mainMenuX = 0.5f;
        [SerializeField] private float _battleX = 0f;

        [Header("Animation")]
        [SerializeField] private float _duration = 1f;

        private Coroutine _moveRoutine;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void OnEnable()
        {
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleGameStateChanged(GameState state)
        {
            // Sadece MainMenu ve Battle için kamera kaydır
            switch (state)
            {
                case GameState.MainMenu:
                    StartMove(_mainMenuX);
                    break;
                case GameState.Battle:
                    StartMove(_battleX);
                    break;
                default:
                    // Diğer state'lerde kamera sabit kalsın
                    break;
            }
        }

        private void StartMove(float targetX)
        {
            if (_camera == null)
                return;

            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
            }

            _moveRoutine = StartCoroutine(MoveCameraX(targetX));
        }

        private IEnumerator MoveCameraX(float targetX)
        {
            var camTransform = _camera.transform;
            Vector3 startPos = camTransform.position;
            float startX = startPos.x;
            float elapsed = 0f;

            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);
                float newX = Mathf.Lerp(startX, targetX, t);
                camTransform.position = new Vector3(newX, startPos.y, startPos.z);
                yield return null;
            }

            camTransform.position = new Vector3(targetX, startPos.y, startPos.z);
            _moveRoutine = null;
        }
    }
}
