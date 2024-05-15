using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial interface IHitRegistrationManager
    {
        /// <summary>
        /// Get hit validate data by attacker and simulate seed
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="simulateSeed"></param>
        /// <returns></returns>
        HitValidateData GetHitValidateData(BaseGameEntity attacker, int simulateSeed);
        /// <summary>
        /// This will be called to store hit reg validation data
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="randomSeed"></param>
        /// <param name="triggerDurations"></param>
        /// <param name="fireSpread"></param>
        /// <param name="damageInfo"></param>
        /// <param name="damageAmounts"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="weapon"></param>
        /// <param name="skill"></param>
        /// <param name="skillLevel"></param>
        void PrepareHitRegValidation(BaseGameEntity attacker, int randomSeed, float[] triggerDurations, byte fireSpread, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> damageAmounts, bool isLeftHand, CharacterItem weapon, BaseSkill skill, int skillLevel);
        /// <summary>
        /// This will be called to confirm hit reg validation data
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="randomSeed"></param>
        /// <param name="triggerIndex"></param>
        /// <param name="increaseDamageAmounts"></param>
        /// <returns></returns>
        void ConfirmHitRegValidation(BaseGameEntity attacker, int randomSeed, byte triggerIndex, Dictionary<DamageElement, MinMaxFloat> increaseDamageAmounts);
        /// <summary>
        /// This will be called at server to perform hit reg validation
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="hitData"></param>
        /// <returns></returns>
        bool PerformValidation(BaseGameEntity attacker, HitRegisterData hitData);
        /// <summary>
        /// Clear all data
        /// </summary>
        void ClearData();
    }
}
