﻿using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterActionComponentManager))]
    public class DefaultCharacterReloadComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterReloadComponent
    {
        public const float DEFAULT_TOTAL_DURATION = 2f;
        public const float DEFAULT_TRIGGER_DURATION = 1f;
        public const float DEFAULT_STATE_SETUP_DELAY = 1f;

        protected readonly List<CancellationTokenSource> _reloadCancellationTokenSources = new List<CancellationTokenSource>();
        public int ReloadingAmmoDataId { get; protected set; }
        public int ReloadingAmmoAmount { get; protected set; }
        public bool IsReloading { get; protected set; }
        public float LastReloadEndTime { get; protected set; }
        protected bool _skipMovementValidation;
        public bool IsSkipMovementValidationWhileReloading { get { return _skipMovementValidation; } set { _skipMovementValidation = value; } }
        protected bool _shouldUseRootMotion;
        public bool IsUseRootMotionWhileReloading { get { return _shouldUseRootMotion; } protected set { _shouldUseRootMotion = value; } }
        public float MoveSpeedRateWhileReloading { get; protected set; }
        public MovementRestriction MovementRestrictionWhileReloading { get; protected set; }
        protected float _totalDuration;
        public float ReloadTotalDuration { get { return _totalDuration; } set { _totalDuration = value; } }
        protected float[] _triggerDurations;
        public float[] ReloadTriggerDurations { get { return _triggerDurations; } set { _triggerDurations = value; } }
        public AnimActionType AnimActionType { get; protected set; }

        protected CharacterActionComponentManager _manager;

        public override void EntityStart()
        {
            _manager = GetComponent<CharacterActionComponentManager>();
        }

        protected virtual void SetReloadActionStates(AnimActionType animActionType, int reloadingAmmoDataId, int reloadingAmmoAmount)
        {
            ClearReloadStates();
            AnimActionType = animActionType;
            ReloadingAmmoDataId = reloadingAmmoDataId;
            ReloadingAmmoAmount = reloadingAmmoAmount;
            IsReloading = true;
        }

        public virtual void ClearReloadStates()
        {
            ReloadingAmmoAmount = 0;
            IsReloading = false;
        }

        protected virtual async UniTaskVoid ReloadRoutine(bool isLeftHand, int reloadingAmmoDataId, int reloadingAmmoAmount)
        {
            // Prepare cancellation
            CancellationTokenSource reloadCancellationTokenSource = new CancellationTokenSource();
            _reloadCancellationTokenSources.Add(reloadCancellationTokenSource);

            // Prepare requires data and get weapon data
            Entity.GetReloadingData(
                ref isLeftHand,
                out AnimActionType animActionType,
                out int animActionDataId,
                out CharacterItem weapon);

            // Prepare requires data and get animation data
            Entity.GetAnimationData(
                animActionType,
                animActionDataId,
                0,
                out float animSpeedRate,
                out _triggerDurations,
                out _totalDuration);

            // Set doing action state at clients and server
            SetReloadActionStates(animActionType, reloadingAmmoDataId, reloadingAmmoAmount);

            // Prepare requires data and get damages data
            IWeaponItem weaponItem = weapon.GetWeaponItem();
            if (weaponItem.ReloadDuration > 0)
                _totalDuration = weaponItem.ReloadDuration;

            // Calculate move speed rate while doing action at clients and server
            MoveSpeedRateWhileReloading = Entity.GetMoveSpeedRateWhileReloading(weaponItem);
            MovementRestrictionWhileReloading = Entity.GetMovementRestrictionWhileReloading(weaponItem);

            // Last attack end time
            float remainsDuration = DEFAULT_TOTAL_DURATION;
            LastReloadEndTime = Time.unscaledTime + DEFAULT_TOTAL_DURATION;
            if (_totalDuration >= 0f)
            {
                remainsDuration = _totalDuration;
                LastReloadEndTime = Time.unscaledTime + (_totalDuration / animSpeedRate);
            }

            try
            {
                bool tpsModelAvailable = Entity.CharacterModel != null && Entity.CharacterModel.gameObject.activeSelf;
                BaseCharacterModel vehicleModel = Entity.PassengingVehicleModel as BaseCharacterModel;
                bool vehicleModelAvailable = vehicleModel != null;
                bool fpsModelAvailable = IsClient && Entity.FpsModel != null && Entity.FpsModel.gameObject.activeSelf;

                // Play animation
                if (tpsModelAvailable)
                    Entity.CharacterModel.PlayActionAnimation(AnimActionType, animActionDataId, 0, out _skipMovementValidation, out _shouldUseRootMotion);
                if (vehicleModelAvailable)
                    vehicleModel.PlayActionAnimation(AnimActionType, animActionDataId, 0, out _skipMovementValidation, out _shouldUseRootMotion);
                if (fpsModelAvailable)
                    Entity.FpsModel.PlayActionAnimation(AnimActionType, animActionDataId, 0, out _, out _);

                // Special effects will plays on clients only
                if (IsClient)
                {
                    // Play weapon reload special effects
                    if (tpsModelAvailable)
                        Entity.CharacterModel.PlayEquippedWeaponReload(isLeftHand);
                    if (fpsModelAvailable)
                        Entity.FpsModel.PlayEquippedWeaponReload(isLeftHand);
                    // Play reload sfx
                    AudioClipWithVolumeSettings audioClip = weaponItem.ReloadClip;
                    if (audioClip != null)
                        AudioManager.PlaySfxClipAtAudioSource(audioClip.audioClip, Entity.CharacterModel.GenericAudioSource, audioClip.GetRandomedVolume());
                }

                // Try setup state data (maybe by animation clip events or state machine behaviours), if it was not set up
                if (_triggerDurations == null || _triggerDurations.Length == 0 || _totalDuration < 0f)
                {
                    // Wait some components to setup proper `attackTriggerDurations` and `attackTotalDuration` within `DEFAULT_STATE_SETUP_DELAY`
                    float setupDelayCountDown = DEFAULT_STATE_SETUP_DELAY;
                    do
                    {
                        await UniTask.Yield(reloadCancellationTokenSource.Token);
                        setupDelayCountDown -= Time.unscaledDeltaTime;
                    } while (setupDelayCountDown > 0 && (_triggerDurations == null || _triggerDurations.Length == 0 || _totalDuration < 0f));
                    if (setupDelayCountDown <= 0f)
                    {
                        // Can't setup properly, so try to setup manually to make it still workable
                        remainsDuration = DEFAULT_TOTAL_DURATION - DEFAULT_STATE_SETUP_DELAY;
                        _triggerDurations = new float[1]
                        {
                            DEFAULT_TRIGGER_DURATION,
                        };
                    }
                    else
                    {
                        // Can setup, so set proper `remainsDuration` and `LastAttackEndTime` value
                        remainsDuration = _totalDuration;
                        LastReloadEndTime = Time.unscaledTime + (_totalDuration / animSpeedRate);
                    }
                }

                bool reloaded = false;
                float tempTriggerDuration;
                for (int i = 0; i < _triggerDurations.Length; ++i)
                {
                    // Wait until triggger before reload ammo
                    tempTriggerDuration = _triggerDurations[i];
                    remainsDuration -= tempTriggerDuration;
                    await UniTask.Delay((int)(tempTriggerDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.FixedUpdate, reloadCancellationTokenSource.Token);

                    // Special effects will plays on clients only
                    if (IsClient)
                    {
                        // Play weapon reload special effects
                        if (tpsModelAvailable)
                            Entity.CharacterModel.PlayEquippedWeaponReloaded(isLeftHand);
                        if (fpsModelAvailable)
                            Entity.FpsModel.PlayEquippedWeaponReloaded(isLeftHand);

                        // Play reload sfx
                        AudioClipWithVolumeSettings audioClip = weaponItem.ReloadedClip;
                        if (audioClip != null)
                            AudioManager.PlaySfxClipAtAudioSource(audioClip.audioClip, Entity.CharacterModel.GenericAudioSource, audioClip.GetRandomedVolume());
                    }

                    await UniTask.Yield(reloadCancellationTokenSource.Token);

                    // Reload / Fill ammo
                    if (!reloaded)
                    {
                        reloaded = true;
                        EquipWeapons equipWeapons = Entity.EquipWeapons;
                        if (IsServer)
                        {
                            if (Entity.DecreaseItems(reloadingAmmoDataId, reloadingAmmoAmount))
                            {
                                if (weapon.ammo > 0 && weapon.ammoDataId != reloadingAmmoDataId)
                                {
                                    Entity.IncreaseItems(CharacterItem.Create(reloadingAmmoDataId, 1, weapon.ammo));
                                    weapon.ammo = 0;
                                }
                                Entity.FillEmptySlots();
                                weapon.ammoDataId = reloadingAmmoDataId;
                                weapon.ammo += reloadingAmmoAmount;
                                if (isLeftHand)
                                    equipWeapons.leftHand = weapon;
                                else
                                    equipWeapons.rightHand = weapon;
                                Entity.EquipWeapons = equipWeapons;
                            }
                        }
                    }

                    if (remainsDuration <= 0f)
                    {
                        // Stop trigger animations loop
                        break;
                    }
                }

                if (remainsDuration > 0f)
                {
                    // Wait until animation ends to stop actions
                    await UniTask.Delay((int)(remainsDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.FixedUpdate, reloadCancellationTokenSource.Token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Catch the cancellation
                LastReloadEndTime = Time.unscaledTime;
            }
            catch (System.Exception ex)
            {
                // Other errors
                Logging.LogException(LogTag, ex);
            }
            finally
            {
                reloadCancellationTokenSource.Dispose();
                _reloadCancellationTokenSources.Remove(reloadCancellationTokenSource);
            }
            // Clear action states at clients and server
            ClearReloadStates();
        }

        public virtual void CancelReload()
        {
            for (int i = _reloadCancellationTokenSources.Count - 1; i >= 0; --i)
            {
                if (!_reloadCancellationTokenSources[i].IsCancellationRequested)
                    _reloadCancellationTokenSources[i].Cancel();
                _reloadCancellationTokenSources.RemoveAt(i);
            }
        }

        public virtual void Reload(bool isLeftHand)
        {
            if (!IsServer && IsOwnerClient)
            {
                RPC(CmdReload, isLeftHand);
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                // Reload immediately at server
                ProceedCmdReload(isLeftHand);
            }
        }

        [ServerRpc]
        protected void CmdReload(bool isLeftHand)
        {
            ProceedCmdReload(isLeftHand);
        }

        protected void ProceedCmdReload(bool isLeftHand)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!_manager.IsAcceptNewAction())
                return;
            // Speed hack avoidance
            if (Time.unscaledTime - LastReloadEndTime < -0.2f)
                return;
            // Get weapon to reload
            CharacterItem reloadingWeapon = isLeftHand ? Entity.EquipWeapons.leftHand : Entity.EquipWeapons.rightHand;
            if (reloadingWeapon.IsEmptySlot())
                return;
            IWeaponItem reloadingWeaponItem = reloadingWeapon.GetWeaponItem();
            if (reloadingWeaponItem == null || reloadingWeaponItem.AmmoCapacity <= 0 || reloadingWeapon.ammo >= reloadingWeaponItem.AmmoCapacity)
                return;
            bool hasAmmoType = reloadingWeaponItem.WeaponType.AmmoType != null;
            bool hasAmmoItems = reloadingWeaponItem.AmmoItems != null && reloadingWeaponItem.AmmoItems.Length > 0;
            if (!hasAmmoType && !hasAmmoItems)
                return;
            // Prepare reload data
            int reloadingAmmoDataId = 0;
            int inventoryAmount = 0;
            if (hasAmmoType)
            {
                inventoryAmount = Entity.CountAmmos(reloadingWeaponItem.WeaponType.AmmoType, out reloadingAmmoDataId);
            }
            else if (hasAmmoItems)
            {
                for (int indexOfAmmoItem = 0; indexOfAmmoItem < reloadingWeaponItem.AmmoItems.Length; ++indexOfAmmoItem)
                {
                    int tempAmmoDataId = reloadingWeaponItem.AmmoItems[indexOfAmmoItem].DataId;
                    int tempAmmoAmount = Entity.CountNonEquipItems(tempAmmoDataId);
                    if (tempAmmoAmount > 0)
                    {
                        reloadingAmmoDataId = tempAmmoDataId;
                        inventoryAmount = tempAmmoAmount;
                        break;
                    }
                }
            }
            int reloadingAmmoAmount = 0;
            int ammoCapacity = reloadingWeaponItem.AmmoCapacity;
            if (GameInstance.Items.TryGetValue(reloadingAmmoDataId, out BaseItem tempItem) && tempItem is IAmmoItem tempAmmoItem && tempAmmoItem.OverrideAmmoCapacity > 0)
                ammoCapacity = tempAmmoItem.OverrideAmmoCapacity;
            if (reloadingWeapon.ammoDataId != 0 && reloadingWeapon.ammoDataId == reloadingAmmoDataId)
                reloadingAmmoAmount = ammoCapacity - reloadingWeapon.ammo;
            else
                reloadingAmmoAmount = ammoCapacity;
            if (inventoryAmount < reloadingAmmoAmount)
                reloadingAmmoAmount = inventoryAmount;
            if (reloadingAmmoAmount <= 0)
                return;
            _manager.ActionAccepted();
            ReloadRoutine(isLeftHand, reloadingAmmoDataId, reloadingAmmoAmount).Forget();
            RPC(RpcReload, isLeftHand, reloadingAmmoDataId, reloadingAmmoAmount);
#endif
        }

        [AllRpc]
        protected void RpcReload(bool isLeftHand, int reloadingAmmoDataId, int reloadingAmmoAmount)
        {
            if (IsServer || IsOwnerClient)
            {
                // Don't play reloading animation again
                return;
            }
            ReloadRoutine(isLeftHand, reloadingAmmoDataId, reloadingAmmoAmount).Forget();
        }
    }
}
