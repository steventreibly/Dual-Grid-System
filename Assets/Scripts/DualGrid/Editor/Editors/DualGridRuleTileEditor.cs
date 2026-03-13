using System.Collections.Generic;
using System.Linq;
using DualGrid.Editor.Extensions;
using DualGrid.Runtime.Components;
using DualGrid.Tiles;
using Unity.Plastic.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DualGrid.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DualGridRuleTile), true)]
    public class DualGridRuleTileEditor : RuleTileEditor
    {
        private DualGridRuleTile _targetDualGridRuleTile;
        private bool _isPreviewActive;
        private const string PreviewActiveStatusKey = "PreviewActiveStatusKey";
        private ReorderableList _tilingRulesReorderableList;

        private bool _hasMultipleTargets = false;
        private List<DualGridRuleTile> _targetDualGridRuleTiles = new List<DualGridRuleTile>();

        //TODO: Finish setting up necessary variable values for Editor functionality.
        public override void OnEnable()
        {
            _targetDualGridRuleTile = (DualGridRuleTile)target;
            _hasMultipleTargets = targets.Length > 1;

            if (_hasMultipleTargets)
                _targetDualGridRuleTiles = targets.Cast<DualGridRuleTile>().ToList();
            else
            {
                _targetDualGridRuleTiles = new List<DualGridRuleTile>()
                {
                    target as DualGridRuleTile
                };
            }

            _isPreviewActive = EditorPrefs.GetBool(PreviewActiveStatusKey);

            _tilingRulesReorderableList = new ReorderableList(tile != null ? tile.m_TilingRules : null,
                typeof(RuleTile.TilingRule), true, true, false, false);
            _tilingRulesReorderableList.drawHeaderCallback = OnDrawHeader;
            _tilingRulesReorderableList.drawElementCallback = OnDrawElement;
            _tilingRulesReorderableList.elementHeightCallback = GetElementHeight;
            _tilingRulesReorderableList.onChangedCallback = ListUpdated;
            
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            if(_hasMultipleTargets)
                Undo.RecordObjects(_targetDualGridRuleTiles.ToArray(), $"Updated {_targetDualGridRuleTiles.Count} Dual Grid Rule Tiles");
            else
                Undo.RecordObject(_targetDualGridRuleTile, $"Updated {_targetDualGridRuleTile.name} Dual Grid Rule Tiles");

            EditorGUI.BeginChangeCheck();

            var shouldContinue = DrawRuleTileOriginalTexture();
        }

        protected virtual bool DrawRuleTileOriginalTexture()
        {
            EditorGUILayout.LabelField("Dual Grid Settings", EditorStyles.boldLabel);

            if (_targetDualGridRuleTiles.Any(dualGridRuleTile =>
                    dualGridRuleTile.OriginalTexture == null && dualGridRuleTile.m_TilingRules.Count == 0))
            {
                DrawDragAndDropArea();
                return false;
            }
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _hasMultipleTargets &&
                                       _targetDualGridRuleTiles.HasDifferentValues(dualGridRuleTile =>
                                           dualGridRuleTile.OriginalTexture);
            Texture2D appliedTexture = EditorGUILayout.ObjectField(RuleTileEditorStyles.OriginalTexture,
                _targetDualGridRuleTile.OriginalTexture, typeof(Texture2D), false) as Texture2D;
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var dualGridRuleTile in _targetDualGridRuleTiles)
                {
                    bool wasTextureApplied = dualGridRuleTile.TryApplyTexture2D(appliedTexture);
                    if (!wasTextureApplied)
                        break; //Invalid texture, stop applying to other selected tiles
                }
            }
            
            EditorGUI.showMixedValue = false;
            
            return appliedTexture != null;
        }

        private void DrawDragAndDropArea()
        {
            if (_hasMultipleTargets)
            {
                EditorGUILayout.HelpBox("At least one of the selected Dual Grid RuleTiles is missing an Original Texture. \n" +
                                        "Please select the individual empty Dual Grid Rule Tile to set the Texture.", MessageType.Error);
            }
            else
            {
                Rect dropArea = GUILayoutUtility.GetRect(0, 100, GUILayout.ExpandWidth( true));
                GUI.Box(dropArea, "", EditorStyles.helpBox);
                GUI.Box(dropArea, "Drag and drop a texture to start creating this Dual Grid Rule Tile", EditorStyles.centeredGreyMiniLabel);
                
                Event evt = Event.current;

                if (evt.type == EventType.DragPerform || evt.type == EventType.DragUpdated)
                {
                    if (dropArea.Contains(evt.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (evt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                            {
                                OnDropObjectTextureArea(draggedObject);
                                break;
                            }
                        }
                    }
                }
            }
        }

        protected virtual void OnDropObjectTextureArea(UnityEngine.Object draggedObject)
        {
            if (draggedObject is Texture2D texture)
            {
                foreach (var dualGridRuleTile in _targetDualGridRuleTiles)
                {
                    bool wasTextureApplied = dualGridRuleTile.TryApplyTexture2D(texture);
                    if (!wasTextureApplied)
                    {
                        Debug.LogWarning($"The texture for {dualGridRuleTile} was invalid, application of texture to other tiles has stopped.");
                        return; //Invalid texture, stop applying to other selected tiles
                    }
                }
                
                Repaint();
            }
        }

        /// <summary>
        ///     Returns true if the Inspector should interrupt the drawing pipeline
        /// </summary>
        /// <returns></returns>
        protected virtual bool DrawRuleTileSettings()
        {
            EditorGUILayout.LabelField("Dual Grid Settings", EditorStyles.boldLabel);

            bool shouldUpdateAffectedModules = false;
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _hasMultipleTargets &&
                                       _targetDualGridRuleTiles.HasDifferentValues(dualGridRuleTile =>
                                           dualGridRuleTile.m_DefaultSprite);
            Sprite defaultSprite = EditorGUILayout.ObjectField(RuleTileEditorStyles.OriginalTexture,
                _targetDualGridRuleTiles.First().m_DefaultSprite, typeof(Sprite), false) as Sprite;
            if (EditorGUI.EndChangeCheck())
            {
                _targetDualGridRuleTiles.ForEach(dualGridRuleTile => dualGridRuleTile.m_DefaultSprite = defaultSprite);
                shouldUpdateAffectedModules = true;
            }
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _hasMultipleTargets &&
                                       _targetDualGridRuleTiles.HasDifferentValues(dualGridRuleTile =>
                                           dualGridRuleTile.m_DefaultGameObject);
            var defaultGameObject = EditorGUILayout.ObjectField(RuleTileEditorStyles.DefaultGameObject,
                tile.m_DefaultGameObject, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck())
            {
                _targetDualGridRuleTiles.ForEach(dualGridRuleTile =>
                    dualGridRuleTile.m_DefaultGameObject = (GameObject)defaultGameObject);
                shouldUpdateAffectedModules = true;
            }
            
            EditorGUI.showMixedValue = false;
            EditorGUILayout.Space();

            if (shouldUpdateAffectedModules)
            {
                _targetDualGridRuleTile.RefreshDataTile();

                var dualGridRuleTiles = FindObjectsByType<CDualGridTilemap>(FindObjectsSortMode.None);
                
                //TODO: Loop through dualGridRuleTiles and look to see the targetDualGridRuleTiles contain a renderTile, if so, update the collider and refresh the tilemap
            }
            
            //TODO:Return the actual value
            return default;
        }

        public override void RuleMatrixOnGUI(RuleTile tile, Rect rect, BoundsInt bounds, RuleTile.TilingRule tilingRule)
        {
            //TODO: investigate how to extend This to accomodate the DualGrid
            //The code below is the same as the base.RuleMatrixOnGUI. 
            Handles.color = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.2f) : new Color(0f, 0f, 0f, 0.2f);
            var w = rect.width / bounds.size.x;
            var h = rect.height / bounds.size.y;

            for (var y = 0; y <= bounds.size.y; y++)
            {
                var top = rect.yMin + y * h;
                Handles.DrawLine(new Vector3(rect.xMin, top), new Vector3(rect.xMax, top));
            }

            for (var x = 0; x <= bounds.size.x; x++)
            {
                var left = rect.xMin + x * w;
                Handles.DrawLine(new Vector3(left, rect.yMin), new Vector3(left, rect.yMax));
            }

            Handles.color = Color.white;

            var neighbors = tilingRule.GetNeighbors();

            for (var y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (var x = bounds.xMin; x < bounds.xMax; x++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    var r = new Rect(rect.xMin + (x - bounds.xMin) * w, rect.yMin + (-y + bounds.yMax - 1) * h, w - 1,
                        h - 1);
                    RuleMatrixIconOnGUI(tilingRule, neighbors, pos, r);
                }
            }
        }

        private float GetElementHeight(int index)
        {
            RuleTile.TilingRule rule = tile.m_TilingRules[index];
            return base.GetElementHeight(rule);
        }
    }
}
