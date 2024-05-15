using UnityEngine;

namespace MultiplayerARPG
{
    public class AnimationCharacterModelFactory : ICharacterModelFactory
    {
        public string Name => "Animation Character Model (Obsolete)";
        public DimensionType DimensionType => DimensionType.Dimension3D;

        public AnimationCharacterModelFactory()
        {

        }

        public bool ValidateSourceObject(GameObject obj)
        {
            Animation comp = obj.GetComponentInChildren<Animation>();
            if (comp == null)
            {
                Debug.LogError("Cannot create new entity with `AnimationCharacterModel`, can't find `Animation` component");
                Object.DestroyImmediate(obj);
                return false;
            }
            return true;
        }

        public BaseCharacterModel Setup(GameObject obj)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            AnimationCharacterModel characterModel = obj.AddComponent<AnimationCharacterModel>();
#pragma warning restore CS0618 // Type or member is obsolete
            characterModel.legacyAnimation = obj.GetComponentInChildren<Animation>();
            return characterModel;
        }
    }
}
