using System;
using DualGrid.Runtime.Components;
using DualGrid.Runtime.Extensions;
using DualGrid.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DualGrid.Tiles
{
    /// <summary>
    /// Custom <see cref="RuleTile"/> used by the <see cref="CDualGridTilemap"/> to generate tiles in the Render Tilemap
    ///<remarks>
    /// Avoid using this tile in a palette, any other data tile can be used.
    /// This tile type will be used in all Render Tilemaps.
    /// </remarks>
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "DualGridRuleTile", menuName = "Scriptable Objects/DualGridRuleTile")]
    public class DualGridRuleTile : RuleTile<DualGridNeighbor>
    {
        [field: SerializeField, HideInInspector]
        private DualGridDataTile _dataTile;
        
        private CDualGridTilemap _cDualGridTilemap;
        
        private Tilemap _dataTilemap;

        public DualGridDataTile DataTile {
            get
            {
                if (_dataTile == null)
                {
                    Debug.LogError($"The DualGridTuleTile {name} does not have an associated Data Tile.", this);
                    throw new ArgumentNullException(
                        $"TheDualGridRuleTile {name} does not have an associated Data Tile.");
                }

                return _dataTile;
            } 
        }
        
        [field: SerializeField, HideInInspector] 
        public Texture2D OriginalTexture { get; set; }
        
        /// <summary>
        ///     Force sets the actual Data Tilemap before updating the tile
        /// </summary>
        /// <param name="position">The grid coordinates of the tile being evaluated</param>
        /// <param name="tilemap">The tilemap the tile belongs to</param>
        /// <param name="tileData">The data of the tile we want to set</param>
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            SetDataTilemap(tilemap);

            var identity = Matrix4x4.identity;
            Matrix4x4 transform = identity;

            tileData.sprite = m_DefaultSprite;
            tileData.gameObject = m_DefaultGameObject;
            tileData.colliderType = m_DefaultColliderType;
            tileData.flags = TileFlags.LockTransform;

            bool shouldBeInRenderTilemap = _cDualGridTilemap == null ||
                                           _cDualGridTilemap.GameObjectOrigin == OriginEnum.RenderTilemap;
            foreach (TilingRule rule in m_TilingRules)
            {
                if (RuleMatches(rule, position, tilemap, ref transform))
                {
                    switch (rule.m_Output)
                    {
                        case TilingRuleOutput.OutputSprite.Single:
                        case TilingRuleOutput.OutputSprite.Animation:
                            tileData.sprite = rule.m_Sprites[0];
                            break;
                        case TilingRuleOutput.OutputSprite.Random:
                            //TODO:Steven Review Perlin offset value
                            //Generate a random index
                            int index = Mathf.Clamp(
                                Mathf.FloorToInt(GetPerlinValue(position, rule.m_PerlinScale, 100000f) *
                                                 rule.m_Sprites.Length), 0, rule.m_Sprites.Length - 1);
                            tileData.sprite = rule.m_Sprites[index];
                            if (rule.m_RandomTransform != TilingRuleOutput.Transform.Fixed)
                            {
                                transform = ApplyRandomTransform(rule.m_RandomTransform, transform, rule.m_PerlinScale, position);
                            }

                            break;
                    }
                    
                    tileData.transform = transform;
                    tileData.gameObject = shouldBeInRenderTilemap ? rule.m_GameObject : null;
                    break;
                }
            }
        }

        /// <summary>
        ///     Refreshes the <see cref="DataTile"/> with this <see cref="DualGridRuleTile"/>'s configuration.
        /// </summary>
        /// <returns>The refreshed data tile</returns>
        public virtual DualGridDataTile RefreshDataTile()
        {
            if(_dataTile == null)
            {
                _dataTile = ScriptableObject.CreateInstance<DualGridDataTile>();
            }

            _dataTile.colliderType = this.m_DefaultColliderType;
            _dataTile.gameObject = this.m_DefaultGameObject;
            
            return _dataTile;
        }

        /// <summary>
        ///     Used to determine if there is a rule match given a Tiling rule and neighboring Tiles
        /// </summary>
        /// <param name="ruleToValidate">The Tiling Rule to match with</param>
        /// <param name="renderTilePosition">The position of the tile on the tilemap</param>
        /// <param name="tilemap">the tilemap to match with</param>
        /// <param name="transform">transform matrix which will match the Rule</param>    
        /// <returns>returns true if there is a match and false if not</returns>
        public override bool RuleMatches(TilingRule ruleToValidate, Vector3Int renderTilePosition, ITilemap tilemap,
            ref Matrix4x4 transform)
        {
            if (GetDataTilemap(tilemap) == null)
                return false;

            Vector3Int[] dataTilemapPositions = DualGridUtils.GetDataTilePositions(renderTilePosition);

            foreach (Vector3Int dataTilePosition in dataTilemapPositions)
            {
                if (!DoesRuleMatchWithDataTile(ruleToValidate, dataTilePosition, renderTilePosition))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ruleToValidate"></param>
        /// <param name="dataTilePosition"></param>
        /// <param name="renderTilePosition"></param>
        /// <returns></returns>
        private bool DoesRuleMatchWithDataTile(TilingRule ruleToValidate, Vector3Int dataTilePosition,
            Vector3Int renderTilePosition)
        {
            Vector3Int dataTileOffset = dataTilePosition - renderTilePosition;
            
            int neighborIndex = ruleToValidate.GetNeighborIndex(dataTileOffset);
            
            //If no neighbor is defined, then it will match with anything
            if (neighborIndex == -1)
                return true;

            //Ensures that EditorPreviewTiles are only considered when running inside the Unity Editor
    #if UNITY_EDITOR
            var neighborDataTile = _dataTilemap.GetEditorPreviewTile(dataTilePosition);
            if (neighborDataTile == null) 
                neighborDataTile = _dataTilemap.GetTile(dataTilePosition);
    #else
            var neighborDataTile = _dataTilemap.GetTile(dataTilePosition);
    #endif
            
            return RuleMatch(ruleToValidate.m_Neighbors[neighborIndex], neighborDataTile);
        }

        /// <summary>
        ///     Checks if there is a match given the neighbor matching rule and a Tile.
        /// </summary>
        /// <param name="neighbor">Neighbor matching rule.</param>
        /// <param name="other">Tile to match.</param>
        /// <returns>True if there is a match, False if not.</returns>
        public override bool RuleMatch(int neighbor, TileBase other)
        {
            //Is the neighbor a preview tile that is currently empty
            bool isEmptyPreviewTile = other is DualGridPreviewTile dualGridPreviewTile &&
                                      dualGridPreviewTile.IsFilled == false;

            return neighbor switch
            {
                DualGridNeighbor.Filled => !isEmptyPreviewTile && other != null,
                DualGridNeighbor.NotFilled => isEmptyPreviewTile || other == null, 
                _ => true //  "_" here means any other case, essentially our "default" case for which we return true
            };
        }

        /// <summary>
        ///     data tilemap getter, which will attempt to set it using the <paramref name="tilemap"/> if the <see cref="_dataTilemap"/> is null
        /// </summary>
        /// <para>
        ///     This is an attempt to combat Unity's execution order as there are certain moments where <see cref="StartUp"/> in RuleTile base has yet to be called but
        ///     the tile is being updated. If the data tilemap would be null, the rule matching will not work properly.
        /// </para>
        /// <param name="tilemap"></param>
        /// <returns>the <see cref="_dataTilemap"/> field</returns>
        private Tilemap GetDataTilemap(ITilemap tilemap)
        {
            if (_cDualGridTilemap == null || _cDualGridTilemap.DataTilemap == null)
            {
                SetDataTilemap(tilemap);
            }
            
            return _dataTilemap;
        }

        /// <summary>
        ///     Sets our data tilemap the Dual Grid Component's data tilemap
        /// </summary>
        /// <param name="tilemap"></param>
        private void SetDataTilemap(ITilemap tilemap)
        {
            var originTilemap = tilemap.GetComponent<Tilemap>();

            _cDualGridTilemap = originTilemap.GetComponentInParent<CDualGridTilemap>();

            if (_cDualGridTilemap != null)
            {
                _dataTilemap = _cDualGridTilemap.DataTilemap;
            }
        }
    }
}
