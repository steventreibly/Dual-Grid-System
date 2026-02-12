using System.Collections.Generic;
using System.Linq;
using DualGrid.Editor.Extensions;
using DualGrid.Runtime.Components;
using DualGrid.Tiles;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DualGrid.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CDualGridTilemap))]
    public class CDualGridEditor : UnityEditor.Editor
    {
        private readonly GUIContent _renderTileGUIContent = new GUIContent("Dual Grid Rule Tile",
            "The Render Tile that will be applied in the Render Tilemap.");

        private readonly GUIContent _enableTilemapColliderGUIContent = new GUIContent("Enable Tilemap Collider",
            "If a TilemapCollider2D should be active based on the Dual Grid Rule Tile's default collider type.");

        private readonly GUIContent _gameObjectOriginGUIContent = new GUIContent("Game Object Origin",
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
        
        /// <summary>
        ///     Initialize the dual grid with any relevant data to its functionality and remove any that should not exist
        /// </summary>
        /// <param name="cDualGridTilemap">the dual grid tilemap to be initialized</param>
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
            
            UpdateTilemapColliderComponents(cDualGridTilemap);
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

        /// <summary>
        ///     Checks for a Renderer attached to a Data Tilemap, if it exists it is destroyed
        /// </summary>
        /// <param name="cDualGridTilemap"></param>
        private static void DestroyTilemapRendererInDataTilemap(CDualGridTilemap cDualGridTilemap)
        {
            TilemapRenderer renderer = cDualGridTilemap.GetComponent<TilemapRenderer>();
            //DestroyComponentIfExists(renderer, "Dual Grid Tilemaps cannot have Tilemap Renderers in the same GameObject. TilemapRenderer has been destroyed.");
        }

        /// <summary>
        ///     Checks to ensure that the correct attributes are set for the Render and Data Tilemaps, if not, they are corrected.
        /// </summary>
        /// <param name="cDualGridTilemap"></param>
        internal static void UpdateTilemapColliderComponents(CDualGridTilemap cDualGridTilemap)
        {
            //Update the tilemap collider components
            TilemapCollider2D tilemapColliderFromDataTilemap = cDualGridTilemap.DataTilemap.GetComponent<TilemapCollider2D>();
            TilemapCollider2D tilemapColliderFromRenderTilemap = cDualGridTilemap.RenderTilemap.GetComponent<TilemapCollider2D>();
            
            string warningMessage = "";
            
            //Check if the tilemap colliders are enabled, if not destroy data and render tilemap colliders if they exist
            if(!cDualGridTilemap.EnableTilemapCollider)
            {
                warningMessage =
                    "Dual Grid Tilemaps cannot have Tilemap Colliders 2D if not enabled in Dual Grid Tilemap Component";
                DestroyComponentIfExists(tilemapColliderFromDataTilemap, warningMessage);
                DestroyComponentIfExists(tilemapColliderFromRenderTilemap, warningMessage);
                return;
            }

            //Check if the dualGrid dataTiles to see if they are the following types: None, Sprite, and Grid and handle these cases
            switch (cDualGridTilemap.DataTile.colliderType)
            {
                case Tile.ColliderType.None:
                    warningMessage =
                        "Dual Grid Tilemaps cannot have Tilemap Colliders 2D if Dual Grid Tile has collider type set to none.";
                    DestroyComponentIfExists(tilemapColliderFromDataTilemap, warningMessage);
                    DestroyComponentIfExists(tilemapColliderFromRenderTilemap, warningMessage);
                    break;
                case Tile.ColliderType.Grid:
                    warningMessage =
                        "Dual Grid Tilemaps cannot have Tilemap Colliders 2D in the Render Tilemap if Dual Grid tile has collider type set to Grid";
                    if (tilemapColliderFromDataTilemap == null)
                    {
                        cDualGridTilemap.DataTilemap.gameObject.AddComponent<TilemapCollider2D>();
                    }
                    DestroyComponentIfExists(tilemapColliderFromRenderTilemap, warningMessage);
                    
                    break;
                case Tile.ColliderType.Sprite:
                    warningMessage =
                        "Dual Grid Tilemaps cannot have Tilemap Colliders 2D in the Data Tilemap if Dual Grid Tile has collider type set to Sprite";
                    if (tilemapColliderFromRenderTilemap == null)
                    {
                        cDualGridTilemap.RenderTilemap.gameObject.AddComponent<TilemapCollider2D>();
                    }
                    DestroyComponentIfExists(tilemapColliderFromDataTilemap, warningMessage);
                    break;
            }
        }

        /// <summary>
        ///     Add our Dual grids to a list and begin their initialization
        /// </summary>
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
        ///     If the component exists then destroy it immediately
        /// </summary>
        /// <remarks>ONLY FOR EDIT MODE USE</remarks>
        /// <param name="component">The component to be destroyed</param>
        /// <param name="warningMessage">The associated log message</param>
        private static void DestroyComponentIfExists(Component component, string warningMessage = null)
        {
            if (component == null) 
                return;
            
            if (warningMessage != null)
                Debug.LogWarning(warningMessage);
                
            DestroyImmediate(component);
        }

        /// <inheritdoc/>
        /// <remarks>
        ///     Creates a custom inspector to edit Dual Grid Tilemaps and show visualization helpers in scene
        /// </remarks>
        public override void OnInspectorGUI()
        {
            if (_hasMultipleTargets)
            {
                Undo.RecordObjects(_targetDualGridTilemaps.ToArray(), 
                    $"Updated {_targetDualGridTilemaps.Count} Dual Grid Tilemap Components");
            }
            else
            {
                Undo.RecordObject(_targetDualGridTilemap, $"Updated {_targetDualGridTilemap.name} Dual Grid Rule Tile");
            }
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _hasMultipleTargets &&
                                       _targetDualGridTilemaps.HasDifferentValues(cDualGridTilemap =>
                                           cDualGridTilemap.RenderTile);
            var renderTile = EditorGUILayout.ObjectField(_renderTileGUIContent, _targetDualGridTilemap.RenderTile, typeof(DualGridRuleTile), false) as DualGridRuleTile;

            if (EditorGUI.EndChangeCheck())
            {
                foreach (CDualGridTilemap dualGridTilemap in _targetDualGridTilemaps)
                {
                    dualGridTilemap.RenderTile = renderTile;
                    dualGridTilemap.DataTilemap.RefreshAllTiles();
                    dualGridTilemap.RefreshRenderTilemap();
                }
            }
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _hasMultipleTargets &&
                                       _targetDualGridTilemaps.HasDifferentValues(cDualGridTilemap =>
                                           cDualGridTilemap.EnableTilemapCollider);
            var enableTilemapCollider =
                EditorGUILayout.Toggle(_enableTilemapColliderGUIContent, _targetDualGridTilemap.EnableTilemapCollider);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (CDualGridTilemap dualGridTilemap in _targetDualGridTilemaps)
                {
                    dualGridTilemap.EnableTilemapCollider = enableTilemapCollider;
                    UpdateTilemapColliderComponents(dualGridTilemap);
                }
            }
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _hasMultipleTargets &&
                                       _targetDualGridTilemaps.HasDifferentValues(cDualGridtilemap =>
                                           cDualGridtilemap.GameObjectOrigin);
            var gameObjectOrigin =
                (OriginEnum)EditorGUILayout.EnumPopup(_gameObjectOriginGUIContent, _targetDualGridTilemap.GameObjectOrigin);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (CDualGridTilemap dualGridTilemap in _targetDualGridTilemaps)
                {
                    dualGridTilemap.GameObjectOrigin = gameObjectOrigin;
                    dualGridTilemap.DataTilemap.RefreshAllTiles();
                    dualGridTilemap.RefreshRenderTilemap();
                }
            }
            
            GUILayout.Space(5);
            GUILayout.Label("Tools", EditorStyles.boldLabel);

            if (EditorGUI.EndChangeCheck())
            {
                //data has changed in the targetDualGridTilemap since it was last saved
                EditorUtility.SetDirty(_targetDualGridTilemap);
            }
            
            EditorGUI.BeginChangeCheck();
            GUILayout.Label("Visualization Handles", EditorStyles.boldLabel);
            _showDataTileBoundaries = EditorGUILayout.Toggle("Data Tile Boundaries", _showDataTileBoundaries);
            _showRenderTileBoundaries = EditorGUILayout.Toggle("Render Tile Boundaries", _showRenderTileBoundaries);
            _showRenderTileConnections = EditorGUILayout.Toggle("Render Tile Connections", _showRenderTileConnections);

            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }

        /// <summary>
        ///     Draws all of tiles within the targeted dual grid tilemap
        /// </summary>
        private void OnSceneGUI()
        {
            foreach (CDualGridTilemap dualGridTilemap in _targetDualGridTilemaps)
            {
                DrawDataTileHandles(dualGridTilemap);
                DrawRenderTileHandles(dualGridTilemap);
            }
        }

        /// <summary>
        ///     Draws all the Data Tile boundaries within the bounds of the Data Tilemap
        /// </summary>
        /// <param name="dualGridTilemap"></param>
        private void DrawDataTileHandles(CDualGridTilemap dualGridTilemap)
        {
            if (!_showDataTileBoundaries)
                return;

            foreach (Vector3Int position in dualGridTilemap.DataTilemap.cellBounds.allPositionsWithin)
            {
                //if there is no tile at this position in the tilemap continue to the next position
                if(!dualGridTilemap.DataTilemap.HasTile(position))
                    continue;

                Vector3 tileCenter = dualGridTilemap.DataTilemap.GetCellCenterWorld(position);

                Handles.color = Color.red;
                DrawTileBoundaries(dualGridTilemap.DataTilemap, tileCenter, thickness: 3);
            }
        }

        /// <summary>
        ///     Draws all the Render Tile boundaries and Render Tile connections within the bounds of the Render Tilemap
        /// </summary>
        /// <param name="dualGridTilemap"></param>
        private void DrawRenderTileHandles(CDualGridTilemap dualGridTilemap)
        {
            if (!_showRenderTileBoundaries && !_showRenderTileConnections)
                return;

            foreach (var renderTilePosition in dualGridTilemap.RenderTilemap.cellBounds.allPositionsWithin)
            {
                //if there is no tile at this position in the tilemap continue to the next position
                if (!dualGridTilemap.RenderTilemap.HasTile(renderTilePosition))
                    continue;

                Vector3 tileCenter = dualGridTilemap.RenderTilemap.GetCellCenterWorld(renderTilePosition);
                
                Handles.color = Color.blue;
                if(_showRenderTileBoundaries)
                    DrawTileBoundaries(dualGridTilemap.RenderTilemap, tileCenter,thickness: 1);

                Handles.color = Color.lawnGreen;
                if (_showRenderTileConnections)
                    DrawRenderTileConnections(dualGridTilemap.DataTilemap ,dualGridTilemap.RenderTilemap, renderTilePosition, tileCenter);
            }
        }
        
        /// <summary>
        ///     Draws a Quad to represent a tile visually in the grid
        /// </summary>
        /// <param name="tilemap">The tilemap associated with this Tile's boundaries</param>
        /// <param name="tileCenter">The center of the tile in world space coordinates</param>
        /// <param name="thickness">The thickness of the lines connecting the Quad</param>
        private void DrawTileBoundaries(Tilemap tilemap, Vector3 tileCenter, float thickness)
        {
            if (tilemap == null)
                return;
            
            Handles.DrawSolidDisc(tileCenter, Vector3.forward, radius: 0.05f);
            float squareSizeX = (tilemap.cellSize.x * tilemap.transform.lossyScale.x) * 0.5f;
            float squareSizeY = (tilemap.cellSize.y * tilemap.transform.lossyScale.y) * 0.5f;

            Vector3 topLeft = tileCenter + new Vector3(-squareSizeX, squareSizeY, 0);
            Vector3 topRight = tileCenter + new Vector3(squareSizeX, squareSizeY, 0);
            Vector3 bottomLeft = tileCenter + new Vector3(-squareSizeX, -squareSizeY, 0);
            Vector3 bottomRight = tileCenter + new Vector3(squareSizeX, -squareSizeY, 0);
            
            Handles.DrawLine(topLeft, topRight, thickness);
            Handles.DrawLine(topRight, bottomRight, thickness);
            Handles.DrawLine(bottomRight, bottomLeft, thickness);
            Handles.DrawLine(bottomLeft, topLeft, thickness);
        }
        
        /// <summary>
        ///     Shows the connection between the Render Tiles to the Data Tile they are offset from
        /// </summary>
        /// <param name="dataTilemap">tilemap containing our data tile grid</param>
        /// <param name="renderTilemap">tilemap containing our render tile grid</param>
        /// <param name="renderTilePosition">the position of the render tile</param>
        /// <param name="tileCenter">the center of the render tile in world space coordinates</param>
        private void DrawRenderTileConnections(Tilemap dataTilemap, Tilemap renderTilemap, Vector3Int renderTilePosition, Vector3 tileCenter)
        {
            if (dataTilemap == null || renderTilemap == null)
                return;

            Vector3Int[] dataTilemapPositions = DualGridUtils.GetDataTilePositions(renderTilePosition);

            foreach (Vector3Int dataTilePosition in dataTilemapPositions)
            {
                if (!dataTilemap.HasTile(dataTilePosition))
                    continue;

                Vector3Int dataTileOffset = dataTilePosition - renderTilePosition;
                Vector3Int neighborOffset = DualGridUtils.ConvertDataTileOffsetToNeighborOffset(dataTileOffset);
                
                Vector3 corner = tileCenter + new Vector3(neighborOffset.x * renderTilemap.cellSize.x * 0.3f, neighborOffset.y * renderTilemap.cellSize.y * 0.3f, 0.0f);

                DrawArrow(tileCenter, corner);
            }

            static void DrawArrow(Vector3 start, Vector3 end, float arrowHeadLength = 0.15f,
                float arrowHeadAngle = 30f)
            {
                //Draw the arrow body line
                Handles.DrawLine(start, end);
                
                //Calculate the direction of the arrow body line
                Vector3 direction = (end - start).normalized;
                
                //Calculate the points for the arrowhead
                Vector3 right = Quaternion.Euler(0, 0, arrowHeadAngle) * -direction;
                Vector3 left = Quaternion.Euler(0, 0, -arrowHeadAngle) * -direction;
                
                //Draw arrowhead lines
                Handles.DrawLine(end, end + right * arrowHeadLength);
                Handles.DrawLine(end, end + left * arrowHeadLength);
            }
        }
    }
}
