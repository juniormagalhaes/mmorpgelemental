﻿using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public delegate void IsUpdateEntityComponentsDelegate(
        bool isUpdate);

    public delegate void NetworkDestroyDelegate(
        byte reasons);

    public delegate void ReceiveDamageDelegate(
        HitBoxPosition position,
        Vector3 fromPosition,
        IGameEntity attacker,
        Dictionary<DamageElement, MinMaxFloat> damageAmounts,
        CharacterItem weapon,
        BaseSkill skill,
        int skillLevel);

    public delegate void ReceivedDamageDelegate(
        HitBoxPosition position,
        Vector3 fromPosition,
        IGameEntity attacker,
        CombatAmountType combatAmountType,
        int totalDamage,
        CharacterItem weapon,
        BaseSkill skill,
        int skillLevel,
        CharacterBuff buff,
        bool isDamageOverTime);

    public delegate void NotifyEnemySpottedDelegate(
        BaseCharacterEntity enemy);

    public delegate void NotifyEnemySpottedByAllyDelegate(
        BaseCharacterEntity ally,
        BaseCharacterEntity enemy);

    public delegate void AppliedRecoveryAmountDelegate(
        EntityInfo causer,
        int amount);

    public delegate void AttackRoutineDelegate(
        bool isLeftHand,
        CharacterItem weapon,
        int simulateSeed,
        byte triggerIndex,
        DamageInfo damageInfo,
        Dictionary<DamageElement, MinMaxFloat> damageAmounts,
        AimPosition aimPosition);

    public delegate void UseSkillRoutineDelegate(
        BaseSkill skill,
        int level,
        bool isLeftHand,
        CharacterItem weapon,
        int simulateSeed,
        byte triggerIndex,
        Dictionary<DamageElement, MinMaxFloat> damageAmounts,
        uint targetObjectId,
        AimPosition aimPosition);

    public delegate void LaunchDamageEntityDelegate(
        bool isLeftHand,
        CharacterItem weapon,
        int simulateSeed,
        byte triggerIndex,
        byte spreadIndex,
        Dictionary<DamageElement, MinMaxFloat> damageAmounts,
        BaseSkill skill,
        int skillLevel,
        AimPosition aimPosition);

    public delegate void ApplyBuffDelegate(
        BuffType type,
        int dataId,
        int level,
        EntityInfo buffApplier);

    public delegate void CharacterStatsDelegate(
        ref CharacterStats a,
        CharacterStats b);

    public delegate void CharacterStatsAndNumberDelegate(
        ref CharacterStats a,
        float b);

    public delegate void RandomCharacterStatsDelegate(
        System.Random random,
        ItemRandomBonus randomBonus,
        bool isRateStats,
        ref CharacterStats stats,
        ref int appliedAmount);

    public delegate void DamageOriginPreparedDelegate(
        int simulateSeed,
        byte triggerIndex,
        byte spreadIndex,
        Vector3 position,
        Vector3 direction,
        Quaternion rotation);

    public delegate void DamageHitDelegate(
        int simulateSeed,
        byte triggerIndex,
        byte spreadIndex,
        uint objectId,
        byte hitBoxIndex,
        Vector3 hitPoint);

    public delegate void CanMoveDelegate(
        ref bool canMove);

    public delegate void CanSprintDelegate(
        ref bool canSprint);

    public delegate void CanWalkDelegate(
        ref bool canWalk);

    public delegate void CanCrouchDelegate(
        ref bool canCrouch);

    public delegate void CanCrawlDelegate(
        ref bool canCrawl);

    public delegate void CanJumpDelegate(
        ref bool canJump);

    public delegate void CanTurnDelegate(
        ref bool canTurn);

    public delegate void JumpForceAppliedDelegate(
        float verticalVelocity);

    public delegate void UpdateEquipmentModelsDelegate(
        BaseCharacterModel characterModel,
        Dictionary<string, EquipmentModel> showingModels,
        Dictionary<string, EquipmentModel> storingModels,
        HashSet<string> unequippingSockets);

    public delegate void EquipmentModelDelegate(
        EquipmentModel model,
        GameObject instantiatedObject,
        BaseEquipmentEntity instantiatedEntity,
        EquipmentInstantiatedObjectGroup instantiatedObjectGroup,
        EquipmentContainer equipmentContainer);

    public delegate void CalculatedItemBuffDelegate(
        CalculatedItemBuff calculatedItemBuff);

    public delegate void CalculatedBuffDelegate(
        CalculatedBuff calculatedBuff);
}
