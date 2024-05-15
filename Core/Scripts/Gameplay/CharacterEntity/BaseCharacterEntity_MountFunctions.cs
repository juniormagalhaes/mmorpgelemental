﻿using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        protected float _lastMountTime;

        public virtual void Mount(VehicleEntity mountEntityPrefab)
        {
            if (!IsServer || mountEntityPrefab == null || Time.unscaledTime - _lastMountTime < CurrentGameInstance.mountDelay)
                return;

            _lastMountTime = Time.unscaledTime;

            Vector3 enterPosition = EntityTransform.position;
            if (PassengingVehicleEntity != null)
            {
                enterPosition = PassengingVehicleEntity.Entity.EntityTransform.position;
                ExitVehicle();
            }

            // Instantiate new mount entity
            LiteNetLibIdentity spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                mountEntityPrefab.Identity.HashAssetId, enterPosition,
                Quaternion.Euler(0, EntityTransform.eulerAngles.y, 0));
            VehicleEntity vehicle = spawnObj.GetComponent<VehicleEntity>();
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj, 0, ConnectionId);

            // Seat index for mount entity always 0
            EnterVehicle(vehicle, 0);
        }

        public override void SetPassengingVehicle(byte seatIndex, IVehicleEntity vehicleEntity)
        {
            base.SetPassengingVehicle(seatIndex, vehicleEntity);
            _isRecaching = true;
        }

        public override bool CanEnterVehicle(IVehicleEntity vehicleEntity, byte seatIndex, out UITextKeys gameMessage)
        {
            if (!base.CanEnterVehicle(vehicleEntity, seatIndex, out gameMessage))
                return false;

            if (!IsGameEntityInDistance(vehicleEntity))
            {
                gameMessage = UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR;
                return false;
            }

            return true;
        }
    }
}