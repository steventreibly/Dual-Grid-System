using System;
using DualGrid.Runtime.Components;
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
        public Texture2D OriginalTexture { get; internal set; }
        
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
