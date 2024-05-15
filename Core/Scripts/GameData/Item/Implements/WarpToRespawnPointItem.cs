﻿using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.WARP_TO_RESPAWN_POINT_ITEM_FILE, menuName = GameDataMenuConsts.WARP_TO_RESPAWN_POINT_ITEM_MENU, order = GameDataMenuConsts.WARP_TO_RESPAWN_POINT_ITEM_ORDER)]
    public class WarpToRespawnPointItem : BaseItem, IUsableItem
    {
        [Category(2, "Requirements")]
        [SerializeField]
        private ItemRequirement requirement = default;
        public ItemRequirement Requirement
        {
            get { return requirement; }
        }

        [System.NonSerialized]
        private Dictionary<Attribute, float> _cacheRequireAttributeAmounts = null;
        public Dictionary<Attribute, float> RequireAttributeAmounts
        {
            get
            {
                if (_cacheRequireAttributeAmounts == null)
                    _cacheRequireAttributeAmounts = GameDataHelpers.CombineAttributes(requirement.attributeAmounts, new Dictionary<Attribute, float>(), 1f);
                return _cacheRequireAttributeAmounts;
            }
        }

        [SerializeField]
        private float useItemCooldown = 0f;
        public float UseItemCooldown
        {
            get { return useItemCooldown; }
        }
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_CONSUMABLE.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Potion; }
        }

        public bool HasCustomAimControls()
        {
            return false;
        }

        public AimPosition UpdateAimControls(Vector2 aimAxes, params object[] data)
        {
            return default;
        }

        public void FinishAimControls(bool isCancel)
        {

        }

        public bool IsChanneledAbility()
        {
            return false;
        }

        public void UseItem(BaseCharacterEntity characterEntity, int itemIndex, CharacterItem characterItem)
        {
            BasePlayerCharacterEntity playerCharacterEntity = characterEntity as BasePlayerCharacterEntity;
            if (playerCharacterEntity == null || !characterEntity.CanUseItem() || characterItem.level <= 0 || !characterEntity.DecreaseItemsByIndex(itemIndex, 1, false))
                return;
            GameInstance.ServerCharacterHandlers.Respawn(0, playerCharacterEntity);
        }
    }
}
