using System.Collections.Generic;
using BoardDefence.Core.Events;
using UnityEngine;

namespace BoardDefence.Board
{

    public class GameBoard : MonoBehaviour
    {
        [Header("Board Configuration")]
        [SerializeField] private int _width = 4;
        [SerializeField] private int _height = 8;
        [SerializeField] private int _placeableRowStart = 4;
        
        [Header("Cell Settings")]
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private float _cellSpacing = 0.1f;
        
        [Header("Visual Settings")]
        [SerializeField] private Color _topZoneColor = new(0.4f, 0.6f, 0.8f, 1f);
        [SerializeField] private Color _bottomZoneColor = new(0.6f, 0.7f, 0.9f, 1f);

        private BoardCell[,] _cells;
        private Dictionary<Vector2Int, BoardCell> _cellLookup;

        #region Properties
        
        public int Width => _width;
        public int Height => _height;
        public int PlaceableRowStart => _placeableRowStart;
        public float CellSize => _cellSize;
        public float CellSpacing => _cellSpacing;
        public float TotalCellSize => _cellSize + _cellSpacing;
        
        #endregion

        private void Awake()
        {
            _cells = new BoardCell[_width, _height];
            _cellLookup = new Dictionary<Vector2Int, BoardCell>();
        }
        public void Initialize()
        {
            ClearBoard();
            CreateBoard();
        }

        private void CreateBoard()
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    CreateCell(x, y);
                }
            }
        }

        private void CreateCell(int x, int y)
        {
            var position = GridToWorldPosition(new Vector2Int(x, y));
            var cellObj = Instantiate(_cellPrefab, position, Quaternion.identity, transform);
            cellObj.name = $"Cell_{x}_{y}";
            
            var cell = cellObj.GetComponent<BoardCell>();
            if (cell == null)
            {
                cell = cellObj.AddComponent<BoardCell>();
            }
            
            bool isPlaceableZone = y >= _placeableRowStart;
            cell.Initialize(new Vector2Int(x, y), isPlaceableZone);
            
            _cells[x, y] = cell;
            _cellLookup[new Vector2Int(x, y)] = cell;
        }


        public Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            float totalSize = TotalCellSize;
            float xOffset = -(_width - 1) * totalSize / 2f;
            float yOffset = -(_height - 1) * totalSize / 2f;
            
            return new Vector3(
                gridPos.x * totalSize + xOffset,
                -gridPos.y * totalSize - yOffset,
                0
            );
        }

        public Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            float totalSize = TotalCellSize;
            float xOffset = -(_width - 1) * totalSize / 2f;
            float yOffset = -(_height - 1) * totalSize / 2f;
            
            int x = Mathf.RoundToInt((worldPos.x - xOffset) / totalSize);
            int y = Mathf.RoundToInt((-worldPos.y - yOffset) / totalSize);
            
            return new Vector2Int(
                Mathf.Clamp(x, 0, _width - 1),
                Mathf.Clamp(y, 0, _height - 1)
            );
        }

        public BoardCell GetCell(Vector2Int position)
        {
            return _cellLookup.TryGetValue(position, out var cell) ? cell : null;
        }


        public BoardCell GetCell(int x, int y) => GetCell(new Vector2Int(x, y));


        public bool IsValidPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < _width &&
                   position.y >= 0 && position.y < _height;
        }


        public bool IsPlaceablePosition(Vector2Int position)
        {
            return IsValidPosition(position) && position.y >= _placeableRowStart;
        }


        public void ClearBoard()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            
            _cells = new BoardCell[_width, _height];
            _cellLookup.Clear();
            
            GameEvents.RaiseBoardCleared();
        }
    }
}

