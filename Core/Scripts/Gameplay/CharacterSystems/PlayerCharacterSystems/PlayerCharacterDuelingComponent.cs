using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class PlayerCharacterDuelingComponent : BaseNetworkedGameEntityComponent<BasePlayerCharacterEntity>
    {
        [SerializeField]
        private SyncFieldByte duelingState = new SyncFieldByte();

        protected float _duelingStartTime = -1f;
        public float DuelingStartTime
        {
            get
            {
                return _duelingStartTime;
            }
            set
            {
                _duelingStartTime = value;
                if (IsServer && _duelingStartTime <= 0f)
                    duelingState.Value = (byte)DuelingState.None;
            }
        }
        public bool DuelingStarting
        {
            get
            {
                return (DuelingState)duelingState.Value == DuelingState.Starting;
            }
        }
        public bool DuelingStarted
        {
            get
            {
                return (DuelingState)duelingState.Value == DuelingState.Started;
            }
        }
        public bool DuelingTimeout
        {
            get
            {
                return (DuelingState)duelingState.Value == DuelingState.Timeout;
            }
        }
        public bool DuelingStartingOrStarted
        {
            get
            {
                return DuelingStarting || DuelingStarted;
            }
        }

        public uint DuelingCharacterObjectId { get; private set; }
        private BasePlayerCharacterEntity _duelingCharacter;
        public BasePlayerCharacterEntity DuelingCharacter
        {
            get
            {
                if (!DuelingStarted && Time.unscaledTime - DuelingRequestTime >= CurrentGameInstance.duelingRequestDuration)
                {
                    _duelingCharacter = null;
                    DuelingCharacterObjectId = 0;
                }
                return _duelingCharacter;
            }
            set
            {
                _duelingCharacter = value;
                if (_duelingCharacter == null)
                    DuelingCharacterObjectId = 0;
                else
                    DuelingCharacterObjectId = _duelingCharacter.ObjectId;
                DuelingRequestTime = Time.unscaledTime;
            }
        }


        /// <summary>
        /// Action: BasePlayerCharacterEntity anotherCharacter
        /// </summary>
        public event System.Action<BasePlayerCharacterEntity> onRequestDueling;
        /// <summary>
        /// Action: BasePlayerCharacterEntity anotherCharacter
        /// </summary>
        public event System.Action<BasePlayerCharacterEntity, float, float> onStartDueling;
        /// <summary>
        /// Action: BasePlayerCharacterEntity loserCharacter
        /// </summary>
        public event System.Action<BasePlayerCharacterEntity> onEndDueling;

        public float DuelingRequestTime { get; private set; }

        public bool DisableDueling
        {
            get
            {
                return Entity.IsDead() || CurrentGameInstance.disableDueling || BaseGameNetworkManager.CurrentMapInfo.DisableDueling;
            }
        }

        protected float _countDownDuration;
        protected float _duelDuration;

        public override void EntityUpdate()
        {
            if (IsServer && DuelingStartTime > 0)
            {
                float time = Time.unscaledTime;
                if (time - DuelingStartTime > _countDownDuration + _duelDuration)
                {
                    duelingState.Value = (byte)DuelingState.Timeout;
                }
                else if (time - DuelingStartTime > _countDownDuration)
                {
                    duelingState.Value = (byte)DuelingState.Started;
                }
                else
                {
                    duelingState.Value = (byte)DuelingState.Starting;
                }
            }
            if (IsOwnerClient)
            {
                if (DuelingStartingOrStarted && !DuelingTimeout && DuelingCharacterObjectId > 0 && _duelingCharacter == null && Manager.TryGetEntityByObjectId(DuelingCharacterObjectId, out _duelingCharacter))
                {
                    _duelingCharacter.Dueling.DuelingCharacter = Entity;
                    _duelingCharacter.Dueling.DuelingStartTime = DuelingStartTime;
                    _duelingCharacter.Dueling._countDownDuration = _countDownDuration;
                    _duelingCharacter.Dueling._duelDuration = _duelDuration;
                }
            }
        }

        public void ClearDuelingData()
        {
            DuelingStartTime = -1f;
            CancelInvoke(nameof(UpdateDueling));
        }

        public void StopDueling()
        {
            if (DuelingCharacter == null)
            {
                ClearDuelingData();
                return;
            }
            // Set dueling state/data for co player character entity
            DuelingCharacter.Dueling.ClearDuelingData();
            DuelingCharacter.Dueling.DuelingCharacter = null;
            // Set dueling state/data for player character entity
            ClearDuelingData();
            DuelingCharacter = null;
        }

        public bool CallCmdSendDuelingRequest(uint objectId)
        {
            if (DisableDueling)
            {
                ClientGenericActions.ClientReceiveGameMessage(UITextKeys.UI_ERROR_FEATURE_IS_DISABLED);
                return false;
            }
            RPC(CmdSendDuelingRequest, objectId);
            return true;
        }

        [ServerRpc]
        protected void CmdSendDuelingRequest(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (DisableDueling)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_FEATURE_IS_DISABLED);
                return;
            }
            if (!Manager.TryGetEntityByObjectId(objectId, out BasePlayerCharacterEntity targetCharacterEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_NOT_FOUND);
                return;
            }
            if (targetCharacterEntity.Dueling.DuelingCharacter != null)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_DUELING);
                return;
            }
            if (!Entity.IsGameEntityInDistance(targetCharacterEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }
            if (targetCharacterEntity.IsInSafeArea)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_IN_SAFE_AREA);
                return;
            }
            DuelingCharacter = targetCharacterEntity;
            targetCharacterEntity.Dueling.DuelingCharacter = Entity;
            // Send receive dueling request to player
            DuelingCharacter.Dueling.CallOwnerReceiveDuelingRequest(ObjectId);
