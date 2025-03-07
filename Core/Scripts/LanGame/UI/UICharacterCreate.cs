﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class UICharacterCreate : UIBase
    {
        [Header("Game Object Elements")]
        public Transform characterModelContainer;

        [Header("UI Elements")]
        public CharacterRaceTogglePair[] raceToggles = new CharacterRaceTogglePair[0];
        public UICharacter uiCharacterPrefab;
        public Transform uiCharacterContainer;
        public UICharacterClass uiCharacterClassPrefab;
        public Transform uiCharacterClassContainer;
        public UIFaction uiFactionPrefab;
        public Transform uiFactionContainer;

        [System.Obsolete("Deprecated, use `uiInputCharacterName` instead.")]
        [HideInInspector]
        public InputField inputCharacterName;
        public InputFieldWrapper uiInputCharacterName;
        public Button buttonCreate;
        public UIBodyPartManager[] uiBodyPartManagers = new UIBodyPartManager[0];
        public UIBlendshapeManager uiBlendshapeManager;

        [Header("Event")]
        public UnityEvent eventOnCreateCharacter = new UnityEvent();
        public CharacterDataEvent eventOnSelectCharacter = new CharacterDataEvent();
        public FactionEvent eventOnSelectFaction = new FactionEvent();
        public CharacterClassEvent eventOnSelectCharacterClass = new CharacterClassEvent();
        public CharacterModelEvent eventOnBeforeUpdateAnimation = new CharacterModelEvent();
        public CharacterModelEvent eventOnAfterUpdateAnimation = new CharacterModelEvent();
        public CharacterModelEvent eventOnShowInstantiatedCharacter = new CharacterModelEvent();

        private Toggle firstRaceToggle;
        private Dictionary<CharacterRace, Toggle> _raceToggles;
        public Dictionary<CharacterRace, Toggle> RaceToggles
        {
            get
            {
                if (_raceToggles == null)
                {
                    _raceToggles = new Dictionary<CharacterRace, Toggle>();
                    foreach (CharacterRaceTogglePair raceToggle in raceToggles)
                    {
                        if (raceToggle.race == null || raceToggle.toggle == null)
                            continue;
                        _raceToggles[raceToggle.race] = raceToggle.toggle;
                        if (firstRaceToggle == null)
                            firstRaceToggle = raceToggle.toggle;
                    }
                }
                return _raceToggles;
            }
        }

        private UIList _characterList;
        public UIList CharacterList
        {
            get
            {
                if (_characterList == null)
                {
                    _characterList = gameObject.AddComponent<UIList>();
                    if (uiCharacterPrefab != null && uiCharacterContainer != null)
                    {
                        _characterList.uiPrefab = uiCharacterPrefab.gameObject;
                        _characterList.uiContainer = uiCharacterContainer;
                    }
                }
                return _characterList;
            }
        }

        private UIList _characterClassList;
        public UIList CharacterClassList
        {
            get
            {
                if (_characterClassList == null)
                {
                    _characterClassList = gameObject.AddComponent<UIList>();
                    if (uiCharacterClassPrefab != null && uiCharacterClassContainer != null)
                    {
                        _characterClassList.uiPrefab = uiCharacterClassPrefab.gameObject;
                        _characterClassList.uiContainer = uiCharacterClassContainer;
                    }
                }
                return _characterClassList;
            }
        }

        private UIList _factionList;
        public UIList FactionList
        {
            get
            {
                if (_factionList == null)
                {
                    _factionList = gameObject.AddComponent<UIList>();
                    if (uiFactionPrefab != null && uiFactionContainer != null)
                    {
                        _factionList.uiPrefab = uiFactionPrefab.gameObject;
                        _factionList.uiContainer = uiFactionContainer;
                    }
                }
                return _factionList;
            }
        }

        private UICharacterSelectionManager _characterSelectionManager;
        public UICharacterSelectionManager CharacterSelectionManager
        {
            get
            {
                if (_characterSelectionManager == null)
                    _characterSelectionManager = gameObject.GetOrAddComponent<UICharacterSelectionManager>();
                _characterSelectionManager.selectionMode = UISelectionMode.Toggle;
                return _characterSelectionManager;
            }
        }

        private UICharacterClassSelectionManager _characterClassSelectionManager;
        public UICharacterClassSelectionManager CharacterClassSelectionManager
        {
            get
            {
                if (_characterClassSelectionManager == null)
                    _characterClassSelectionManager = gameObject.GetOrAddComponent<UICharacterClassSelectionManager>();
                _characterClassSelectionManager.selectionMode = UISelectionMode.Toggle;
                return _characterClassSelectionManager;
            }
        }

        private UIFactionSelectionManager _factionSelectionManager;
        public UIFactionSelectionManager FactionSelectionManager
        {
            get
            {
                if (_factionSelectionManager == null)
                    _factionSelectionManager = GetComponent<UIFactionSelectionManager>();
                if (_factionSelectionManager == null)
                    _factionSelectionManager = gameObject.AddComponent<UIFactionSelectionManager>();
                _factionSelectionManager.selectionMode = UISelectionMode.Toggle;
                return _factionSelectionManager;
            }
        }

        protected readonly Dictionary<int, BaseCharacterModel> _characterModelByEntityId = new Dictionary<int, BaseCharacterModel>();
        protected BaseCharacterModel _selectedModel;
        public BaseCharacterModel SelectedModel { get { return _selectedModel; } }
        protected readonly Dictionary<int, PlayerCharacter[]> _playerCharacterDataByEntityId = new Dictionary<int, PlayerCharacter[]>();
        protected PlayerCharacter[] _selectableCharacterClasses;
        public PlayerCharacter[] SelectableCharacterClasses { get { return _selectableCharacterClasses; } }
        protected PlayerCharacter _selectedPlayerCharacter;
        public PlayerCharacter SelectedPlayerCharacter { get { return _selectedPlayerCharacter; } }
        protected PlayerCharacterData _selectedPlayerCharacterData;
        public PlayerCharacterData SelectedPlayerCharacterData { get { return _selectedPlayerCharacterData; } }
        protected readonly HashSet<CharacterRace> SelectedRaces = new HashSet<CharacterRace>();
        protected Faction _selectedFaction;
        public Faction SelectedFaction { get { return _selectedFaction; } }
        public int SelectedEntityId { get; protected set; }
        public int SelectedDataId { get; protected set; }
        public int SelectedFactionId { get; protected set; }
        public List<CharacterDataBoolean> PublicBools { get; protected set; } = new List<CharacterDataBoolean>();
        public List<CharacterDataInt32> PublicInts { get; protected set; } = new List<CharacterDataInt32>();
        public List<CharacterDataFloat32> PublicFloats { get; protected set; } = new List<CharacterDataFloat32>();

        protected override void Awake()
        {
            base.Awake();
            MigrateInputComponent();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (MigrateInputComponent())
                EditorUtility.SetDirty(this);
        }
#endif

        public bool MigrateInputComponent()
        {
            bool hasChanges = false;
            InputFieldWrapper wrapper;
#pragma warning disable CS0618 // Type or member is obsolete
            if (inputCharacterName != null)
            {
                hasChanges = true;
                wrapper = inputCharacterName.gameObject.GetOrAddComponent<InputFieldWrapper>();
                wrapper.unityInputField = inputCharacterName;
                uiInputCharacterName = wrapper;
                inputCharacterName = null;
            }
#pragma warning restore CS0618 // Type or member is obsolete
            return hasChanges;
        }

        protected virtual List<BasePlayerCharacterEntity> GetCreatableCharacters()
        {
            if (RaceToggles.Count == 0)
                return GameInstance.PlayerCharacterEntities.Values.ToList();
            else
                return GameInstance.PlayerCharacterEntities.Values.Where((o) => SelectedRaces.Contains(o.Race)).ToList();
        }

        protected virtual List<Faction> GetSelectableFactions()
        {
            return GameInstance.Factions.Values.Where(o => !o.IsLocked).ToList();
        }

        protected virtual void LoadCharacters()
        {
            // Remove all models
            characterModelContainer.RemoveChildren();
            _characterModelByEntityId.Clear();
            // Remove all cached data
            _playerCharacterDataByEntityId.Clear();
            // Clear character selection
            CharacterSelectionManager.Clear();
            CharacterList.HideAll();
            // Show list of characters that can be created
            PlayerCharacterData firstData = null;
            CharacterList.Generate(GetCreatableCharacters(), (index, characterEntity, ui) =>
            {
                // Cache player character to dictionary, we will use it later
                _playerCharacterDataByEntityId[characterEntity.EntityId] = characterEntity.CharacterDatabases;
                // Prepare data
                BaseCharacter playerCharacter = characterEntity.CharacterDatabases[0];
                PlayerCharacterData playerCharacterData = new PlayerCharacterData();
                playerCharacterData.SetNewPlayerCharacterData(characterEntity.EntityTitle, playerCharacter.DataId, characterEntity.EntityId, characterEntity.FactionId);
                // Hide all model, the first one will be shown later
                BaseCharacterModel characterModel = playerCharacterData.InstantiateModel(characterModelContainer);
                _characterModelByEntityId[playerCharacterData.EntityId] = characterModel;
                characterModel.gameObject.SetActive(false);
                // Setup UI
                if (ui != null)
                {
                    UICharacter uiCharacter = ui.GetComponent<UICharacter>();
                    uiCharacter.NotForOwningCharacter = true;
                    uiCharacter.Data = playerCharacterData;
                    CharacterSelectionManager.Add(uiCharacter);
                }
                if (index == 0)
                    firstData = playerCharacterData;
            });
            // Select first entry
            if (CharacterSelectionManager.Count > 0)
                CharacterSelectionManager.Select(0);
            else
                OnSelectCharacter(firstData);
        }

        protected virtual void LoadFactions()
        {
            // Clear faction selection
            FactionSelectionManager.Clear();
            FactionList.HideAll();
            // Show list of factions that can be selected
            Faction firstData = null;
            FactionList.Generate(GetSelectableFactions(), (index, faction, ui) =>
            {
                // Setup UI
                if (ui != null)
                {
                    UIFaction uiFaction = ui.GetComponent<UIFaction>();
                    uiFaction.Data = faction;
                    FactionSelectionManager.Add(uiFaction);
                }
                if (index == 0)
                    firstData = faction;
            });
            // Select first entry
            if (FactionSelectionManager.Count > 0)
                FactionSelectionManager.Select(0);
            else
                OnSelectFaction(firstData);
        }

        protected virtual void OnEnable()
        {
            // Setup Events
            buttonCreate.onClick.RemoveListener(OnClickCreate);
            buttonCreate.onClick.AddListener(OnClickCreate);
            CharacterSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacter);
            CharacterSelectionManager.eventOnSelect.AddListener(OnSelectCharacter);
            CharacterClassSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterClass);
            CharacterClassSelectionManager.eventOnSelect.AddListener(OnSelectCharacterClass);
            FactionSelectionManager.eventOnSelect.RemoveListener(OnSelectFaction);
            FactionSelectionManager.eventOnSelect.AddListener(OnSelectFaction);
            SelectedRaces.Clear();
            if (RaceToggles.Count > 0)
            {
                foreach (KeyValuePair<CharacterRace, Toggle> raceToggle in RaceToggles)
                {
                    raceToggle.Value.SetIsOnWithoutNotify(false);
                    raceToggle.Value.onValueChanged.RemoveAllListeners();
                    raceToggle.Value.onValueChanged.AddListener((isOn) =>
                    {
                        OnRaceToggleUpdate(raceToggle.Key, isOn);
                    });
                }
                firstRaceToggle.isOn = true;
            }
            else
            {
                LoadCharacters();
            }
            LoadFactions();
        }

        protected virtual void OnDisable()
        {
            characterModelContainer.RemoveChildren();
            uiInputCharacterName.text = string.Empty;
        }

        protected virtual void Update()
        {
            if (SelectedModel != null)
            {
                eventOnBeforeUpdateAnimation.Invoke(SelectedModel);
                SelectedModel.UpdateAnimation(Time.deltaTime);
                eventOnAfterUpdateAnimation.Invoke(SelectedModel);
            }
        }

        protected void OnSelectCharacter(UICharacter uiCharacter)
        {
            // Set data
            _selectedPlayerCharacterData = uiCharacter.Data as PlayerCharacterData;
            SelectedDataId = _selectedPlayerCharacterData.DataId;
            SelectedEntityId = _selectedPlayerCharacterData.EntityId;
            PublicBools.Clear();
            PublicInts.Clear();
            PublicFloats.Clear();
            // Hide models
            characterModelContainer.SetChildrenActive(false);
            // Show selected character model
            _characterModelByEntityId.TryGetValue(SelectedEntityId, out _selectedModel);
            if (SelectedModel != null)
            {
                SelectedModel.gameObject.SetActive(true);
                eventOnShowInstantiatedCharacter.Invoke(SelectedModel);
                for (int i = 0; i < uiBodyPartManagers.Length; ++i)
                {
                    uiBodyPartManagers[i].onSetModelValue = PublicInts.SetValue;
                    uiBodyPartManagers[i].onSetColorValue = PublicInts.SetValue;
                    uiBodyPartManagers[i].SetCharacterModel(SelectedModel);
                }
                if (uiBlendshapeManager != null)
                {
                    uiBlendshapeManager.onSetBlendshapeValue = PublicFloats.SetValue;
                    uiBlendshapeManager.SetCharacterModel(SelectedModel);
                }
            }
            // Run event
            eventOnSelectCharacter.Invoke(_selectedPlayerCharacterData);
            OnSelectCharacter(_selectedPlayerCharacterData);
            // Clear character class selection
            CharacterClassSelectionManager.Clear();
            CharacterClassList.HideAll();
            // Setup character class list
            PlayerCharacter firstData = null;
            _playerCharacterDataByEntityId.TryGetValue(SelectedEntityId, out _selectableCharacterClasses);
            CharacterClassList.Generate(_selectableCharacterClasses, (index, playerCharacter, ui) =>
            {
                // Setup UI
                if (ui != null)
                {
                    UICharacterClass uiCharacterClass = ui.GetComponent<UICharacterClass>();
                    uiCharacterClass.Data = playerCharacter;
                    CharacterClassSelectionManager.Add(uiCharacterClass);
                }
                if (index == 0)
                    firstData = playerCharacter;
            });
            // Select first entry
            if (CharacterClassSelectionManager.Count > 0)
                CharacterClassSelectionManager.Select(0);
            else
                OnSelectCharacterClass(firstData);
        }

        protected virtual void OnSelectCharacter(IPlayerCharacterData playerCharacterData)
        {
            // NOTE: Override this function to do something as you wish
        }

        protected void OnSelectCharacterClass(UICharacterClass uiCharacterClass)
        {
            // Set data
            _selectedPlayerCharacter = uiCharacterClass.Data as PlayerCharacter;
            if (SelectedPlayerCharacter != null)
            {
                // Set creating player character data
                SelectedDataId = _selectedPlayerCharacter.DataId;
                // Prepare equip items
                List<CharacterItem> equipItems = new List<CharacterItem>();
                foreach (BaseItem armorItem in SelectedPlayerCharacter.ArmorItems)
                {
                    if (armorItem == null)
                        continue;
                    equipItems.Add(CharacterItem.Create(armorItem));
                }
                // Prepare equip weapons
                EquipWeapons equipWeapons = new EquipWeapons();
                if (SelectedPlayerCharacter.RightHandEquipItem != null)
                    equipWeapons.rightHand = CharacterItem.Create(SelectedPlayerCharacter.RightHandEquipItem);
                if (SelectedPlayerCharacter.LeftHandEquipItem != null)
                    equipWeapons.leftHand = CharacterItem.Create(SelectedPlayerCharacter.LeftHandEquipItem);
                // Set model equip weapons
                IList<EquipWeapons> selectableWeaponSets = new List<EquipWeapons>
                {
                    equipWeapons
                };
                // Set model equip items
                SelectedModel.SetEquipItems(equipItems, selectableWeaponSets, 0, false);
            }
            // Run event
            eventOnSelectCharacterClass.Invoke(_selectedPlayerCharacter);
            OnSelectCharacterClass(uiCharacterClass.Data);
        }

        protected virtual void OnSelectCharacterClass(BaseCharacter baseCharacter)
        {
            // NOTE: Override this function to do something as you wish
        }

        protected void OnSelectFaction(UIFaction uiFaction)
        {
            // Set data
            _selectedFaction = uiFaction.Data;
            if (SelectedFaction != null)
            {
                // Set creating player character's faction
                SelectedFactionId = _selectedFaction.DataId;
            }
            // Run event
            eventOnSelectFaction.Invoke(_selectedFaction);
            OnSelectFaction(_selectedFaction);
        }

        protected virtual void OnSelectFaction(Faction faction)
        {
            // NOTE: Override this function to do something as you wish
        }

        protected virtual void OnRaceToggleUpdate(CharacterRace race, bool isOn)
        {
            if (isOn)
            {
                SelectedRaces.Add(race);
                LoadCharacters();
            }
            else
            {
                SelectedRaces.Remove(race);
            }
        }

        protected virtual void OnClickCreate()
        {
            GameInstance gameInstance = GameInstance.Singleton;
            // Validate character name
            string characterName = uiInputCharacterName.text.Trim();
            int minCharacterNameLength = gameInstance.minCharacterNameLength;
            int maxCharacterNameLength = gameInstance.maxCharacterNameLength;
            if (characterName.Length < minCharacterNameLength)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_SHORT.ToString()));
                Debug.LogWarning("Cannot create character, character name is too short");
                return;
            }
            if (characterName.Length > maxCharacterNameLength)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_LONG.ToString()));
                Debug.LogWarning("Cannot create character, character name is too long");
                return;
            }

            SaveCreatingPlayerCharacter(characterName);

            if (eventOnCreateCharacter != null)
                eventOnCreateCharacter.Invoke();
        }

        protected virtual void SaveCreatingPlayerCharacter(string characterName)
        {
            PlayerCharacterData characterData = new PlayerCharacterData();
            characterData.Id = GenericUtils.GetUniqueId();
            characterData.SetNewPlayerCharacterData(characterName, SelectedDataId, SelectedEntityId, SelectedFactionId);
            characterData.PublicBools = PublicBools;
            characterData.PublicInts = PublicInts;
            characterData.PublicFloats = PublicFloats;
            GameInstance.Singleton.SaveSystem.SaveCharacter(characterData);
        }
    }
}
