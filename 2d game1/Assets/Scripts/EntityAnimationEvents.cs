using UnityEngine;

public class EntityAnimationEvents : MonoBehaviour
{
    private Entity player;

    private void Awake()
    {
        player = GetComponentInParent<Entity>();
    }

    public void DamageEnemies()
    {
        player.DamageTargets();
    }
    private void DisableMovementAndJump()
    { player.EnableMovement(false);
    }
    private void EnableMovementAndJump()
    {
        player.EnableMovement(true);
    }

}
