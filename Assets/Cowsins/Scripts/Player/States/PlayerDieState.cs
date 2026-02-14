using UnityEngine;
namespace cowsins2D
{
    public class PlayerDieState : PlayerBaseState
    {
        public PlayerDieState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) {}
        public override void EnterState()
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        public override void UpdateState() { }

        public override void FixedUpdateState() {}

        public override void ExitState()
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        public override void CheckSwitchState() {}
    }
}