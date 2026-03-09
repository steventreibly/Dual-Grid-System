using System.Collections.Generic;
using System.Linq;
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
            
            //TODO: Return data we're actually interested in
            return default;
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
                    //TODO: Need an editor extension to check if the sprite is split in 16x sprites, otherwise the texture is incompatible
                    
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
