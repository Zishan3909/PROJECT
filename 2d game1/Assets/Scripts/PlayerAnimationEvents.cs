using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private Entity entity;

    private void Awake()
    {
        entity = GetComponentInParent<Entity>();
    }

    public void DamageTargets()
    {
        entity.DamageTargets();
    }
    private void DisableMovementAndJump()
    { entity.EnableMovement(false);
    }
    private void EnableMovementAndJump()
    {
        entity.EnableMovement(true);
    }

}
