#if UNITY_EDITOR
using BoardDefence.Core.Enums;
using BoardDefence.Data;
using UnityEditor;
using UnityEngine;

namespace BoardDefence.Editor
{
    /// <summary>
    /// Editor utility for quick setup of Board Defence game
    /// </summary>
    public static class BoardDefenceSetup
    {
        private const string SO_PATH = "Assets/ScriptableObjects";
        
        [MenuItem("Board Defence/Setup/Create All ScriptableObjects")]
        public static void CreateAllScriptableObjects()
        {
            CreateDirectories();
            CreateDefenceItemData();
            CreateEnemyData();
            CreateLevelData();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Board Defence: All ScriptableObjects created!");
        }

        [MenuItem("Board Defence/Setup/Create Defence Item Data")]
        public static void CreateDefenceItemData()
        {
            CreateDirectories();
            
            // Type 1: damage: 3, range: 4, interval: 3s, direction: forward
            CreateDefenceItem("DefenceItem_Type1", DefenceItemType.Type1, 
                "Forward Turret", 3, 4, 3f, AttackDirection.Forward, Color.blue);
            
            // Type 2: damage: 5, range: 2, interval: 4s, direction: forward
            CreateDefenceItem("DefenceItem_Type2", DefenceItemType.Type2,
                "Heavy Turret", 5, 2, 4f, AttackDirection.Forward, Color.green);
            
            // Type 3: damage: 10, range: 1, interval: 5s, direction: all
            CreateDefenceItem("DefenceItem_Type3", DefenceItemType.Type3,
                "Omni Turret", 10, 1, 5f, AttackDirection.All, Color.red);
        }

        [MenuItem("Board Defence/Setup/Create Enemy Data")]
        public static void CreateEnemyData()
        {
            CreateDirectories();
            
            // Type 1: health: 3, speed: 1 block/s
            CreateEnemy("Enemy_Type1", EnemyType.Type1, "Fast Runner", 3, 1f, Color.yellow);
            
            // Type 2: health: 10, speed: 0.25 block/s
            CreateEnemy("Enemy_Type2", EnemyType.Type2, "Tank", 10, 0.25f, Color.gray);
            
            // Type 3: health: 5, speed: 0.5 block/s
            CreateEnemy("Enemy_Type3", EnemyType.Type3, "Balanced", 5, 0.5f, Color.magenta);
        }

        [MenuItem("Board Defence/Setup/Create Level Data")]
        public static void CreateLevelData()
        {
            CreateDirectories();
            
            // Level 1
            CreateLevel("Level_01", 1, "Training Ground",
                new[] { (DefenceItemType.Type1, 3), (DefenceItemType.Type2, 2), (DefenceItemType.Type3, 1) },
                new[] { (EnemyType.Type1, 3), (EnemyType.Type2, 1), (EnemyType.Type3, 1) });
            
            // Level 2
            CreateLevel("Level_02", 2, "The Challenge",
                new[] { (DefenceItemType.Type1, 3), (DefenceItemType.Type2, 4), (DefenceItemType.Type3, 2) },
                new[] { (EnemyType.Type1, 5), (EnemyType.Type2, 2), (EnemyType.Type3, 3) });
            
            // Level 3
            CreateLevel("Level_03", 3, "Final Stand",
                new[] { (DefenceItemType.Type1, 5), (DefenceItemType.Type2, 7), (DefenceItemType.Type3, 5) },
                new[] { (EnemyType.Type1, 7), (EnemyType.Type2, 3), (EnemyType.Type3, 5) });
        }

        private static void CreateDirectories()
        {
            if (!AssetDatabase.IsValidFolder(SO_PATH))
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            if (!AssetDatabase.IsValidFolder($"{SO_PATH}/DefenceItems"))
                AssetDatabase.CreateFolder(SO_PATH, "DefenceItems");
            if (!AssetDatabase.IsValidFolder($"{SO_PATH}/Enemies"))
                AssetDatabase.CreateFolder(SO_PATH, "Enemies");
            if (!AssetDatabase.IsValidFolder($"{SO_PATH}/Levels"))
                AssetDatabase.CreateFolder(SO_PATH, "Levels");
        }

        private static void CreateDefenceItem(string name, DefenceItemType type, string displayName,
            int damage, int range, float interval, AttackDirection direction, Color color)
        {
            var path = $"{SO_PATH}/DefenceItems/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<DefenceItemData>(path) != null) return;
            
            var data = ScriptableObject.CreateInstance<DefenceItemData>();
            // Set values via SerializedObject for private fields
            var so = new SerializedObject(data);
            so.FindProperty("_itemType").enumValueIndex = (int)type;
            so.FindProperty("_itemName").stringValue = displayName;
            so.FindProperty("_damage").intValue = damage;
            so.FindProperty("_range").intValue = range;
            so.FindProperty("_attackInterval").floatValue = interval;
            so.FindProperty("_attackDirection").enumValueIndex = (int)direction;
            so.FindProperty("_tintColor").colorValue = color;
            so.ApplyModifiedPropertiesWithoutUndo();
            
            AssetDatabase.CreateAsset(data, path);
        }

        private static void CreateEnemy(string name, EnemyType type, string displayName,
            int health, float speed, Color color)
        {
            var path = $"{SO_PATH}/Enemies/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<EnemyData>(path) != null) return;
            
            var data = ScriptableObject.CreateInstance<EnemyData>();
            var so = new SerializedObject(data);
            so.FindProperty("_enemyType").enumValueIndex = (int)type;
            so.FindProperty("_enemyName").stringValue = displayName;
            so.FindProperty("_maxHealth").intValue = health;
            so.FindProperty("_moveSpeed").floatValue = speed;
            so.FindProperty("_tintColor").colorValue = color;
            so.ApplyModifiedPropertiesWithoutUndo();
            
            AssetDatabase.CreateAsset(data, path);
        }

        private static void CreateLevel(string name, int levelNum, string displayName,
            (DefenceItemType type, int count)[] defenceItems,
            (EnemyType type, int count)[] enemies)
        {
            var path = $"{SO_PATH}/Levels/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<LevelData>(path) != null) return;
            
            var data = ScriptableObject.CreateInstance<LevelData>();
            var so = new SerializedObject(data);
            so.FindProperty("_levelNumber").intValue = levelNum;
            so.FindProperty("_levelName").stringValue = displayName;
            
            var defProp = so.FindProperty("_availableDefenceItems");
            defProp.arraySize = defenceItems.Length;
            for (int i = 0; i < defenceItems.Length; i++)
            {
                var elem = defProp.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("ItemType").enumValueIndex = (int)defenceItems[i].type;
                elem.FindPropertyRelative("Count").intValue = defenceItems[i].count;
            }
            
            var enemyProp = so.FindProperty("_enemySpawns");
            enemyProp.arraySize = enemies.Length;
            for (int i = 0; i < enemies.Length; i++)
            {
                var elem = enemyProp.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("EnemyType").enumValueIndex = (int)enemies[i].type;
                elem.FindPropertyRelative("Count").intValue = enemies[i].count;
            }
            
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(data, path);
        }
    }
}
#endif

