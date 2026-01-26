using BoardDefence.Board;
using BoardDefence.Core.Enums;
using BoardDefence.Core.Events;
using UnityEngine;

namespace BoardDefence.Defence
{

    [RequireComponent(typeof(Transform))]
    public class AttackRangeVisualizer : MonoBehaviour
    {
	    private static AttackRangeVisualizer _currentActive;

        [SerializeField] private Color _color = new(0f, 1f, 0f, 0.6f);
        [SerializeField] private float _lineWidth = 0.03f;
        [SerializeField] private int _segments = 40;
	        
	        [Header("Radar Effect")]
	        [SerializeField] private bool _enableRadar = true;
	        [SerializeField] private float _radarRotationSpeed = 90f; // derece/sn
	        [SerializeField] private float _radarWidthMultiplier = 1.2f;

        private LineRenderer _line;
        private float _radiusWorld;
	        
	        private LineRenderer _radarLine;
	        private float _currentRadarAngle;


        public void Initialize(float rangeInCells)
        {
            float cellSize = 1f;
            var board = FindObjectOfType<GameBoard>();
            if (board != null)
            {
                cellSize = board.TotalCellSize;
            }

            _radiusWorld = Mathf.Max(0.01f, rangeInCells * cellSize);

            if (_line == null)
            {
                SetupLineRenderer();
            }

            UpdateCircle();
	            SetupRadarLine();

	        SetAsActive();
        }

        private void Awake()
        {
            if (_line == null)
            {
                SetupLineRenderer();
            }
	            if (_radarLine == null)
	            {
	                SetupRadarLine();
	            }
        }

	    private void OnEnable()
	    {
	        GameEvents.OnGameStateChanged += HandleGameStateChanged;
	    }

	    private void OnDisable()
	    {
	        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
	        if (_currentActive == this)
	        {
	            _currentActive = null;
	        }
	    }

        private void SetupLineRenderer()
        {
            _line = GetComponent<LineRenderer>();
            if (_line == null)
            {
                _line = gameObject.AddComponent<LineRenderer>();
            }

            _line.useWorldSpace = false; // Merkez bu obje, noktalari lokal çizeriz
            _line.loop = true;
            _line.positionCount = Mathf.Max(3, _segments);
            _line.startWidth = _lineWidth;
            _line.endWidth = _lineWidth;

            var mat = new Material(Shader.Find("Sprites/Default"));
            _line.material = mat;
            _line.startColor = _color;
            _line.endColor = _color;
	        _line.enabled = false; // varsayılan olarak GÖRÜNMEZ, Show() ile açılır
        }
	        

	        private void SetupRadarLine()
	        {
	            if (!_enableRadar)
	                return;

	            if (_radarLine != null)
	                return;

	            var radarObj = new GameObject("RadarSweep");
	            radarObj.transform.SetParent(transform, false);

	            _radarLine = radarObj.AddComponent<LineRenderer>();
	            _radarLine.useWorldSpace = false;
	            _radarLine.loop = false;
	            _radarLine.positionCount = 2;
	            _radarLine.startWidth = _lineWidth * _radarWidthMultiplier;
	            _radarLine.endWidth = 0f; // uçta incelsin

	            var mat = new Material(Shader.Find("Sprites/Default"));
	            _radarLine.material = mat;

	            var startColor = _color;
	            var endColor = new Color(_color.r, _color.g, _color.b, 0f);
	            _radarLine.startColor = startColor;
	            _radarLine.endColor = endColor;

	            _radarLine.enabled = false;
	            _currentRadarAngle = 0f;
	        }

        private void UpdateCircle()
        {
            if (_line == null || _radiusWorld <= 0f)
                return;

            int count = _line.positionCount;
            float angleStep = 2f * Mathf.PI / count;

            for (int i = 0; i < count; i++)
            {
                float a = i * angleStep;
                float x = Mathf.Cos(a) * _radiusWorld;
                float y = Mathf.Sin(a) * _radiusWorld;
                _line.SetPosition(i, new Vector3(x, y, 0f));
            }
        }


	        private void Update()
	        {
	            if (_radarLine == null || !_radarLine.enabled || _radiusWorld <= 0f)
	                return;

	            _currentRadarAngle += _radarRotationSpeed * Mathf.Deg2Rad * Time.deltaTime;
	            if (_currentRadarAngle > Mathf.PI * 2f)
	            {
	                _currentRadarAngle -= Mathf.PI * 2f;
	            }

	            float x = Mathf.Cos(_currentRadarAngle) * _radiusWorld;
	            float y = Mathf.Sin(_currentRadarAngle) * _radiusWorld;

	            _radarLine.SetPosition(0, Vector3.zero);
	            _radarLine.SetPosition(1, new Vector3(x, y, 0f));
	        }


	        private void SetAsActive()
	        {
	            if (_currentActive != null && _currentActive != this)
	            {
	                _currentActive.Hide();
	            }
	            _currentActive = this;
	            Show();
	        }

	        public void Show()
	        {
	            if (_line == null)
	            {
	                SetupLineRenderer();
	                UpdateCircle();
	            }
	            _line.enabled = true;
	            
	            if (_enableRadar)
	            {
	                if (_radarLine == null)
	                {
	                    SetupRadarLine();
	                }
	                if (_radarLine != null)
	                {
	                    _radarLine.enabled = true;
	                }
	            }
	        }

	        public void Hide()
	        {
	            if (_line != null)
	            {
	                _line.enabled = false;
	            }
	            if (_radarLine != null)
	            {
	                _radarLine.enabled = false;
	            }
	        }

	        private void HandleGameStateChanged(GameState state)
	        {
	            if (state == GameState.Battle)
	            {
	                Hide();
	                if (_currentActive == this)
	                {
	                    _currentActive = null;
	                }
	            }
	        }

        private void OnValidate()
        {
            if (_line != null)
            {
                _line.startWidth = _lineWidth;
                _line.endWidth = _lineWidth;
                _line.startColor = _color;
                _line.endColor = _color;
            }
        }
    }
}

