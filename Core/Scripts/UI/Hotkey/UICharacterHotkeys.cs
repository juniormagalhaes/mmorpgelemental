﻿using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICharacterHotkeys : UIBase
    {
        public List<string> filterCategories = new List<string>();
        public bool doNotIncludeSkills;
        public List<SkillType> filterSkillTypes = new List<SkillType>() { SkillType.Active };
        public bool doNotIncludeItems;
        public List<ItemType> filterItemTypes = new List<ItemType>() { ItemType.Armor, ItemType.Shield, ItemType.Weapon, ItemType.Potion, ItemType.Building, ItemType.Pet, ItemType.Mount, ItemType.Skill };
        public bool doNotIncludeGuildSkills;
        public UICharacterHotkeyAssigner uiCharacterHotkeyAssigner;
        public UICharacterHotkeyPair[] uiCharacterHotkeys;
        public UICharacterSkill uiCharacterSkillPrefab;
        public UICharacterItem uiCharacterItemPrefab;
        public UIGuildSkill uiGuildSkillPrefab;

        [Header("Mobile Touch Controls")]
        [FormerlySerializedAs("hotkeyMovementJoyStick")]
        [FormerlySerializedAs("hotkeyAimJoyStick")]
        public MobileMovementJoystick hotkeyAimJoyStickPrefab;
        public RectTransform hotkeyCancelArea;

        [Header("Console Controls")]
        public string consoleConfirmButtonName = "Activate";

        public static UICharacterHotkey UsingHotkey { get; private set; }
        public static AimPosition HotkeyAimPosition { get; private set; }

        internal static UICharacterHotkey s_hotkeyForDialogControlling;
        public static UICharacterHotkey HotkeyForDialogControlling
        {
            get
            {
                if (GameInstance.UseMobileInput())
                    return HotkeyJoystickForDialogControlling.UICharacterHotkey;
                if (s_hotkeyForDialogControlling == null)
                    s_hotkeyForDialogControlling = new GameObject("_HotkeyForDialogControlling").AddComponent<UICharacterHotkey>();
                return s_hotkeyForDialogControlling;
            }
        }

        internal static IHotkeyJoystickEventHandler s_hotkeyJoystickForDialogControlling;
        public static IHotkeyJoystickEventHandler HotkeyJoystickForDialogControlling
        {
            get
            {
                if (s_hotkeyJoystickForDialogControlling == null && s_hotkeyJoysticks.Count > 0)
                {
                    IHotkeyJoystickEventHandler prefab = s_hotkeyJoysticks[0];
                    s_hotkeyJoystickForDialogControlling = Instantiate(prefab.gameObject, prefab.transform.parent).GetComponent<IHotkeyJoystickEventHandler>();
                    s_hotkeyJoystickForDialogControlling.gameObject.name = "_HotkeyJoystickForDialogControlling";
                    s_hotkeyJoystickForDialogControlling.transform.localPosition = prefab.transform.localPosition;
                    s_hotkeyJoystickForDialogControlling.transform.localRotation = prefab.transform.localRotation;
                    s_hotkeyJoystickForDialogControlling.transform.localScale = prefab.transform.localScale;
                    s_hotkeyJoystickForDialogControlling.UICharacterHotkey.Setup(prefab.UICharacterHotkey.UICharacterHotkeys, null, new CharacterHotkey(), -1);
                }
                return s_hotkeyJoystickForDialogControlling;
            }
        }
        private static readonly List<IHotkeyJoystickEventHandler> s_hotkeyJoysticks = new List<IHotkeyJoystickEventHandler>();

        private Dictionary<string, List<UICharacterHotkey>> _cacheUICharacterHotkeys;
        public Dictionary<string, List<UICharacterHotkey>> CacheUICharacterHotkeys
        {
            get
            {
                InitCaches();
                return _cacheUICharacterHotkeys;
            }
        }

        private UICharacterHotkeySelectionManager _cacheSelectionManager;
        public UICharacterHotkeySelectionManager CacheSelectionManager
        {
            get
            {
                if (_cacheSelectionManager == null)
                    _cacheSelectionManager = gameObject.GetOrAddComponent<UICharacterHotkeySelectionManager>();
                _cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return _cacheSelectionManager;
            }
        }

        private void InitCaches()
        {
            if (_cacheUICharacterHotkeys == null)
            {
                CacheSelectionManager.DeselectSelectedUI();
                CacheSelectionManager.Clear();
                int j = 0;
                _cacheUICharacterHotkeys = new Dictionary<string, List<UICharacterHotkey>>();
                for (int i = 0; i < uiCharacterHotkeys.Length; ++i)
                {
                    UICharacterHotkeyPair uiCharacterHotkey = uiCharacterHotkeys[i];
                    string id = uiCharacterHotkey.hotkeyId;
                    UICharacterHotkey ui = uiCharacterHotkey.ui;
                    if (!string.IsNullOrEmpty(id) && ui != null)
                    {
                        CharacterHotkey characterHotkey = new CharacterHotkey();
                        characterHotkey.hotkeyId = id;
                        characterHotkey.type = HotkeyType.None;
                        characterHotkey.relateId = string.Empty;
                        ui.Setup(this, uiCharacterHotkeyAssigner, characterHotkey, -1);
                        if (!_cacheUICharacterHotkeys.ContainsKey(id))
                            _cacheUICharacterHotkeys.Add(id, new List<UICharacterHotkey>());
                        _cacheUICharacterHotkeys[id].Add(ui);
                        CacheSelectionManager.Add(ui);
                        // Select first UI
                        if (j == 0)
                            ui.SelectByManager();
                        ++j;
                    }
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            // Deactivate this because this variable used to be in-scene object variable
            // but now it is a variable for a prefab.
            if (hotkeyAimJoyStickPrefab != null)
                hotkeyAimJoyStickPrefab.gameObject.SetActive(false);
            if (hotkeyCancelArea != null)
                hotkeyCancelArea.gameObject.SetActive(false);
        }

        protected virtual void OnEnable()
        {
            UpdateData();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onRecached += UpdateData;
            GameInstance.PlayingCharacterEntity.onHotkeysOperation += PlayingCharacterEntity_onHotkeysOperation;
        }

        protected virtual void OnDisable()
        {
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onRecached -= UpdateData;
            GameInstance.PlayingCharacterEntity.onHotkeysOperation -= PlayingCharacterEntity_onHotkeysOperation;
        }

        private void PlayingCharacterEntity_onHotkeysOperation(LiteNetLibSyncList.Operation arg1, int arg2)
        {
            UpdateData();
        }

        private void Update()
        {
            if (GameInstance.UseMobileInput())
                UpdateHotkeyMobileInputs();
            else if (GameInstance.UseConsoleInput())
                UpdateHotkeyConsoleInputs();
            else
                UpdateHotkeyInputs();
        }

        public override void Hide()
        {
            CacheSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        public virtual void UpdateData()
        {
            InitCaches();
            IList<CharacterHotkey> characterHotkeys = GameInstance.PlayingCharacterEntity.Hotkeys;
            for (int i = 0; i < characterHotkeys.Count; ++i)
            {
                CharacterHotkey characterHotkey = characterHotkeys[i];
                List<UICharacterHotkey> uis;
                if (!string.IsNullOrEmpty(characterHotkey.hotkeyId) && CacheUICharacterHotkeys.TryGetValue(characterHotkey.hotkeyId, out uis))
                {
                    foreach (UICharacterHotkey ui in uis)
                    {
                        ui.Setup(this, uiCharacterHotkeyAssigner, characterHotkey, i);
                        ui.Show();
                    }
                }
            }
        }

        #region Mobile Controls
        public static void SetUsingHotkey(UICharacterHotkey hotkey)
        {
            if (IsAnyHotkeyJoyStickDragging())
                return;
            // Cancel old using hotkey
            if (UsingHotkey != null)
            {
                UsingHotkey.FinishAimControls(true, HotkeyAimPosition);
                UsingHotkey = null;
                HotkeyAimPosition = default;
            }
            UsingHotkey = hotkey;
            if (UsingHotkey != null && UsingHotkey.IsChanneledAbility())
                UsingHotkey.StartChanneledAbility();
        }

        /// <summary>
        /// Update hotkey input for PC devices
        /// </summary>
        private void UpdateHotkeyInputs()
        {
            if (UsingHotkey == null)
                return;
            HotkeyAimPosition = UsingHotkey.UpdateAimControls(Vector2.zero);
            // Click anywhere (on the map) to use skill
            if (InputManager.GetMouseButtonDown(0) && !UsingHotkey.IsChanneledAbility() && !UIBlockController.IsBlockController())
                FinishHotkeyAimControls(false);
        }

        /// <summary>
        /// Update hotkey input for Mobile devices
        /// </summary>
        private void UpdateHotkeyMobileInputs()
        {
            bool isAnyHotkeyJoyStickDragging = false;
            for (int i = 0; i < s_hotkeyJoysticks.Count; ++i)
            {
                if (s_hotkeyJoysticks[i] == null)
                    continue;
                s_hotkeyJoysticks[i].UpdateEvent();
                if (UsingHotkey == s_hotkeyJoysticks[i].UICharacterHotkey)
                    HotkeyAimPosition = s_hotkeyJoysticks[i].AimPosition;
                if (s_hotkeyJoysticks[i].IsDragging)
                    isAnyHotkeyJoyStickDragging = true;
            }

            if (hotkeyCancelArea != null)
                hotkeyCancelArea.gameObject.SetActive(isAnyHotkeyJoyStickDragging);
        }

        /// <summary>
        /// Update hotkey input for Console devices
        /// </summary>
        private void UpdateHotkeyConsoleInputs()
        {
            if (UsingHotkey == null)
                return;
            HotkeyAimPosition = UsingHotkey.UpdateAimControls(Vector2.zero);
            // Click anywhere (on the map) to use skill
            if (InputManager.GetButtonDown(consoleConfirmButtonName) && !UsingHotkey.IsChanneledAbility() && !UIBlockController.IsBlockController())
                FinishHotkeyAimControls(false);
        }

        public static void FinishHotkeyAimControls(bool isCancel)
        {
            if (UsingHotkey == null)
                return;
            UsingHotkey.FinishAimControls(isCancel, HotkeyAimPosition);
            if (HotkeyJoystickForDialogControlling != null && HotkeyJoystickForDialogControlling.UICharacterHotkey == UsingHotkey)
                HotkeyJoystickForDialogControlling.gameObject.SetActive(false);
            UsingHotkey = null;
            HotkeyAimPosition = default;
        }

        public static void RegisterHotkeyJoystick(IHotkeyJoystickEventHandler hotkeyJoystick)
        {
            if (!s_hotkeyJoysticks.Contains(hotkeyJoystick))
                s_hotkeyJoysticks.Add(hotkeyJoystick);
        }

        public static bool IsAnyHotkeyJoyStickDragging()
        {
            foreach (IHotkeyJoystickEventHandler hotkeyJoystick in s_hotkeyJoysticks)
            {
                if (hotkeyJoystick == null)
                    continue;
                if (hotkeyJoystick.IsDragging)
                    return true;
            }
            return false;
        }
        #endregion

        public static void SetupHotkeyForDialogControlling(HotkeyType type, string relateId)
        {
            CharacterHotkey hotkey = new CharacterHotkey();
            hotkey.type = type;
            hotkey.relateId = relateId;
            if (HotkeyJoystickForDialogControlling != null)
                HotkeyJoystickForDialogControlling.gameObject.SetActive(true);
            HotkeyForDialogControlling.Data = hotkey;
            HotkeyForDialogControlling.OnClickUse();
        }
    }
}
