using UnityEditor;
using UnityEngine;

namespace DualGrid.Editor
{
    public static class DualGridTilemapMenu
    {
        [MenuItem("GameObject/2D Object/Tilemap/Dual Grid Tilemap", false, 0)]
        private static void CreateDualGridTilemapMenu()
        {
            Grid selectedGrid = Selection.activeGameObject?.GetComponent<Grid>();
            
            var newCDualGridTilemap = CDualGridEditor.CreateNewDualGridTilemap(selectedGrid);

            Selection.activeGameObject = newCDualGridTilemap.gameObject;
        }   
    }
}
