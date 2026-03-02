using System.Collections.Generic;
using DualGrid.Tiles;
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
        private ReorderableList _tilingRulesReorderableList;

        private bool _hasMultipleTargets = false;
        private List<DualGridRuleTile> _targetDualGridRuleTiles = new List<DualGridRuleTile>();

        //TODO: Finish setting up necessary variable values for Editor functionality.
        public override void OnEnable()
        {
            _targetDualGridRuleTile = new DualGridRuleTile();
            
            base.OnEnable();
        }
    }
}
