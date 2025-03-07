﻿using UnityEngine;

namespace MultiplayerARPG
{
    public partial interface IWeaponItem : IEquipmentItem
    {
        /// <summary>
        /// Weapon type data
        /// </summary>
        WeaponType WeaponType { get; }
        /// <summary>
        /// Off-hand equipment models, these models will be instantiated when equipping this item to off-hand (left-hand)
        /// </summary>
        EquipmentModel[] OffHandEquipmentModels { get; }
        /// <summary>
        /// These models will be instantiated when this item not being equipped or sheathed
        /// </summary>
        EquipmentModel[] SheathModels { get; }
        /// <summary>
        /// These models will be instantiated when this item not being equipped or sheathed
        /// </summary>
        EquipmentModel[] OffHandSheathModels { get; }
        /// <summary>
        /// Damange amount which will be used when attacking characters, buildings and so on
        /// </summary>
        DamageIncremental DamageAmount { get; }
        /// <summary>
        /// Damage amount which will be used when attacking harvestable entities
        /// </summary>
        IncrementalMinMaxFloat HarvestDamageAmount { get; }
        /// <summary>
        /// This will be multiplied with character's movement speed while reloading this weapon
        /// </summary>
        float MoveSpeedRateWhileReloading { get; }
        /// <summary>
        /// This will be multiplied with character's movement speed while charging this weapon
        /// </summary>
        float MoveSpeedRateWhileCharging { get; }
        /// <summary>
        /// This will be multiplied with character's movement speed while attacking with this weapon
        /// </summary>
        float MoveSpeedRateWhileAttacking { get; }
        MovementRestriction MovementRestrictionWhileReloading { get; }
        MovementRestriction MovementRestrictionWhileCharging { get; }
        MovementRestriction MovementRestrictionWhileAttacking { get; }
        ActionRestriction AttackRestriction { get; }
        ActionRestriction ReloadRestriction { get; }
        /// <summary>
        /// You can set ammo items into this list to use it as weapon instead of the one which setup on weapon type's require ammo type
        /// This setting is useful for shooter games which can have the same type of weapon (eg. machine-gun for 20 guns) but can be reloaded by differences ammo items
        /// </summary>
        BaseItem[] AmmoItems { get; }
        /// <summary>
        /// How many ammo can store in the gun's magazine
        /// </summary>
        int AmmoCapacity { get; }
        /// <summary>
        /// Weapon ability such as zoom, change how to fire, change launch clip. (For now, it has only zoom)
        /// </summary>
        BaseWeaponAbility[] WeaponAbilities { get; }
        /// <summary>
        /// Crosshair setting
        /// </summary>
        CrosshairSetting CrosshairSetting { get; }
        /// <summary>
        /// Audio clip which will plays when fire and bullet launched
        /// </summary>
        AudioClipWithVolumeSettings LaunchClip { get; }
        /// <summary>
        /// Audio clip which will plays when reload an ammo
        /// </summary>
        AudioClipWithVolumeSettings ReloadClip { get; }
        /// <summary>
        /// Audio clip which will plays when reloaded an ammo
        /// </summary>
        AudioClipWithVolumeSettings ReloadedClip { get; }
        /// <summary>
        /// Audio clip which will plays when there is no ammo
        /// </summary>
        AudioClipWithVolumeSettings EmptyClip { get; }
        /// <summary>
        /// How to fire
        /// </summary>
        FireType FireType { get; }
        /// <summary>
        /// If this value > 0, it will fire by duration which being calculated by this value, default duration calculation formula is `60f / rate of fire`
        /// </summary>
        float RateOfFire { get; }
        /// <summary>
        /// If this value > 0, it will reload by using this duration, NOT by animation length
        /// </summary>
        float ReloadDuration { get; }
        /// <summary>
        /// Random stagger from aiming position, then when shoot actual shot position will be {aim position} + {randomed stagger}
        /// </summary>
        Vector2 FireStagger { get; }
        /// <summary>
        /// Amount of bullets that will be launched when fire onnce, will be used for shotgun items
        /// </summary>
        byte FireSpread { get; }
        /// <summary>
        /// Minimum charge duration to attack
        /// </summary>
        float ChargeDuration { get; }
        /// <summary>
        /// If this is `TRUE`, character's item will be destroyed after fired, will be used for grenade items
        /// </summary>
        bool DestroyImmediatelyAfterFired { get; }
    }
}
