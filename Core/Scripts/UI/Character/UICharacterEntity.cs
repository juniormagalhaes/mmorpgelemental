﻿using Cysharp.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UICharacterEntity : UIDamageableEntity<BaseCharacterEntity>
    {
        [Header("Character Entity - String Formats")]
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);
        [Tooltip("Format => {0} = {Count Down Duration}")]
        public UILocaleKeySetting formatKeySkillCastDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("Character Entity - UI Elements")]
        public TextWrapper uiTextLevel;
        // Mp
        public UIGageValue uiGageMp;
        // Skill cast
        public GameObject uiSkillCastContainer;
        public TextWrapper uiTextSkillCast;
        public Image imageSkillCastGage;
        public Slider sliderSkillCastGage;
        public UICharacterBuffs uiCharacterBuffs;

        protected float _castingSkillCountDown;
        protected float _castingSkillDuration;
        protected BasePlayerCharacterEntity _previousPlayingCharacterEntity;

        protected override void AddEvents(BaseCharacterEntity entity)
        {
            if (entity == null)
                return;
            base.AddEvents(entity);
            entity.onLevelChange += OnLevelChange;
            entity.onCurrentMpChange += OnCurrentMpChange;
            entity.onBuffsOperation += OnBuffsOperation;
            if (entity is BasePlayerCharacterEntity playerEntity)
                playerEntity.onPkPointChange += OnPkPointChange;
            GameInstance.onSetPlayingCharacter += GameInstance_onSetPlayingCharacter;
            GameInstance_onSetPlayingCharacter(GameInstance.PlayingCharacterEntity);
        }

        protected override void RemoveEvents(BaseCharacterEntity entity)
        {
            if (entity == null)
                return;
            base.RemoveEvents(entity);
            entity.onLevelChange -= OnLevelChange;
            entity.onCurrentMpChange -= OnCurrentMpChange;
            entity.onBuffsOperation -= OnBuffsOperation;
            if (entity is BasePlayerCharacterEntity playerEntity)
                playerEntity.onPkPointChange -= OnPkPointChange;
            GameInstance.onSetPlayingCharacter -= GameInstance_onSetPlayingCharacter;
            GameInstance_onSetPlayingCharacter(null);
        }

        private void OnLevelChange(int level)
        {
            UpdateLevel();
        }

        private void OnCurrentMpChange(int mp)
        {
            UpdateMp();
        }

        private void OnBuffsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation op, int index)
        {
            UpdateBuffs();
        }

        private void OnPkPointChange(int pkPoint)
        {
            UpdateTitle();
        }

        private void GameInstance_onSetPlayingCharacter(IPlayerCharacterData playingCharacterData)
        {
            if (_previousPlayingCharacterEntity != null)
            {
                _previousPlayingCharacterEntity.onLevelChange += PlayingCharacterEntity_onLevelChange;
            }
            BasePlayerCharacterEntity playerCharacterEntity = playingCharacterData as BasePlayerCharacterEntity;
            _previousPlayingCharacterEntity = playerCharacterEntity;
            if (_previousPlayingCharacterEntity != null)
            {
                _previousPlayingCharacterEntity.onLevelChange -= PlayingCharacterEntity_onLevelChange;
                UpdateTitle();
            }
        }

        private void PlayingCharacterEntity_onLevelChange(int level)
        {
            UpdateTitle();
        }

        protected override void Update()
        {
            base.Update();

            if (Data == null)
            {
                if (uiSkillCastContainer != null)
                    uiSkillCastContainer.SetActive(false);
                return;
            }

            _castingSkillCountDown = Data.CastingSkillCountDown;
            _castingSkillDuration = Data.CastingSkillDuration;

            if (uiSkillCastContainer != null)
                uiSkillCastContainer.SetActive(_castingSkillCountDown > 0 && _castingSkillDuration > 0);

            if (uiTextSkillCast != null)
                uiTextSkillCast.text = ZString.Format(LanguageManager.GetText(formatKeySkillCastDuration), _castingSkillCountDown.ToString("N2"));

            if (imageSkillCastGage != null)
                imageSkillCastGage.fillAmount = _castingSkillDuration <= 0 ? 0 : 1 - (_castingSkillCountDown / _castingSkillDuration);

            if (sliderSkillCastGage != null)
                sliderSkillCastGage.value = _castingSkillDuration <= 0 ? 0 : 1 - (_castingSkillCountDown / _castingSkillDuration);
        }

        protected override void UpdateData()
        {
            base.UpdateData();
            UpdateLevel();
            UpdateMp();
            UpdateBuffs();
        }

        private void UpdateLevel()
        {
            if (uiTextLevel == null)
                return;
            uiTextLevel.text = ZString.Format(LanguageManager.GetText(formatKeyLevel), Data == null ? "1" : Data.Level.ToString("N0"));
        }

        private void UpdateMp()
        {
            if (uiGageMp == null)
                return;
            int currentMp = 0;
            int maxMp = 0;
            if (Data != null)
            {
                currentMp = Data.CurrentMp;
                maxMp = Data.MaxMp;
            }
            uiGageMp.Update(currentMp, maxMp);
        }

        private void UpdateBuffs()
        {
            if (uiCharacterBuffs == null)
                return;
            uiCharacterBuffs.UpdateData(Data);
        }
    }
}
