using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Island.Common.Services.Network;
using Island.Gameplay.Services.World;
using Island.Gameplay.Services.World.Items;
using TendedTarsier.Core.Services.Modules;
using UnityEditor;
using UnityEngine;

namespace Island.Gameplay.Configs.World
{
    [CreateAssetMenu(menuName = "Island/WorldConfig", fileName = "WorldConfig")]
    public class WorldConfig : ConfigBase
    {
        [field: SerializedDictionary("Type", "Prefab")]
        public SerializedDictionary<WorldObjectType, WorldObjectBase> WorldItemObjects;

        [field: SerializeField] public List<NetworkSpawnRequest> WorldItemPlacement;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WorldConfig))]
    public class WorldConfigEditor : Editor
    {
        private WorldConfig _worldConfig;

        public void OnEnable()
        {
            _worldConfig = (WorldConfig)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDropArea();
            DrawDefaultInspector();
        }

        private void DrawDropArea()
        {
            var rect = GUILayoutUtility.GetRect(0, 80, GUILayout.ExpandWidth(true));
            GUI.Box(rect, "Drag & Drop scene objects here", EditorStyles.helpBox);

            var e = Event.current;
            if (!rect.Contains(e.mousePosition)) return;

            if (e.type == EventType.DragUpdated || e.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    var dragged = DragAndDrop.objectReferences;
                    var fromHierarchy = dragged
                        .Select(o => (o as GameObject)?.GetComponent<WorldObjectBase>())
                        .Where(t => t != null)
                        .ToArray();

                    AddTransforms(fromHierarchy);
                }

                e.Use();
            }
        }

        private void AddTransforms(WorldObjectBase[] worldItems)
        {
            if (_worldConfig == null || worldItems == null || worldItems.Length == 0) return;

            Undo.RecordObject(_worldConfig, "Add WorldItemObject");

            foreach (var item in worldItems)
            {
                _worldConfig.WorldItemPlacement.Add(new NetworkSpawnRequest(item.CombineHash, item.Type, item.transform.position, item.transform.rotation));
            }

            EditorUtility.SetDirty(_worldConfig);
        }
    }
#endif
}