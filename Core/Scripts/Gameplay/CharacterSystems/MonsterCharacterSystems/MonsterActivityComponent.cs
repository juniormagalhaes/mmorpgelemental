﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class MonsterActivityComponent : BaseMonsterActivityComponent
    {
        [SerializeField]
        protected float turnSmoothSpeed = 10f;
        [Tooltip("Min random delay for next wander")]
        public float randomWanderDelayMin = 2f;
        [Tooltip("Max random delay for next wander")]
        public float randomWanderDelayMax = 5f;
        [Tooltip("Random distance around spawn position to wander")]
        public float randomWanderDistance = 2f;
        [Tooltip("Max distance it can move from spawn point, if it's <= 0, it will be determined that it is no limit")]
        public float maxDistanceFromSpawnPoint = 5f;
        [Tooltip("Delay before find enemy again")]
        public float findEnemyDelayMin = 1f;
        [FormerlySerializedAs("findEnemyDelay")]
        public float findEnemyDelayMax = 3f;
        [Tooltip("If following target time reached this value it will stop following target")]
        public float followTargetDuration = 5f;
        [Tooltip("Turn to enemy speed")]
        public float turnToEnemySpeed = 800f;
        [Tooltip("Duration to pausing after received damage")]
        public float miniStunDuration = 0f;
        [Tooltip("If this is TRUE, monster will attacks buildings")]
        public bool isAttackBuilding = false;
        [Tooltip("If this is TRUE, monster will prioritize targetting buildings first")]
        [FormerlySerializedAs("isBuildingPriority")]
        public bool isAttackBuildingFirst = false;
        [Tooltip("If this is TRUE, monster will attacks targets while its summoner still idle")]
        [FormerlySerializedAs("isAggressiveWhileSummonerIdle")]
        public bool aggressiveWhileSummoned = false;
        [Tooltip("Delay before it can switch target again")]
        public float switchTargetDelay = 3;

        protected readonly List<DamageableEntity> _enemies = new List<DamageableEntity>();
        protected float _findEnemyCountDown;
        protected float _randomedWanderCountDown;
        protected float _randomedWanderDelay;
        protected float _followEnemyElasped;
        protected Vector3 _lastPosition;
        protected BaseSkill _queueSkill;
        protected int _queueSkillLevel;
        protected bool _alreadySetActionState;
        protected bool _isLeftHandAttacking;
        protected float _lastSetDestinationTime;
        protected bool _reachedSpawnPoint;
        protected bool _enemyExisted;
        protected float _pauseCountdown;
        protected float _lastSwitchTargetTime;

        public override void EntityAwake()
        {
            base.EntityAwake();
            Entity.onNotifyEnemySpotted += Entity_onNotifyEnemySpotted;
            Entity.onNotifyEnemySpottedByAlly += Entity_onNotifyEnemySpottedByAlly;
            Entity.onReceivedDamage += Entity_onReceivedDamage;
        }

        public override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            Entity.onNotifyEnemySpotted -= Entity_onNotifyEnemySpotted;
            Entity.onNotifyEnemySpottedByAlly -= Entity_onNotifyEnemySpottedByAlly;
            Entity.onReceivedDamage -= Entity_onReceivedDamage;
        }

        private void Entity_onNotifyEnemySpotted(BaseCharacterEntity enemy)
        {
            if (Entity.Characteristic != MonsterCharacteristic.Assist)
                return;
            // Warn that this character received damage to nearby characters
            List<BaseCharacterEntity> foundCharacters = Entity.FindAliveEntities<BaseCharacterEntity>(CharacterDatabase.VisualRange, true, false, false, GameInstance.Singleton.playerLayer.Mask | GameInstance.Singleton.playingLayer.Mask | GameInstance.Singleton.monsterLayer.Mask);
            if (foundCharacters == null || foundCharacters.Count == 0) return;
            foreach (BaseCharacterEntity foundCharacter in foundCharacters)
            {
                foundCharacter.NotifyEnemySpottedByAlly(Entity, enemy);
            }
        }

        private void Entity_onNotifyEnemySpottedByAlly(BaseCharacterEntity ally, BaseCharacterEntity enemy)
        {
            if ((Entity.Summoner != null && Entity.Summoner == ally) ||
                Entity.Characteristic == MonsterCharacteristic.Assist)
                Entity.SetAttackTarget(enemy);
        }

        private void Entity_onReceivedDamage(HitBoxPosition position, Vector3 fromPosition, IGameEntity attacker, CombatAmountType combatAmountType, int totalDamage, CharacterItem weapon, BaseSkill skill, int skillLevel, CharacterBuff buff, bool isDamageOverTime)
        {
            BaseCharacterEntity attackerCharacter = attacker as BaseCharacterEntity;
            if (attackerCharacter == null)
                return;
            // If character is not dead, try to attack
            if (!Entity.IsDead())
            {
                if (Entity.GetTargetEntity() == null)
                {
                    // If no target enemy, set target enemy as attacker
                    Entity.SetAttackTarget(attackerCharacter);
                }
                else if (attackerCharacter != Entity.GetTargetEntity() && Random.value > 0.5f && Time.unscaledTime - _lastSwitchTargetTime > switchTargetDelay)
                {
                    // Random 50% to change target when receive damage from anyone
                    _lastSwitchTargetTime = Time.unscaledTime;
                    Entity.SetAttackTarget(attackerCharacter);
                }
                _pauseCountdown = miniStunDuration;
            }
        }

        public override void EntityUpdate()
        {
            if (!Entity.IsServer || Entity.Identity.CountSubscribers() == 0 || CharacterDatabase == null)
                return;

            if (Entity.IsDead())
            {
                Entity.StopMove();
                Entity.SetTargetEntity(null);
                return;
            }

            float deltaTime = Time.unscaledDeltaTime;
            if (_pauseCountdown > 0f)
            {
                _pauseCountdown -= deltaTime;
                if (_pauseCountdown <= 0f)
                    _pauseCountdown = 0f;
                Entity.StopMove();
                return;
            }

            Profiler.BeginSample("MonsterActivityComponent - Update");
            Entity.SetSmoothTurnSpeed(turnSmoothSpeed);

            Vector3 currentPosition = Entity.MovementTransform.position;
            if (Entity.Summoner != null)
            {
                if (!UpdateAttackEnemy(deltaTime, currentPosition))
                {
                    UpdateEnemyFindingActivity(deltaTime);

                    if (Vector3.Distance(currentPosition, Entity.Summoner.EntityTransform.position) > CurrentGameInstance.minFollowSummonerDistance)
                        FollowSummoner();
                    else
                        UpdateWanderDestinationRandomingActivity(deltaTime);
                }
            }
            else
            {
                float distFromSpawnPoint = Vector3.Distance(Entity.SpawnPosition, currentPosition);
                if (!_reachedSpawnPoint)
                {
                    if (distFromSpawnPoint <= Mathf.Max(1f, randomWanderDistance))
                    {
                        _reachedSpawnPoint = true;
                        _followEnemyElasped = 0f;
                        Entity.SetTargetEntity(null);
                        ClearActionState();
                    }
                    return;
                }

                if (Entity.IsInSafeArea)
                {
                    UpdateMoveBackToSpawnPointActivity(deltaTime);
                    return;
                }

                if (maxDistanceFromSpawnPoint > 0f && distFromSpawnPoint >= maxDistanceFromSpawnPoint)
                {
                    UpdateMoveBackToSpawnPointActivity(deltaTime);
                    return;
                }

                if (followTargetDuration > 0f && _followEnemyElasped >= followTargetDuration)
                {
                    UpdateMoveBackToSpawnPointActivity(deltaTime);
                    return;
                }

                if (!UpdateAttackEnemy(deltaTime, currentPosition))
                {
                    // No enemy, try find it
                    _enemyExisted = false;
                    UpdateEnemyFindingActivity(deltaTime);
                    // Random movement (if no enemy existed)
                    UpdateWanderDestinationRandomingActivity(deltaTime);
                }
            }
            Profiler.EndSample();
        }

        protected virtual void UpdateEnemyFindingActivity(float deltaTime)
        {
            _findEnemyCountDown -= deltaTime;
            if (_enemies.Count <= 0 && _findEnemyCountDown > 0f)
                return;
            _findEnemyCountDown = Random.Range(findEnemyDelayMin, findEnemyDelayMin);
            if (!FindEnemy())
                return;
            _enemyExisted = true;
        }

        protected virtual void UpdateMoveBackToSpawnPointActivity(float deltaTime)
        {
            _randomedWanderCountDown -= deltaTime;
            if (_randomedWanderCountDown > 0f)
                return;
            _randomedWanderCountDown = _randomedWanderDelay;
            if (!RandomWanderDestination())
                return;
            _reachedSpawnPoint = false;
        }

        protected virtual void UpdateWanderDestinationRandomingActivity(float deltaTime)
        {
            if (_enemyExisted)
                return;
            _randomedWanderCountDown -= deltaTime;
            if (_randomedWanderCountDown > 0f)
                return;
            _randomedWanderCountDown = _randomedWanderDelay;
            if (!RandomWanderDestination())
                return;
        }

        /// <summary>
        /// Return `TRUE` if following / attacking enemy
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        private bool UpdateAttackEnemy(float deltaTime, Vector3 currentPosition)
        {
            if (Entity.Characteristic == MonsterCharacteristic.NoHarm || !Entity.TryGetTargetEntity(out IDamageableEntity targetEnemy))
            {
                // No target, stop attacking
                ClearActionState();
                return false;
            }

            if (targetEnemy.GetObjectId() == Entity.ObjectId || targetEnemy.IsDeadOrHideFrom(Entity) || !targetEnemy.CanReceiveDamageFrom(Entity.GetInfo()))
            {
                // If target is dead or in safe area stop attacking
                Entity.SetTargetEntity(null);
                ClearActionState();
                return false;
            }

            // If it has target then go to target
            if (targetEnemy != null && !Entity.IsPlayingActionAnimation() && !_alreadySetActionState)
            {
                // Random action state to do next time
                if (CharacterDatabase.RandomSkill(Entity, out _queueSkill, out _queueSkillLevel) && _queueSkill != null)
                {
                    // Cooling down
                    if (Entity.IndexOfSkillUsage(SkillUsageType.Skill, _queueSkill.DataId) >= 0)
                    {
                        _queueSkill = null;
                        _queueSkillLevel = 0;
                    }
                }
                _isLeftHandAttacking = !_isLeftHandAttacking;
                _alreadySetActionState = true;
                return true;
            }

            Vector3 targetPosition = targetEnemy.GetTransform().position;
            float attackDistance = GetAttackDistance();
            if (OverlappedEntity(targetEnemy.Entity, GetDamageTransform().position, targetPosition, attackDistance))
            {
                // Reset follow time, because it is not following
                _followEnemyElasped = 0f;
                // Stop movement
                SetWanderDestination(CacheTransform.position);
                // Lookat target then do something when it's in range
                Vector3 lookAtDirection = (targetPosition - currentPosition).normalized;
                bool turnedToEnemy = false;
                if (lookAtDirection.sqrMagnitude > 0)
                {
                    if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                    {
                        Quaternion currentLookAtRotation = Entity.GetLookRotation();
                        Vector3 lookRotationEuler = Quaternion.LookRotation(lookAtDirection).eulerAngles;
                        lookRotationEuler.x = 0;
                        lookRotationEuler.z = 0;
                        currentLookAtRotation = Quaternion.RotateTowards(currentLookAtRotation, Quaternion.Euler(lookRotationEuler), turnToEnemySpeed * Time.deltaTime);
                        Entity.SetLookRotation(currentLookAtRotation);
                        turnedToEnemy = Mathf.Abs(currentLookAtRotation.eulerAngles.y - lookRotationEuler.y) < 15f;
                    }
                    else
                    {
                        // Update 2D direction
                        Entity.SetLookRotation(Quaternion.LookRotation(lookAtDirection));
                        turnedToEnemy = true;
                    }
                }

                if (!turnedToEnemy)
                    return true;

                Entity.AimPosition = Entity.GetAttackAimPosition(ref _isLeftHandAttacking);
                if (Entity.IsPlayingActionAnimation())
                    return true;

                if (_queueSkill != null && Entity.IndexOfSkillUsage(SkillUsageType.Skill, _queueSkill.DataId) < 0)
                {
                    // Use skill when there is queue skill or randomed skill that can be used
                    Entity.UseSkill(_queueSkill.DataId, false, 0, new AimPosition()
                    {
                        type = AimPositionType.Position,
                        position = _queueSkill.GetDefaultAttackAimPosition(Entity, _queueSkillLevel, _isLeftHandAttacking, targetEnemy),
                    });
                }
                else
                {
                    // Attack when no queue skill
                    bool isLeftHand = false;
                    Entity.Attack(ref isLeftHand);
                }

                ClearActionState();
            }
            else
            {
                // Counting follow time
                _followEnemyElasped += deltaTime;
                // Follow the enemy
                SetDestination(targetPosition, attackDistance);
            }
            return true;
        }

        public void SetDestination(Vector3 destination, float distance)
        {
            float time = Time.unscaledTime;
            if (time - _lastSetDestinationTime <= 0.1f)
                return;
            _lastSetDestinationTime = time;
            Vector3 direction = (destination - Entity.MovementTransform.position).normalized;
            Vector3 position = destination - (direction * (distance - Entity.StoppingDistance));
            Entity.SetExtraMovementState(ExtraMovementState.None);
            Entity.PointClickMovement(position);
        }

        public bool SetWanderDestination(Vector3 destination)
        {
            float time = Time.unscaledTime;
            if (time - _lastSetDestinationTime <= 0.1f)
                return false;
            _lastSetDestinationTime = time;
            Entity.SetExtraMovementState(ExtraMovementState.IsWalking);
            Entity.PointClickMovement(destination);
            return true;
        }

        public virtual bool RandomWanderDestination()
        {
            if (!Entity.CanMove() || Entity.IsPlayingActionAnimation())
                return false;
            _randomedWanderDelay = Random.Range(randomWanderDelayMin, randomWanderDelayMax);
            Vector3 randomPosition;
            // Random position around summoner or around spawn point
            if (Entity.Summoner != null)
            {
                // Random position around summoner
                randomPosition = CurrentGameInstance.GameplayRule.GetSummonPosition(Entity.Summoner);
            }
            else
            {
                // Random position around spawn point
                Vector2 randomCircle = Random.insideUnitCircle * randomWanderDistance;
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                    randomPosition = Entity.SpawnPosition + new Vector3(randomCircle.x, randomCircle.y);
                else
                    randomPosition = Entity.SpawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
            }

            if (!SetWanderDestination(randomPosition))
                return false;

            Entity.SetTargetEntity(null);
            return true;
        }

        public virtual void FollowSummoner()
        {
            Vector3 randomPosition;
            // Random position around summoner or around spawn point
            if (Entity.Summoner != null)
            {
                // Random position around summoner
                randomPosition = GameInstance.Singleton.GameplayRule.GetSummonPosition(Entity.Summoner);
            }
            else
            {
                // Random position around spawn point
                Vector2 randomCircle = Random.insideUnitCircle * randomWanderDistance;
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                    randomPosition = Entity.SpawnPosition + new Vector3(randomCircle.x, randomCircle.y);
                else
                    randomPosition = Entity.SpawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
            }

            Entity.SetTargetEntity(null);
            SetDestination(randomPosition, 0f);
        }

        /// <summary>
        /// Return `TRUE` if found enemy
        /// </summary>
        /// <returns></returns>
        public virtual bool FindEnemy()
        {
            // No harm, don't find enemy
            if (Entity.Characteristic == MonsterCharacteristic.NoHarm)
                return false;

            // Aggressive monster or summoned monster will find target to attack
            bool isAggressive = Entity.Characteristic == MonsterCharacteristic.Aggressive;
            if (!isAggressive && Entity.Summoner == null)
                return false;

            if (!Entity.TryGetTargetEntity(out IDamageableEntity targetEntity) ||
                targetEntity.GetObjectId() == Entity.ObjectId || targetEntity.IsDead() ||
                !targetEntity.CanReceiveDamageFrom(Entity.GetInfo()))
            {
                bool isSummonedAndSummonerExisted = Entity.IsSummonedAndSummonerExisted;
                DamageableEntity enemy;
                // Find one enemy from previously found list
                if (FindOneEnemyFromList(isSummonedAndSummonerExisted, out enemy))
                {
                    Entity.SetAttackTarget(enemy);
                    return true;
                }

                // If no target enemy or target enemy is dead, Find nearby character by layer mask
                _enemies.Clear();
                int overlapMask = CurrentGameInstance.playerLayer.Mask | CurrentGameInstance.monsterLayer.Mask;
                if (isAttackBuilding)
                    overlapMask |= CurrentGameInstance.buildingLayer.Mask;
                if (isSummonedAndSummonerExisted)
                {
                    isAggressive = isAggressive || aggressiveWhileSummoned;
                    // Find enemy around summoner
                    _enemies.AddRange(Entity.FindAliveEntities<DamageableEntity>(
                        Entity.Summoner.EntityTransform.position,
                        CharacterDatabase.SummonedVisualRange,
                        false, /* Don't find an allies */
                        isAggressive,  /* Find an enemies */
                        isAggressive,  /* Find an neutral */
                        overlapMask));
                }
                else
                {
                    _enemies.AddRange(Entity.FindAliveEntities<DamageableEntity>(
                        CharacterDatabase.VisualRange,
                        false, /* Don't find an allies */
                        true,  /* Find an enemies */
                        false, /* Don't find an neutral */
                        overlapMask));
                }
                // Find one enemy from a found list
                if (FindOneEnemyFromList(isSummonedAndSummonerExisted, out enemy))
                {
                    Entity.SetAttackTarget(enemy);
                    return true;
                }
            }

            return false;
        }

        protected virtual bool FindOneEnemyFromList(bool isSummonedAndSummonerExisted, out DamageableEntity enemy)
        {
            enemy = null;
            DamageableEntity tempEntity;
            BuildingEntity tempBuildingEntity;
            for (int i = _enemies.Count - 1; i >= 0; --i)
            {
                tempEntity = _enemies[i];
                _enemies.RemoveAt(i);
                if (tempEntity.GetObjectId() == Entity.ObjectId || tempEntity.IsDead() || !tempEntity.CanReceiveDamageFrom(Entity.GetInfo()))
                {
                    // If enemy is null or cannot receive damage from monster, skip it
                    continue;
                }
                tempBuildingEntity = tempEntity as BuildingEntity;
                if (isAttackBuilding && isSummonedAndSummonerExisted && tempBuildingEntity != null && Entity.Summoner.Id == tempBuildingEntity.CreatorId)
                {
                    // If building was built by summoner, skip it
                    continue;
                }
                // This entity can be enemy
                enemy = tempEntity;
                if (isAttackBuilding && isAttackBuildingFirst && tempBuildingEntity == null)
                {
                    // Trying to find building, if it is not building then try to find it next time
                    continue;
                }
                // Found target, break the loop
                break;
            }
            return enemy != null;
        }

        protected virtual void ClearActionState()
        {
            _queueSkill = null;
            _isLeftHandAttacking = false;
            _alreadySetActionState = false;
        }

        protected Transform GetDamageTransform()
        {
            return _queueSkill != null ? _queueSkill.GetApplyTransform(Entity, _isLeftHandAttacking) :
                Entity.GetWeaponDamageInfo(null).GetDamageTransform(Entity, _isLeftHandAttacking);
        }

        protected float GetAttackDistance()
        {
            return _queueSkill != null && _queueSkill.IsAttack ? _queueSkill.GetCastDistance(Entity, _queueSkillLevel, _isLeftHandAttacking) :
                Entity.GetAttackDistance(_isLeftHandAttacking);
        }

        protected virtual bool OverlappedEntity<T>(T entity, Vector3 measuringPosition, Vector3 targetPosition, float distance)
            where T : BaseGameEntity
        {
            if (Vector3.Distance(measuringPosition, targetPosition) <= distance)
                return true;
            // Target is far from controlling entity, try overlap the entity
            return Entity.FindPhysicFunctions.IsGameEntityInDistance(entity, measuringPosition, distance, false);
        }
    }
}
