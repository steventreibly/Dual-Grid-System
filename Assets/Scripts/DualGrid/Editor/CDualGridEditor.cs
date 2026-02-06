using System.Collections.Generic;
using System.Linq;
using DualGrid.Runtime.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DualGrid.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CDualGridTilemap))]
    public class CDualGridEditor : UnityEditor.Editor
    {
        private readonly GUIContent _renderTile = new GUIContent("Dual Grid Rule Tile",
            "The Render Tile that will be applied in the Render Tilemap.");

        private readonly GUIContent _enableTilemapCollider = new GUIContent("Enable Tilemap Collider",
            "If a TilemapCollider2D should be active based on the Dual Grid Rule Tile's default collider type.");

        private readonly GUIContent _gameObjectOrigin = new GUIContent("Game Object Origin",
            "Determines which tilemap the GameObjects defined in the Dual Grid Rule Tile should be in.");
        
        private CDualGridTilemap _targetDualGridTilemap;

        private bool _hasMultipleTargets = false;
        
        private List<CDualGridTilemap> _targetDualGridTilemaps = new List<CDualGridTilemap>();
        
        private bool _showDataTileBoundaries = false;
        
        private bool _showRenderTileBoundaries = false;

        private bool _showRenderTileConnections = false;

        /// <summary>
        ///     Creates a grid to be used as the foundation for a dual grid.
        /// </summary>
        /// <returns>a new Grid object named "Dual Grid"</returns>
        public static Grid CreateNewDualGrid()
        {
            var dualGrid = new GameObject("Dual Grid");
            return dualGrid.AddComponent<Grid>();
        }

        /// <summary>
        /// Creates a new dual grid tilemap, utilizes lazy loading; if nothing or null is passed then a new grid is created
        /// </summary>
        /// <param name="grid">The dual grid object, if nothing is passed then a new dual grid is created</param>
        /// <returns>a new CDualGridTilemap</returns>
        public static CDualGridTilemap CreateNewDualGridTilemap(Grid grid = null)
        {
            if (grid == null)
                grid = CreateNewDualGrid();

            //TODO:Review
            var newDataTilemap = new GameObject("DataTilemap");
            newDataTilemap.AddComponent<Tilemap>();
            newDataTilemap.transform.parent = grid.transform;
            var cDualGridTilemap = newDataTilemap.AddComponent<CDualGridTilemap>();

            InitializeRenderTilemap(cDualGridTilemap);

            return cDualGridTilemap;
        }
        
        private static void InitializeRenderTilemap(CDualGridTilemap cDualGridTilemap)
        {
            if (cDualGridTilemap == null)
                return;

            if (cDualGridTilemap.RenderTilemap == null)
            {
                CreateRenderTilemapObject(cDualGridTilemap);
            }
                
            //Destroy Tilemap Renderer in data tilemap
            TilemapRenderer renderer = cDualGridTilemap.GetComponent<TilemapRenderer>();
            DestroyComponentIfExists(renderer, "Dual Grid Tilemaps cannot have TilemapRenderers in the same GameObject. TilemapRenderer has been destroyed.");
            
            //Update the tilemap collider components
            TilemapCollider2D tilemapColliderFromDataTilemap = cDualGridTilemap.DataTilemap.GetComponent<TilemapCollider2D>();
            TilemapCollider2D tilemapColliderFromRenderTilemap = cDualGridTilemap.RenderTilemap.GetComponent<TilemapCollider2D>();
            
            //Check if the tilemap colliders are enabled, if not destroy data and render tilemap colliders if they exist
            
            //Check if the dualGrid dataTiles to see if they are the following types: None, Sprite, and Grid and handle these cases
        }

        private void OnEnable()
        {
            //we want this to fail if target is not the expected type
            _targetDualGridTilemap = (CDualGridTilemap)target;
            
            _hasMultipleTargets = targets.Length > 1;

            if (_hasMultipleTargets)
            {
                _targetDualGridTilemaps = targets.Cast<CDualGridTilemap>().ToList();
            }
            else
            {
                _targetDualGridTilemaps = new List<CDualGridTilemap>()
                {
                    target as CDualGridTilemap
                };
            }

            foreach (CDualGridTilemap dualGridTilemap in _targetDualGridTilemaps)
            {
                InitializeRenderTilemap(dualGridTilemap);
            }
        }

        /// <summary>
        ///     Creates a new Render Tilemap that is offset from the provided dataTilemap by half a tile in the x,y 
        /// </summary>
        /// <param name="targetDataTilemap">The dataTilemap we want to render</param>
        /// <returns>A new Render Tilemap</returns>
        internal static GameObject CreateRenderTilemapObject(CDualGridTilemap targetDataTilemap)
        {
            GameObject renderTilemapGO = new GameObject("RenderTilemap")
            {
                transform =
                {
                    parent = targetDataTilemap.transform,
                    localPosition = new Vector3(-0.5f, -0.5f, 0f)
                }
            };

            renderTilemapGO.AddComponent<Tilemap>();
            renderTilemapGO.AddComponent<TilemapRenderer>();
            
            return renderTilemapGO;
        }

        private static void DestroyComponentIfExists(Component component, string warningMessage = null)
        {
            if (component == null) 
                return;
            
            if (warningMessage != null)
                Debug.LogWarning(warningMessage);
                
            DestroyImmediate(component);
        }
    }
}
