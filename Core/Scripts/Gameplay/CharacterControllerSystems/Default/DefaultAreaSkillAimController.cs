using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultAreaSkillAimController : MonoBehaviour, IAreaSkillAimController
    {
        public const float GROUND_DETECTION_DISTANCE = 30f;
        private readonly RaycastHit[] findGroundRaycastHits = new RaycastHit[32];
        public bool IsAiming { get { return Time.frameCount - _lastUpdateFrame <= 1; } }
        public Transform EntityTransform { get { return GameInstance.PlayingCharacterEntity.EntityTransform; } }

        [SerializeField]
        protected float consoleDistanceRate = 0.75f;

        private int _lastUpdateFrame;
        private bool _beginDragged;
        private GameObject _targetObject;

        public AimPosition UpdateAimControls(Vector2 aimAxes, BaseAreaSkill skill, int skillLevel)
        {
            _lastUpdateFrame = Time.frameCount;
            if (!_beginDragged && skill.targetObjectPrefab != null)
            {
                _beginDragged = true;
                if (_targetObject != null)
                    Destroy(_targetObject);
                _targetObject = Instantiate(skill.targetObjectPrefab);
                _targetObject.SetActive(true);
            }
            if (GameInstance.UseMobileInput())
                return UpdateAimControls_Mobile(aimAxes, skill, skillLevel);
            else if (GameInstance.UseConsoleInput())
                return UpdateAimControls_Console(skill, skillLevel);
            return UpdateAimControls_PC(InputManager.MousePosition(), skill, skillLevel);
        }

        public void FinishAimControls(bool isCancel)
        {
            _beginDragged = false;
            if (_targetObject != null)
                Destroy(_targetObject);
        }

        public AimPosition UpdateAimControls_PC(Vector3 cursorPosition, BaseAreaSkill skill, int skillLevel)
        {
            float castDistance = skill.castDistance.GetAmount(skillLevel);
            Vector3 position = GameplayUtils.CursorWorldPosition(Camera.main, cursorPosition);
            position = GameplayUtils.ClampPosition(EntityTransform.position, position, castDistance);
            position = PhysicUtils.FindGroundedPosition(position, findGroundRaycastHits, GROUND_DETECTION_DISTANCE, GameInstance.Singleton.GetAreaSkillGroundDetectionLayerMask());
            if (_targetObject != null)
                _targetObject.transform.position = position;
            return AimPosition.CreatePosition(position);
        }

        public AimPosition UpdateAimControls_Mobile(Vector2 aimAxes, BaseAreaSkill skill, int skillLevel)
        {
            float castDistance = skill.castDistance.GetAmount(skillLevel);
            Vector3 position = EntityTransform.position + (GameplayUtils.GetDirectionByAxes(Camera.main.transform, aimAxes.x, aimAxes.y) * castDistance);
            position = PhysicUtils.FindGroundedPosition(position, findGroundRaycastHits, GROUND_DETECTION_DISTANCE, GameInstance.Singleton.GetAreaSkillGroundDetectionLayerMask());
            if (_targetObject != null)
                _targetObject.transform.position = position;
            return AimPosition.CreatePosition(position);
        }

        public AimPosition UpdateAimControls_Console(BaseAreaSkill skill, int skillLevel)
        {
            float castDistance = skill.castDistance.GetAmount(skillLevel);
            Vector3 position = EntityTransform.position + (EntityTransform.forward * castDistance * consoleDistanceRate);
            position = PhysicUtils.FindGroundedPosition(position, findGroundRaycastHits, GROUND_DETECTION_DISTANCE, GameInstance.Singleton.GetAreaSkillGroundDetectionLayerMask());
            if (_targetObject != null)
                _targetObject.transform.position = position;
            return AimPosition.CreatePosition(position);
        }
    }
}