#endif
        }

        public bool CallOwnerReceiveDuelingRequest(uint objectId)
        {
            RPC(TargetReceiveDuelingRequest, ConnectionId, objectId);
            return true;
        }

        [TargetRpc]
        protected void TargetReceiveDuelingRequest(uint objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onRequestDueling != null)
                onRequestDueling.Invoke(playerCharacterEntity);
        }

        public bool CallCmdAcceptDuelingRequest()
        {
            RPC(CmdAcceptDuelingRequest);
            return true;
        }

        [ServerRpc]
        protected void CmdAcceptDuelingRequest()
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (DuelingCharacter == null)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CANNOT_ACCEPT_DUELING_REQUEST);
                StopDueling();
                return;
            }
            if (!Entity.IsGameEntityInDistance(DuelingCharacter))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                StopDueling();
                return;
            }
            if (Entity.IsInSafeArea)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_IN_SAFE_AREA);
                StopDueling();
                return;
            }
            float countDownDuration = CurrentGameInstance.duelingCountDownDuration;
            float duelDuration = CurrentGameInstance.duelingDuration;
            // Set dueling state/data for co player character entity
            DuelingCharacter.Dueling.ClearDuelingData();
            DuelingCharacter.Dueling.StartDueling(countDownDuration, duelDuration);
            DuelingCharacter.Dueling.CallOwnerAcceptedDuelingRequest(ObjectId, countDownDuration, duelDuration);
            // Set dueling state/data for player character entity
            ClearDuelingData();
            StartDueling(countDownDuration, duelDuration);
            CallOwnerAcceptedDuelingRequest(DuelingCharacter.ObjectId, countDownDuration, duelDuration);
#endif
        }

        public bool CallCmdDeclineDuelingRequest()
        {
            RPC(CmdDeclineDuelingRequest);
            return true;
        }

        [ServerRpc]
        protected void CmdDeclineDuelingRequest()
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (DuelingCharacter != null)
                GameInstance.ServerGameMessageHandlers.SendGameMessage(DuelingCharacter.ConnectionId, UITextKeys.UI_ERROR_DUELING_REQUEST_DECLINED);
            GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_DUELING_REQUEST_DECLINED);
            StopDueling();
#endif
        }

        public bool CallOwnerAcceptedDuelingRequest(uint objectId, float countDownDuration, float duelDuration)
        {
            RPC(TargetAcceptedDuelingRequest, ConnectionId, objectId, countDownDuration, duelDuration);
            return true;
        }

        [TargetRpc]
        protected void TargetAcceptedDuelingRequest(uint objectId, float countDownDuration, float duelDuration)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (!IsServer)
            {
                // Already setup in accept request function, so don't setup again
                DuelingCharacter = playerCharacterEntity;
                DuelingCharacter.Dueling.DuelingCharacter = Entity;
                DuelingCharacter.Dueling.ClearDuelingData();
                DuelingCharacter.Dueling.StartDueling(countDownDuration, duelDuration);
                ClearDuelingData();
                StartDueling(countDownDuration, duelDuration);
            }
            if (onStartDueling != null)
                onStartDueling.Invoke(playerCharacterEntity, countDownDuration, duelDuration);
        }

        public bool CallOwnerEndDueling(uint loserObjectId)
        {
            if (!DuelingStarted)
                return false;
            RPC(TargetEndDueling, ConnectionId, loserObjectId);
            return true;
        }

        [TargetRpc]
        protected void TargetEndDueling(uint loserObjectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(loserObjectId, out playerCharacterEntity))
                playerCharacterEntity = null;
            StopDueling();
            if (onEndDueling != null)
                onEndDueling.Invoke(playerCharacterEntity);
        }

        protected void StartDueling(float countDownDuration, float duelDuration)
        {
            DuelingStartTime = Time.unscaledTime;
            _countDownDuration = countDownDuration;
            _duelDuration = duelDuration;
            if (IsServer)
            {
                CancelInvoke(nameof(UpdateDueling));
                InvokeRepeating(nameof(UpdateDueling), 0f, 1f);
            }
        }

        protected void UpdateDueling()
        {
            if (DuelingTimeout)
            {
                EndDueling(null);
                return;
            }

            if (Entity.IsInSafeArea)
            {
                EndDueling(Entity);
                return;
            }
        }

        public void EndDueling(BasePlayerCharacterEntity loser)
        {
            uint loserObjectId = loser != null ? loser.ObjectId : 0;
            if (DuelingCharacter != null)
                DuelingCharacter.Dueling.CallOwnerEndDueling(loserObjectId);
            CallOwnerEndDueling(loserObjectId);
            StopDueling();
        }

        public override void EntityOnDestroy()
        {
            // Player disconnect?
            if (IsServer && DuelingStarted)
                EndDueling(Entity);
        }
    }
}