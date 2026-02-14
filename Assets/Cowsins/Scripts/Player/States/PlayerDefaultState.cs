using UnityEngine;

namespace cowsins2D
{
    public class PlayerDefaultState : PlayerBaseState
    {
        public PlayerDefaultState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory){}

        public override void EnterState()
        {
            // Reset Jumping
            player.isJumping = false;

            inputManager.onJump += HandleJumpInput;
            inputManager.onDash += Dash;
        }

        public override void UpdateState()
        {
            player.CheckCollisions();
            player.HandleVelocities();

            // Avoid running the following code if the player is not controllable
            if (!playerControl.Controllable) return;
            player.CheckIfJumpingOrWallSliding();
            player.orientatePlayer?.Invoke();
            player.HandleFootsteps();
            CheckSwitchState();
        }

        public override void FixedUpdateState()
        {
            // Avoid running the following code if the player is not controllable
            if (!playerControl.Controllable) return;
            // Handles the player movement
            player.Movement();
        }

        public override void ExitState()
        {
            inputManager.onJump -= HandleJumpInput;
            inputManager.onDash -= Dash;
        }

        public override void CheckSwitchState()
        {
            // Check ladder entry. It must have a ladder available and not be on cooldown
            if (player.ladderAvailable && !player.IsOnLadderCooldown && ((player.AutoEnterLadderInAir && !player.IsGrounded) || inputManager.PlayerInputs.VerticalMovement > 0))
            {
                SwitchState(_factory.Ladder());
                return;
            }

            if(player.CheckIfPerformJump()) SwitchState(_factory.Jump());

            if (player.CheckSlideStatus()) SwitchState(_factory.WallSlide());

            if (inputManager.PlayerInputs.Crouch && player.AllowCrouch && player.CurrentLadder == null) SwitchState(_factory.Crouch());

            if (inputManager.PlayerInputs.Jump && player.currentJumps <= 0 && player.LastOnGroundTime <= 0 && player.LastPressedJumpTime > 0 && player.CanGlide) SwitchState(_factory.Glide());
        }
    }
}