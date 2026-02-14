using UnityEngine;

namespace cowsins2D
{
    public class PlayerLadderState : PlayerBaseState
    {
        private PlayerAnimator anim;

        public PlayerLadderState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) {}

        public override void EnterState()
        {
            if (player.isGliding) player.StopGlide();
            
            player.SetGravityScale(0);
            player.IsClimbing = true;
        }

        public override void UpdateState()
        {
            player.CheckCollisions();
            if (!playerControl.Controllable) return;
            player.LadderVelocity();
            CheckSwitchState();

            if (rb.linearVelocity.magnitude < .1f) player.PlayerMovementEvents.onIdleLadder?.Invoke();
            else player.PlayerMovementEvents.onMovingLadder?.Invoke();
        }

        public override void FixedUpdateState()
        {
            if (!playerControl.Controllable) return;
            player.Movement();
            player.VerticalMovement();
        }

        public override void ExitState() => player.IsClimbing = false;

        public override void CheckSwitchState()
        {
            // Exit if ladder is no longer available
            if (!player.ladderAvailable)
            {
                SwitchState(_factory.Default());
                return;
            }
            
            if (player.CurrentLadder != null && player.CurrentLadder.IsInExitZone(player.transform.position))
            {
                if (inputManager.PlayerInputs.VerticalMovement > 0)
                {
                    float topY = player.CurrentLadder.GetTopY();
                    if (player.transform.position.y >= topY - 0.01f)
                    {
                        SwitchState(_factory.Default());
                        return;
                    }
                }
            }
            
            // exit if grounded and trying to go down or not moving
            if (player.IsGrounded && inputManager.PlayerInputs.VerticalMovement <= 0)
            {
                SwitchState(_factory.Default());
            }
        }
    }
}