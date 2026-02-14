using UnityEngine;
namespace cowsins2D
{
    public class PlayerWallJumpState : PlayerBaseState
    {
        public PlayerWallJumpState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory){}

        public override void EnterState()
        {
            player.ReduceJumpAmount();
            player.SetGravityScale(0);

            inputManager.onJumpCut += HandleJumpCutInput;
            inputManager.onDash += Dash;
        }

        public override void UpdateState()
        {
            player.CheckCollisions();
            player.HandleVelocities();

            if (!playerControl.Controllable) return;
            player.orientatePlayer?.Invoke();
            CheckSwitchState();
        }

        public override void FixedUpdateState()
        {
            if (!playerControl.Controllable) return;
            player.Movement();
        }

        public override void ExitState()
        {
            inputManager.onJumpCut -= HandleJumpCutInput;
            inputManager.onDash -= Dash;
        }

        public override void CheckSwitchState()
        {
            // Check ladder interaction
            if (player.ladderAvailable && !player.IsOnLadderCooldown && ((player.AutoEnterLadderInAir && !player.IsGrounded) || inputManager.PlayerInputs.VerticalMovement > 0)) 
            {
                player.StopWallJump();
                SwitchState(_factory.Ladder());
                return;
            }

            // Then, check landing on ground
            if (player.LastOnGroundTime > 0) 
            {
                player.StopWallJump();
                SwitchState(_factory.Default());
                return;
            }

            // Then, check wall slide
            bool canTransitionToWallSlide = player.WallJumpControlTimer <= 0.1f || 
                                           ((player.WallLeft && inputManager.PlayerInputs.HorizontalMovement < 0) ||
                                            (player.WallRight && inputManager.PlayerInputs.HorizontalMovement > 0));
            
            if (canTransitionToWallSlide && player.CheckSlideStatus()) 
            {
                player.StopWallJump();
                SwitchState(_factory.WallSlide());
                return;
            }

            if (player.isJumping && player.rb.linearVelocity.y < 0)
            {
                player.JumpFall();
                SwitchState(_factory.Default());
                return;
            }

            // Time based transition
            if (Time.time - player.wallJumpStartTime > player.WallJumpTime)
            {
                player.StopWallJump();
                SwitchState(_factory.Default());
            }
        }
    }
}