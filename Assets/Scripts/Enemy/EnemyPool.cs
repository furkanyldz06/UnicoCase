using System.Collections.Generic;
using BoardDefence.Core.Enums;
using BoardDefence.Data;
using UnityEngine;

namespace BoardDefence.Enemy
{

    public class EnemyPool : MonoBehaviour
    {
        [Header("Pool Settings")]
        [SerializeField] private int _initialPoolSize = 20;
        [SerializeField] private bool _expandable = true;
        [SerializeField] private Transform _poolContainer;
        
        [Header("Enemy Data")]
        [SerializeField] private EnemyData _type1Data;
        [SerializeField] private EnemyData _type2Data;
        [SerializeField] private EnemyData _type3Data;
        
        [Header("Default Prefab")]
        [SerializeField] private GameObject _defaultEnemyPrefab;

        private Dictionary<EnemyType, Queue<EnemyBase>> _pools;
        private Dictionary<EnemyType, EnemyData> _dataLookup;
        private Dictionary<EnemyType, List<EnemyBase>> _activeEnemies;

        #region Properties
        
        public int TotalActiveEnemies
        {
            get
            {
                int count = 0;
                foreach (var list in _activeEnemies.Values)
                {
                    count += list.Count;
                }
                return count;
            }
        }
        
        #endregion

        private void Awake()
        {
            InitializeLookups();
            InitializePools();
        }

        private void InitializeLookups()
        {
            _dataLookup = new Dictionary<EnemyType, EnemyData>
            {
                { EnemyType.Type1, _type1Data },
                { EnemyType.Type2, _type2Data },
                { EnemyType.Type3, _type3Data }
            };

            _activeEnemies = new Dictionary<EnemyType, List<EnemyBase>>
            {
                { EnemyType.Type1, new List<EnemyBase>() },
                { EnemyType.Type2, new List<EnemyBase>() },
                { EnemyType.Type3, new List<EnemyBase>() }
            };
        }

        private void InitializePools()
        {
            _pools = new Dictionary<EnemyType, Queue<EnemyBase>>();
            
            if (_poolContainer == null)
            {
                var containerObj = new GameObject("EnemyPoolContainer");
                containerObj.transform.SetParent(transform);
                _poolContainer = containerObj.transform;
            }

            foreach (EnemyType type in System.Enum.GetValues(typeof(EnemyType)))
            {
                _pools[type] = new Queue<EnemyBase>();
                PrePopulatePool(type, _initialPoolSize / 3);
            }
        }

        private void PrePopulatePool(EnemyType type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var enemy = CreateEnemy(type);
                if (enemy != null)
                {
                    enemy.gameObject.SetActive(false);
                    _pools[type].Enqueue(enemy);
                }
            }
        }


        public EnemyBase Get(EnemyType type)
        {
            EnemyBase enemy;

            if (_pools[type].Count > 0)
            {
                enemy = _pools[type].Dequeue();
            }
            else if (_expandable)
            {
                enemy = CreateEnemy(type);
            }
            else
            {
                Debug.LogWarning($"Enemy pool for {type} is exhausted and not expandable");
                return null;
            }

            if (enemy != null)
            {
                enemy.OnSpawn();
                _activeEnemies[type].Add(enemy);
                
                enemy.OnDefeated = (e) => Return(e);
                enemy.OnReachedBase = (e) => Return(e);
            }

            return enemy;
        }


        public void Return(EnemyBase enemy)
        {
            if (enemy == null) return;

            enemy.OnDespawn();
            _activeEnemies[enemy.EnemyType].Remove(enemy);
            _pools[enemy.EnemyType].Enqueue(enemy);
        }


        public void ReturnAll()
        {
            foreach (var type in _activeEnemies.Keys)
            {
                var enemies = new List<EnemyBase>(_activeEnemies[type]);
                foreach (var enemy in enemies)
                {
                    Return(enemy);
                }
            }
        }


        public List<EnemyBase> GetActiveEnemies()
        {
            var allActive = new List<EnemyBase>();
            foreach (var list in _activeEnemies.Values)
            {
                allActive.AddRange(list);
            }
            return allActive;
        }

        private EnemyBase CreateEnemy(EnemyType type)
        {
            var data = _dataLookup.TryGetValue(type, out var d) ? d : null;
            var prefab = data?.Prefab ?? _defaultEnemyPrefab;
            
            if (prefab == null)
            {
                Debug.LogError($"No prefab for enemy type: {type}");
                return null;
            }

            var instance = Instantiate(prefab, _poolContainer);
            var enemy = instance.GetComponent<EnemyBase>();
            
            if (enemy == null)
            {
                enemy = instance.AddComponent<EnemyBase>();
            }

            if (data != null)
            {
                enemy.Initialize(data);
            }

            return enemy;
        }
    }
}

