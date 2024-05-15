using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class GoldDropEntity : BaseRewardDropEntity, IPickupActivatableEntity
    {
        public static GoldDropEntity Drop(BaseGameEntity dropper, float multiplier, RewardGivenType givenType, int giverLevel, int sourceLevel, int amount, IEnumerable<string> looters)
        {
            return Drop(GameInstance.Singleton.goldDropEntityPrefab, dropper, multiplier, givenType, giverLevel, sourceLevel, amount, looters, GameInstance.Singleton.itemAppearDuration) as GoldDropEntity;
        }

        protected override bool ProceedPickingUpAtServer_Implementation(BaseCharacterEntity characterEntity, out UITextKeys message)
        {
            BaseCharacterEntity rewardingCharacter = characterEntity;
            if (characterEntity is BaseMonsterCharacterEntity monsterCharacterEntity && monsterCharacterEntity.Summoner is BasePlayerCharacterEntity summonerCharacterEntity)
                rewardingCharacter = summonerCharacterEntity;
            CurrentGameplayRule.RewardGold(rewardingCharacter, Amount, Multiplier, GivenType, GiverLevel, SourceLevel, out int rewardedGold);
            GameInstance.ServerGameMessageHandlers.NotifyRewardGold(rewardingCharacter.ConnectionId, GivenType, rewardedGold);
            message = UITextKeys.NONE;
            return true;
        }
    }
}
