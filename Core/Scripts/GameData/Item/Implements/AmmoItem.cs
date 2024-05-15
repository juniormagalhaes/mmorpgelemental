﻿using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.AMMO_ITEM_FILE, menuName = GameDataMenuConsts.AMMO_ITEM_MENU, order = GameDataMenuConsts.AMMO_ITEM_ORDER)]
    public partial class AmmoItem : BaseItem, IAmmoItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_AMMO.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Ammo; }
        }

        [Category(2, "Ammo Settings")]
        [SerializeField]
        [Tooltip("Ammo type data")]
        private AmmoType ammoType = null;
        public AmmoType AmmoType
        {
            get { return ammoType; }
        }

        [SerializeField]
        [Tooltip("Increasing damages stats while attacking by weapon which put this item")]
        private DamageIncremental[] increaseDamages = new DamageIncremental[0];
        public DamageIncremental[] IncreaseDamages
        {
            get { return increaseDamages; }
        }

        [SerializeField]
        [Tooltip("If this value > 0 it will override weapon's ammo capacity when reload")]
        private int overrideAmmoCapacity = 0;
        public int OverrideAmmoCapacity
        {
            get { return overrideAmmoCapacity; }
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddAmmoTypes(AmmoType);
        }
    }
}
