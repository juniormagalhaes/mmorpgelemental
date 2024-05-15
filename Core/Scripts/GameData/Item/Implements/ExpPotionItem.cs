﻿using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.EXP_POTION_ITEM_FILE, menuName = GameDataMenuConsts.EXP_POTION_ITEM_MENU, order = GameDataMenuConsts.EXP_POTION_ITEM_ORDER)]
    public partial class ExpPotionItem : BaseItem, IPotionItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_POTION.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Potion; }
        }

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

        [Header("Potion Configs")]
        [SerializeField]
        private Buff buff = Buff.Empty;
        public Buff? BuffData
        {
            get { return buff; }
        }

        [SerializeField]
        private int exp = 0;
        public int Exp
        {
            get { return exp; }
        }

        [SerializeField]
        private string autoUseSettingKey;
        public string AutoUseKey
        {
            get { return autoUseSettingKey; }
        }

        [SerializeField]
        private float useItemCooldown = 0f;
        public float UseItemCooldown
        {
            get { return useItemCooldown; }
        }

        public void UseItem(BaseCharacterEntity characterEntity, int itemIndex, CharacterItem characterItem)
        {
            if (!characterEntity.CanUseItem() || characterItem.level <= 0 || !characterEntity.DecreaseItemsByIndex(itemIndex, 1, false))
                return;
            characterEntity.FillEmptySlots();
            characterEntity.ApplyBuff(DataId, BuffType.PotionBuff, characterItem.level, characterEntity.GetInfo(), null);
            characterEntity.RewardExp(Exp, 1, RewardGivenType.None, 1, 1);
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
    }
}
