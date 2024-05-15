﻿using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [DefaultExecutionOrder(DefaultExecutionOrders.UI_CRAFTING_QUEUE_ITEMS)]
    public class UICraftingQueueItems : UIBase
    {
        [Header("UI Elements")]
        public GameObject listEmptyObject;
        public UICraftingQueueItem uiDialog;
        public UICraftingQueueItem uiPrefab;
        public Transform uiContainer;
        public UIItemCraftFormulas uiFormulas;
        public bool selectFirstEntryByDefault;

        public ICraftingQueueSource Source { get; private set; }
        public SyncListCraftingQueueItem CraftingQueueItems { get; private set; }

        private UIList _cacheList;
        public UIList CacheList
        {
            get
            {
                if (_cacheList == null)
                {
                    _cacheList = gameObject.AddComponent<UIList>();
                    _cacheList.uiPrefab = uiPrefab.gameObject;
                    _cacheList.uiContainer = uiContainer;
                }
                return _cacheList;
            }
        }

        private UICraftingQueueItemSelectionManager _cacheSelectionManager;
        public UICraftingQueueItemSelectionManager CacheSelectionManager
        {
            get
            {
                if (_cacheSelectionManager == null)
                    _cacheSelectionManager = gameObject.GetOrAddComponent<UICraftingQueueItemSelectionManager>();
                _cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return _cacheSelectionManager;
            }
        }

        public void Show(ICraftingQueueSource source)
        {
            if (IsVisible())
                UnregisterSourceEvents();
            Source = source;
            if (IsVisible())
            {
                if (uiFormulas != null)
                {
                    uiFormulas.CraftingQueueManager = this;
                    uiFormulas.UpdateData();
                }
                RegisterSourceEvents();
                UpdateData();
            }
            Show();
        }

        public void ShowPlayerCraftingQueue()
        {
            if (IsVisible())
                UnregisterSourceEvents();
            Source = null;
            if (IsVisible())
            {
                if (uiFormulas != null)
                {
                    uiFormulas.CraftingQueueManager = this;
                    uiFormulas.UpdateData();
                }
                RegisterSourceEvents();
                UpdateData();
            }
            Show();
        }

        public void RegisterSourceEvents()
        {
            if (Source == null && GameInstance.PlayingCharacterEntity != null)
                Source = GameInstance.PlayingCharacterEntity.Crafting;
            if (Source != null)
            {
                if (Source.PublicQueue)
                    CraftingQueueItems = Source.QueueItems;
                else
                    CraftingQueueItems = GameInstance.PlayingCharacterEntity.Crafting.QueueItems;
                CraftingQueueItems.onOperation += OnCraftingQueueItemsOperation;
            }
        }

        public void UnregisterSourceEvents()
        {
            if (CraftingQueueItems != null)
                CraftingQueueItems.onOperation -= OnCraftingQueueItemsOperation;
        }

        protected virtual void OnEnable()
        {
            CacheSelectionManager.eventOnSelected.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelected.AddListener(OnSelect);
            CacheSelectionManager.eventOnDeselected.RemoveListener(OnDeselect);
            CacheSelectionManager.eventOnDeselected.AddListener(OnDeselect);
            if (uiDialog != null)
            {
                uiDialog.onHide.AddListener(OnDialogHide);
                uiDialog.CraftingQueueManager = this;
            }
            if (uiFormulas != null)
            {
                uiFormulas.CraftingQueueManager = this;
                uiFormulas.Show();
            }
            RegisterSourceEvents();
            UpdateData();
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnDialogHide);
            CacheSelectionManager.DeselectSelectedUI();
            UnregisterSourceEvents();
            Source = null;
        }

        protected virtual void OnDialogHide()
        {
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnSelect(UICraftingQueueItem ui)
        {
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheSelectionManager;
                uiDialog.Data = ui.Data;
                uiDialog.Show();
            }
        }

        protected virtual void OnDeselect(UICraftingQueueItem ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
            }
        }

        protected void OnCraftingQueueItemsOperation(LiteNetLibSyncList.Operation op, int itemIndex)
        {
            UpdateData();
        }

        public virtual void UpdateData()
        {
            int selectedDataId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.dataId : 0;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            UICraftingQueueItem tempUI;
            CacheList.Generate(CraftingQueueItems, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UICraftingQueueItem>();
                tempUI.CraftingQueueManager = this;
                tempUI.Setup(data, GameInstance.PlayingCharacterEntity, index);
                tempUI.Show();
                CacheSelectionManager.Add(tempUI);
                if ((selectFirstEntryByDefault && index == 0) || selectedDataId == data.dataId)
                    tempUI.SelectByManager();
            });
            if (listEmptyObject != null)
                listEmptyObject.SetActive(CraftingQueueItems.Count == 0);
        }
    }
}
