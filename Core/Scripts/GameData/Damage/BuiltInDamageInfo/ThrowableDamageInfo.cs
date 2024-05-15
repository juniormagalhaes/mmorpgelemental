using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ThrowableDamageInfo : BaseCustomDamageInfo
    {
        public float throwForce;
        public float throwableLifeTime;
        public ThrowableDamageEntity throwableDamageEntity;

        public override void PrepareRelatesData()
        {
            GameInstance.AddPoolingObjects(throwableDamageEntity);
        }

        public override Transform GetDamageTransform(BaseCharacterEntity attacker, bool isLeftHand)
        {
            Transform transform = null;
            if (attacker.ModelManager.IsFps)
            {
                if (attacker.FpsModel && attacker.FpsModel.gameObject.activeSelf)
                {
                    // Spawn bullets from fps model
                    transform = isLeftHand ? attacker.FpsModel.GetLeftHandMissileDamageTransform() : attacker.FpsModel.GetRightHandMissileDamageTransform();
                }
            }
            else
            {
                // Spawn bullets from tps model
                transform = isLeftHand ? attacker.CharacterModel.GetLeftHandMissileDamageTransform() : attacker.CharacterModel.GetRightHandMissileDamageTransform();
            }

            if (transform == null)
            {
                // Still no missile transform, use default missile transform
                transform = attacker.MissileDamageTransform;
            }
            return transform;
        }

        public override float GetDistance()
        {
            // NOTE: It is actually can't find actual distance by simple math because it has many factors,
            // Such as thrown position, distance from ground, gravity. 
            // So all throwable weapons are suited for shooter games only.
            return throwForce * 0.5f;
        }

        public override float GetFov()
        {
            return 10f;
        }

        public override bool IsHitValid(HitValidateData hitValidateData, HitRegisterData hitData, DamageableHitBox hitBox)
        {
            return true;
        }

        public override void LaunchDamageEntity(BaseCharacterEntity attacker, bool isLeftHand, CharacterItem weapon, int simulateSeed, byte triggerIndex, byte spreadIndex, Vector3 fireStagger, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, int skillLevel, AimPosition aimPosition)
        {
            if (throwableDamageEntity == null)
                return;

            // Get generic attack data
            EntityInfo instigator = attacker.GetInfo();
            System.Random random = new System.Random(unchecked(simulateSeed + ((triggerIndex + 1) * (spreadIndex + 1) * 16)));
            Vector3 stagger = new Vector3(GenericUtils.RandomFloat(random.Next(), -fireStagger.x, fireStagger.x), GenericUtils.RandomFloat(random.Next(), -fireStagger.y, fireStagger.y));
            this.GetDamagePositionAndRotation(attacker, isLeftHand, aimPosition, stagger, out Vector3 damagePosition, out Vector3 damageDirection, out Quaternion damageRotation);
            // Prepare hit reg data
            HitRegisterData hitRegData = new HitRegisterData()
            {
                SimulateSeed = simulateSeed,
                TriggerIndex = triggerIndex,
                SpreadIndex = spreadIndex,
                LaunchTimestamp = BaseGameNetworkManager.Singleton.Timestamp,
                Origin = damagePosition,
                Direction = damageDirection,
            };

            // Instantiate throwable damage entity
            // TODO: May predict and move missile ahead of time based on client's RTT
            float throwForce = this.throwForce;
            float throwableLifeTime = this.throwableLifeTime;
            PoolSystem.GetInstance(throwableDamageEntity, damagePosition, damageRotation).Setup(instigator, weapon, simulateSeed, triggerIndex, spreadIndex, damageAmounts, skill, skillLevel, hitRegData, throwForce, throwableLifeTime);
        }
    }
}