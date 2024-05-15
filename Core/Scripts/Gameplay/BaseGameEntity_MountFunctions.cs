﻿using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseGameEntity
    {

        public byte PassengingVehicleSeatIndex { get; private set; }

        private IVehicleEntity _passengingVehicleEntity;
        public IVehicleEntity PassengingVehicleEntity
        {
            get
            {
                if (_passengingVehicleEntity.IsNull())
                    _passengingVehicleEntity = null;
                return _passengingVehicleEntity;
            }
            private set
            {
                _passengingVehicleEntity = value;
            }
        }

        public VehicleType PassengingVehicleType
        {
            get
            {
                if (!PassengingVehicleEntity.IsNull())
                    return PassengingVehicleEntity.VehicleType;
                return null;
            }
        }

        public VehicleSeat PassengingVehicleSeat
        {
            get
            {
                if (!PassengingVehicleEntity.IsNull())
                    return PassengingVehicleEntity.Seats[PassengingVehicleSeatIndex];
                return VehicleSeat.Empty;
            }
        }

        public GameEntityModel PassengingVehicleModel
        {
            get
            {
                if (!PassengingVehicleEntity.IsNull())
                    return PassengingVehicleEntity.Entity.Model;
                return null;
            }
        }

        protected virtual bool EnterVehicle(IVehicleEntity vehicle, byte seatIndex)
        {
            if (!IsServer || vehicle.IsNull())
                return false;

            if (!vehicle.IsSeatAvailable(seatIndex))
            {
                // TODO: Send error message
                return false;
            }

            if (!vehicle.CanBePassenger(seatIndex, this))
            {
                // TODO: Send error message
                return false;
            }

            // Change object owner to driver
            if (vehicle.IsDriver(seatIndex))
                Manager.Assets.SetObjectOwner(vehicle.Entity.ObjectId, ConnectionId);

            // Set passenger to vehicle
            vehicle.SetPassenger(seatIndex, this);

            return true;
        }

        protected virtual void ExitVehicle()
        {
            if (!IsServer || PassengingVehicleEntity.IsNull())
                return;

            bool isDriver = PassengingVehicleEntity.IsDriver(PassengingVehicleSeatIndex);
            bool isDestroying = PassengingVehicleEntity.IsDestroyWhenExit(PassengingVehicleSeatIndex);

            // Clear object owner from driver
            if (PassengingVehicleEntity.IsDriver(PassengingVehicleSeatIndex))
                Manager.Assets.SetObjectOwner(PassengingVehicleEntity.Entity.ObjectId, -1);

            BaseGameEntity vehicleEntity = PassengingVehicleEntity.Entity;
            if (isDestroying)
            {
                // Remove all entity from vehicle
                PassengingVehicleEntity.RemoveAllPassengers();
                // Destroy vehicle entity
                vehicleEntity.NetworkDestroy();
            }
            else
            {
                // Remove this from vehicle
                PassengingVehicleEntity.RemovePassenger(PassengingVehicleSeatIndex);
                // Stop move if driver exit (if not driver continue move by driver controls)
                if (isDriver)
                    vehicleEntity.StopMove();
            }
        }

        /// <summary>
        /// This function will be called by Vehicle Entity to inform that this entity exited vehicle
        /// </summary>
        public void ExitedVehicle(Vector3 exitPosition, Quaternion exitRotation)
        {
            CallRpcOnExitVehicle();
            Teleport(exitPosition, exitRotation, true);
        }

        public virtual void ClearPassengingVehicle()
        {
            SetPassengingVehicle(0, null);
        }

        public virtual void SetPassengingVehicle(byte seatIndex, IVehicleEntity vehicleEntity)
        {
            PassengingVehicleSeatIndex = seatIndex;
            PassengingVehicleEntity = vehicleEntity;
        }
        public void CallCmdEnterVehicle(uint objectId, byte seatIndex)
        {
            RPC(CmdEnterVehicle, objectId, seatIndex);
        }

        public virtual bool CanEnterVehicle(IVehicleEntity vehicleEntity, byte seatIndex, out UITextKeys gameMessage)
        {
            gameMessage = UITextKeys.NONE;
            if (vehicleEntity.IsNull())
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_VEHICLE_ENTITY;
                return false;
            }
            if (!vehicleEntity.IsSeatAvailable(seatIndex))
            {
                gameMessage = UITextKeys.UI_ERROR_SEAT_NOT_AVAILABLE;
                return false;
            }
            return true;
        }

        [ServerRpc]
        protected void CmdEnterVehicle(uint objectId, byte seatIndex)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!Manager.Assets.TryGetSpawnedObject(objectId, out LiteNetLibIdentity identity))
                return;
            IVehicleEntity vehicleEntity = identity.GetComponent<IVehicleEntity>();
            if (!CanEnterVehicle(vehicleEntity, seatIndex, out UITextKeys error))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, error);
                return;
            }
            EnterVehicle(vehicleEntity, seatIndex);
#endif
        }

        public void CallCmdExitVehicle()
        {
            RPC(CmdExitVehicle);
        }

        [ServerRpc]
        protected void CmdExitVehicle()
        {
#if UNITY_EDITOR || UNITY_SERVER
            ExitVehicle();
#endif
        }

        public void CallRpcOnExitVehicle()
        {
            RPC(RpcOnExitVehicle);
        }

        [AllRpc]
        protected void RpcOnExitVehicle()
        {
            ClearPassengingVehicle();
        }
    }
}
