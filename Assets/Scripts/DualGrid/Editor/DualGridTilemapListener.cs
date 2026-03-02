using DualGrid.Runtime.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DualGrid.Editor
{
    [InitializeOnLoad]
    public static class DualGridTilemapListener
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        static DualGridTilemapListener()
        {
            Tilemap.tilemapTileChanged += HandleTilemapChange;
        }
        
        /// <summary>
        ///     Handles any changes made to the data tilemap, provided that editor is not in play mode or about to be
        /// </summary>
        /// <param name="tilemap">the tilemap making changes</param>
        /// <param name="tiles">the tiles that have been changeda</param>
        private static void HandleTilemapChange(Tilemap tilemap, Tilemap.SyncTile[] tiles)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying)
                return;

            var dualGridComponent = Object.FindObjectsByType<CDualGridTilemap>(FindObjectsSortMode.None);
            foreach (var component in dualGridComponent)
            {
                if(component.DataTilemap == tilemap)
                {
                    component.HandleTilemapChange(tilemap, tiles);
                }
            }
        }
    }
}
