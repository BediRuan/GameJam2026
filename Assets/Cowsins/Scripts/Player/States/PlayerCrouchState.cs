using UnityEngine;

namespace cowsins2D
{
    public class PlayerCrouchState : PlayerBaseState
    {
        private bool slide;
        private float slideTimer;
        public PlayerCrouchState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) {}

        public override void EnterState()
        {
            if (rb.linearVelocity.magnitude > player.WalkSpeed && player.currentSpeed > player.WalkSpeed && (player.LastOnGroundTime > 0 || player.LastOnGroundTime <= 0 && player.CanCrouchSlideMidAir))
            {
                slide = true;
                slideTimer = player.CrouchSlideDuration;
            }
            player.StartCrouch();

            inputManager.onJump += HandleJumpInput;
            inputManager.onJumpCut += HandleJumpCutInput;
            inputManager.onDash += Dash;
        }

        public override void UpdateState()
        {
            if (!playerControl.Controllable) return;
            player.CheckCollisions();
            player.orientatePlayer?.Invoke();
            CheckSwitchState();

            player.PlayerMovementEvents.onCrouched?.Invoke();
            if(rb.linearVelocity.magnitude > .1f) player.PlayerMovementEvents.onCrouchWalking?.Invoke();
            else player.PlayerMovementEvents.onCrouchedIdle?.Invoke();

            if (!slide) return;
            slideTimer -= Time.deltaTime;

            if (!inputManager.PlayerInputs.Crouch || slideTimer <= 0) slide = false;
            else player.CrouchSlide();
        }

        public override void FixedUpdateState()
        {
            if (!playerControl.Controllable) return;
            player.Movement();
        }

        public override void ExitState() { 
            player.StopCrouch();

            inputManager.onJump -= HandleJumpInput;
            inputManager.onJumpCut -= HandleJumpCutInput;
            inputManager.onDash -= Dash;
        }

        public override void CheckSwitchState()
        {
            if (player.CheckCeiling() || PauseMenu.isPaused) return;

            if (player.CheckIfPerformJump()) SwitchState(_factory.Jump());

            if (player.isJumping && rb.linearVelocity.y < 0)
            {
                player.JumpFall();
                SwitchState(_factory.Default());
            }

            if (!inputManager.PlayerInputs.Crouch) SwitchState(_factory.Default());
        }
    }
}