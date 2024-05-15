using UnityEngine;

namespace MultiplayerARPG
{
    public class AnimatorCharacterModelFactory : ICharacterModelFactory
    {
        public string Name => "Animator Character Model (Obsolete)";
        public DimensionType DimensionType => DimensionType.Dimension3D;

        public AnimatorCharacterModelFactory()
        {

        }

        public bool ValidateSourceObject(GameObject obj)
        {
            Animator comp = obj.GetComponentInChildren<Animator>();
            if (comp == null)
            {
                Debug.LogError("Cannot create new entity with `AnimatorCharacterModel`, can't find `Animator` component");
                Object.DestroyImmediate(obj);
                return false;
            }
            return true;
        }

        public BaseCharacterModel Setup(GameObject obj)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            AnimatorCharacterModel characterModel = obj.AddComponent<AnimatorCharacterModel>();
#pragma warning restore CS0618 // Type or member is obsolete
            characterModel.animator = obj.GetComponentInChildren<Animator>();
            return characterModel;
        }
    }
}
