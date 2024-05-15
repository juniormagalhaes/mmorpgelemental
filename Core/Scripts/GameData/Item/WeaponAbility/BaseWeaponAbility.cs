﻿using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseWeaponAbility : BaseGameData
    {
        protected BasePlayerCharacterController controller;
        protected CharacterItem weapon;
        public virtual bool ShouldDeactivateOnDead { get { return true; } }
        public virtual bool ShouldDeactivateOnReload { get { return true; } }

        public virtual void Setup(BasePlayerCharacterController controller, CharacterItem weapon)
        {
            this.controller = controller;
            this.weapon = weapon;
        }

        public virtual void Desetup() { }
        public virtual void ForceDeactivated() { }
        public abstract void OnPreActivate();
        public abstract WeaponAbilityState UpdateActivation(WeaponAbilityState state, float deltaTime);
        public abstract void OnPreDeactivate();
    }
}
