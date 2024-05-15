﻿using UnityEngine;
using XNode;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract partial class BaseNpcDialog : Node, IGameData
    {
        [Input]
        public BaseNpcDialog input;
        [Header("NPC Dialog Configs")]
        [Tooltip("Default title")]
        public string title;
        [Tooltip("Titles by language keys")]
        public LanguageData[] titles = new LanguageData[0];
        [Tooltip("Default description")]
        [TextArea]
        public string description;
        [Tooltip("Descriptions by language keys")]
        public LanguageData[] descriptions = new LanguageData[0];
        public Sprite icon;
        public AudioClip voice;
        [HideInInspector]
        public BaseNpcDialogAction enterDialogActionOnClient;
        public List<BaseNpcDialogAction> enterDialogActionsOnClient = new List<BaseNpcDialogAction>();
        [HideInInspector]
        public BaseNpcDialogAction enterDialogActionOnServer;
        public List<BaseNpcDialogAction> enterDialogActionsOnServer = new List<BaseNpcDialogAction>();

        #region Generic Data
        public string Id { get { return name; } }

        public int DataId { get { return MakeDataId(Id); } }

        public string Title
        {
            get { return Language.GetText(titles, title); }
        }

        public string Description
        {
            get { return Language.GetText(descriptions, description); }
        }

        public Sprite Icon
        {
            get { return icon; }
        }

        public AudioClip Voice
        {
            get { return voice; }
        }

        public List<BaseNpcDialogAction> EnterDialogActionsOnClient
        {
            get { return enterDialogActionsOnClient; }
        }

        public List<BaseNpcDialogAction> EnterDialogActionsOnServer
        {
            get { return enterDialogActionsOnServer; }
        }

        public static int MakeDataId(string id)
        {
            return id.GenerateHashId();
        }
        #endregion

#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (Validate())
                EditorUtility.SetDirty(this);
        }
#endif

        public virtual bool Validate()
        {
            bool hasChanges = false;
            if (enterDialogActionOnClient != null)
            {
                enterDialogActionsOnClient.Add(enterDialogActionOnClient);
                enterDialogActionOnClient = null;
                hasChanges = true;
            }
            if (enterDialogActionOnServer != null)
            {
                enterDialogActionsOnServer.Add(enterDialogActionOnServer);
                enterDialogActionOnServer = null;
                hasChanges = true;
            }
            return hasChanges;
        }

        public virtual void PrepareRelatesData()
        {

        }

        public override object GetValue(NodePort port)
        {
            return port.node;
        }

        public override void OnCreateConnection(NodePort from, NodePort to, bool swapping)
        {
            if (swapping)
                return;
            SetDialogByPort(from, to);
        }

        public override void OnRemoveConnection(NodePort port, bool swapping)
        {
            if (swapping)
                return;
            SetDialogByPort(port, null);
        }

        public virtual UniTask<bool> IsPassMenuCondition(IPlayerCharacterData character)
        {
            return UniTask.FromResult(true);
        }

        public static BaseNpcDialog GetValidatedDialogOrNull(BaseNpcDialog dialog, BasePlayerCharacterEntity characterEntity)
        {
            if (dialog == null || !dialog.ValidateDialog(characterEntity))
                return null;
            return dialog;
        }

        /// <summary>
        /// This will be called to render current dialog
        /// </summary>
        /// <param name="uiNpcDialog"></param>
        public abstract UniTask RenderUI(UINpcDialog uiNpcDialog);
        /// <summary>
        /// This will be called to un-render previous dialog
        /// </summary>
        /// <param name="uiNpcDialog"></param>
        public abstract void UnrenderUI(UINpcDialog uiNpcDialog);
        /// <summary>
        /// This will be called to validate dialog to determine that it will show to player or not
        /// </summary>
        /// <param name="characterEntity"></param>
        /// <returns></returns>
        public abstract bool ValidateDialog(BasePlayerCharacterEntity characterEntity);
        /// <summary>
        /// Get next dialog by selected menu index
        /// </summary>
        /// <param name="characterEntity"></param>
        /// <param name="menuIndex"></param>
        /// <returns></returns>
        public abstract UniTask GoToNextDialog(BasePlayerCharacterEntity characterEntity, byte menuIndex);
        protected abstract void SetDialogByPort(NodePort from, NodePort to);
        public abstract bool IsShop { get; }

        public async UniTask<bool> IsPassEnterDialogActionConditionsOnClient(IPlayerCharacterData player)
        {
            List<UniTask<bool>> tasks = new List<UniTask<bool>>();
            foreach (BaseNpcDialogAction action in EnterDialogActionsOnClient)
            {
                tasks.Add(action.IsPass(player));
            }
            bool[] isPasses = await UniTask.WhenAll(tasks);
            foreach (bool isPass in isPasses)
            {
                if (!isPass)
                    return false;
            }
            return true;
        }

        public async UniTask<bool> IsPassEnterDialogActionConditionsOnServer(IPlayerCharacterData player)
        {
            List<UniTask<bool>> tasks = new List<UniTask<bool>>();
            foreach (BaseNpcDialogAction action in EnterDialogActionsOnServer)
            {
                tasks.Add(action.IsPass(player));
            }
            bool[] isPasses = await UniTask.WhenAll(tasks);
            foreach (bool isPass in isPasses)
            {
                if (!isPass)
                    return false;
            }
            return true;
        }

        public async UniTask DoEnterDialogActionsOnClient(IPlayerCharacterData player)
        {
            List<UniTask> tasks = new List<UniTask>();
            foreach (BaseNpcDialogAction action in EnterDialogActionsOnClient)
            {
                tasks.Add(action.DoAction(player));
            }
            await UniTask.WhenAll(tasks);
        }

        public async UniTask DoEnterDialogActionsOnServer(IPlayerCharacterData player)
        {
            List<UniTask> tasks = new List<UniTask>();
            foreach (BaseNpcDialogAction action in EnterDialogActionsOnServer)
            {
                tasks.Add(action.DoAction(player));
            }
            await UniTask.WhenAll(tasks);
        }
    }
}
