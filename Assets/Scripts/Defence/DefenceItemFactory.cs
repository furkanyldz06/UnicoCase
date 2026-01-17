using System.Collections.Generic;
using BoardDefence.Core.Enums;
using BoardDefence.Data;
using UnityEngine;

namespace BoardDefence.Defence
{
    /// <summary>
    /// Factory Pattern implementation for creating Defence Items
    /// Provides centralized creation and configuration
    /// </summary>
    public class DefenceItemFactory : MonoBehaviour
    {
        [Header("Defence Item Data")]
        [SerializeField] private DefenceItemData _type1Data;
        [SerializeField] private DefenceItemData _type2Data;
        [SerializeField] private DefenceItemData _type3Data;
        
        [Header("Default Prefab")]
        [SerializeField] private GameObject _defaultPrefab;

        private Dictionary<DefenceItemType, DefenceItemData> _dataLookup;

        private void Awake()
        {
            InitializeLookup();
        }

        private void InitializeLookup()
        {
            _dataLookup = new Dictionary<DefenceItemType, DefenceItemData>
            {
                { DefenceItemType.Type1, _type1Data },
                { DefenceItemType.Type2, _type2Data },
                { DefenceItemType.Type3, _type3Data }
            };
        }

        /// <summary>
        /// Create a defence item of the specified type
        /// </summary>
        public DefenceItemBase Create(DefenceItemType type, Vector3 position, Transform parent = null)
        {
            var data = GetData(type);
            if (data == null)
            {
                Debug.LogError($"No data found for DefenceItemType: {type}");
                return null;
            }

            var prefab = data.Prefab != null ? data.Prefab : _defaultPrefab;
            if (prefab == null)
            {
                Debug.LogError("No prefab available for defence item creation");
                return null;
            }

            var instance = Instantiate(prefab, position, Quaternion.identity, parent);
            var defenceItem = instance.GetComponent<DefenceItemBase>();
            
            if (defenceItem == null)
            {
                defenceItem = instance.AddComponent<DefenceItemBase>();
            }

            defenceItem.Initialize(data);
            return defenceItem;
        }

        /// <summary>
        /// Create a defence item and place it on the board
        /// </summary>
        public DefenceItemBase CreateAndPlace(DefenceItemType type, Vector2Int gridPosition, 
            Vector3 worldPosition, Transform parent = null)
        {
            var item = Create(type, worldPosition, parent);
            if (item != null)
            {
                item.Place(gridPosition);
            }
            return item;
        }

        /// <summary>
        /// Get data for a specific defence item type
        /// </summary>
        public DefenceItemData GetData(DefenceItemType type)
        {
            if (_dataLookup == null)
            {
                InitializeLookup();
            }
            
            return _dataLookup.TryGetValue(type, out var data) ? data : null;
        }

        /// <summary>
        /// Register or update data for a defence item type
        /// </summary>
        public void RegisterData(DefenceItemType type, DefenceItemData data)
        {
            if (_dataLookup == null)
            {
                InitializeLookup();
            }
            
            _dataLookup[type] = data;
        }
    }
}

