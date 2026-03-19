using System.Collections.Generic;
using System.Linq;
using DualGrid.Editor.Extensions;
using DualGrid.Runtime.Components;
using DualGrid.Tiles;
using Unity.Plastic.Antlr3.Runtime.Misc;
using Unity.VisualScripting;
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

        /// <summary>
        ///     Draws a Rule Matrix for the given Rule for a RuleTile.
        /// <remarks>
        ///     Most of this is the base function, except for looping through the rows and columns.
        ///     This has been changed to limit the grid to a 2x2 matrix rather than 3x3.
        /// </remarks>
        /// </summary>
        /// <param name="ruleTile">Tile to draw rule for.</param>
        /// <param name="rect">GUI Rect to draw rule at.</param>
        /// <param name="bounds">Cell bounds of the Rule.</param>
        /// <param name="tilingRule">Rule to draw Rule Matrix for.</param>
        public override void RuleMatrixOnGUI(RuleTile ruleTile, Rect rect, BoundsInt bounds, RuleTile.TilingRule tilingRule)
        {
            //The code below is the same as the base.RuleMatrixOnGUI. Comments indicate what has been changed to accomodate the dual grid
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

            //Limits the grid to only 2 rows
            for (var y = -1; y < 1; y++)
            {
                //Limits the grid to only 2 columns
                for (var x = -1; x < 1; x++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    var r = new Rect(rect.xMin + (x - bounds.xMin) * w, rect.yMin + (-y + bounds.yMax - 1) * h, w - 1,
                        h - 1);
                    RuleMatrixIconOnGUI(tilingRule, neighbors, pos, r);
                }
            }
        }

        /// <summary>
        ///     Get the GUI bounds for a Rule.
        /// <para>
        ///     Returns the bounds for a 2x2 grid.
        ///     Essentially returns:
        /// </para>
        ///     (-1, 0) (0, 0)
        ///<para> (-1, -1) (0, -1) </para>
        /// </summary>
        /// <param name="bounds">Cell bounds of the Rule.</param>
        /// <param name="tilingRule">Rule to get GUI bounds for.</param>
        /// <returns>The GUI bounds for a rule.</returns>
        public override BoundsInt GetRuleGUIBounds(BoundsInt bounds, RuleTile.TilingRule tilingRule)
        {
            return new BoundsInt(-1, -1, 0, 1, 1, 0);
        }

        /// <summary>
        ///     Gets the GUI matrix size for a Rule of a RuleTile
        /// </summary>
        /// <param name="bounds">Cell bounds of the Rule.</param>
        /// <returns>Returns the GUI matrix size for a Rule of a RuleTile.</returns>
        public override Vector2 GetMatrixSize(BoundsInt bounds)
        {
            //TODO: Review this number, adjust size as needed
            float matrixCellSize = 18.0f;
            return new Vector2(bounds.size.x * matrixCellSize, bounds.size.y * matrixCellSize);
        }

        /// <summary>
        ///     Draws the Rule element for the Rule list
        /// <remarks>
        ///     Overriden only to change the RuleInspectorOnGUI call.
        /// </remarks>
        /// </summary>
        /// <param name="rect">Rect to draw the Rule Element in</param>
        /// <param name="index">Index of the Rule Element to draw</param>
        /// <param name="isActive">Whether the Rule Element is active</param>
        /// <param name="isFocused">Whether the Rule Element is focused</param>
        protected override void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var rule = tile.m_TilingRules[index];
            var bounds = GetRuleGUIBounds(rule.GetBounds(), rule);

            var yPos = rect.yMin + 2f;
            var height = rect.height - k_PaddingBetweenRules;
            var matrixSize = GetMatrixSize(bounds);

            var spriteRect = new Rect(rect.xMax - k_DefaultElementHeight - 5f, yPos, k_DefaultElementHeight,
                k_DefaultElementHeight);
            var matrixRect = new Rect(rect.xMax - matrixSize.x - spriteRect.width - 10f, yPos, matrixSize.x,
                matrixSize.y);
            var inspectorRect = new Rect(rect.xMin, yPos, rect.width - matrixSize.x - spriteRect.width - 20f, height);

            RuleInspectorOnGUI(inspectorRect, rule);
            RuleMatrixOnGUI(tile, matrixRect, bounds, rule);
            SpriteOnGUI(spriteRect, rule);
        }
        
        protected virtual void DualGridRuleInspectorOnGUI(Rect rect, RuleTile.TilingRule tilingRule)
        {
            float y = rect.yMin;
            GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), RuleTileEditorStyles.TilingRules);
            tilingRule.m_GameObject = (GameObject)EditorGUI.ObjectField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), "", tilingRule.m_GameObject, typeof(GameObject), false);
            y += k_SingleLineHeight;
        }

        private float GetElementHeight(int index)
        {
            RuleTile.TilingRule rule = tile.m_TilingRules[index];
            return base.GetElementHeight(rule);
        }
    }
}
