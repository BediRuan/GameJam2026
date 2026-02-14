using System;
using UnityEngine;

namespace cowsins2D
{

    public class PlayerAnimator : MonoBehaviour
    {
        private IPlayerMovement player;
        private IPlayerStats playerStats;
        private IPlayerControl playerControl;
        private Rigidbody2D rb;
        private PlayerDependencies playerDependencies;

        private string currentState;
        private Animator animator;

        private void Start()
        {
            // Initial settings
            animator = GetComponentInChildren<Animator>();

            if(animator == null) Debug.LogWarning("[COWSINS] There is no Animator component assigned within your Player.");

            playerDependencies = GetComponent<PlayerDependencies>();
            player = playerDependencies.PlayerMovement;
            rb = playerDependencies.Rigidbody;
            playerControl = playerDependencies.PlayerControl;
            playerStats = playerDependencies.PlayerStats;

            IdleAnim(); 

            // Subscribe to the Movement events in order to play the animations properly
            player.PlayerMovementEvents.onStartGlide.AddListener(GlideAnim);
            player.PlayerMovementEvents.onStartWallSliding.AddListener(WallSlidingAnim);
            player.PlayerMovementEvents.onWallJump.AddListener(WallJumpAnim);
            player.PlayerMovementEvents.onStartFall.AddListener(FallAnim);
            player.PlayerMovementEvents.onJump.AddListener(JumpAnim);
            player.PlayerMovementEvents.onCrouchedIdle.AddListener(CrouchAnim);
            player.PlayerMovementEvents.onCrouchWalking.AddListener(CrouchWalkAnim);
            player.PlayerMovementEvents.onIdleLadder.AddListener(LadderAnim);
            player.PlayerMovementEvents.onMovingLadder.AddListener(LadderMoveAnim);
            player.PlayerMovementEvents.onIdle.AddListener(IdleAnim);
            player.PlayerMovementEvents.onWalking.AddListener(WalkAnim);
            player.PlayerMovementEvents.onRunning.AddListener(RunAnim);
            player.PlayerMovementEvents.onLand.AddListener(LandAnim);
            playerControl.PlayerControlEvents.onLoseControl.AddListener(IdleAnim);
            playerStats.PlayerStatsEvents.onDie.AddListener(DieAnim);
        }

        private void OnDisable()
        {
            player.PlayerMovementEvents.onStartGlide.RemoveListener(GlideAnim);
            player.PlayerMovementEvents.onStartWallSliding.RemoveListener(WallSlidingAnim);
            player.PlayerMovementEvents.onWallJump.RemoveListener(WallJumpAnim);
            player.PlayerMovementEvents.onStartFall.RemoveListener(FallAnim);
            player.PlayerMovementEvents.onJump.RemoveListener(JumpAnim);
            player.PlayerMovementEvents.onCrouchedIdle.RemoveListener(CrouchAnim);
            player.PlayerMovementEvents.onCrouchWalking.RemoveListener(CrouchWalkAnim);
            player.PlayerMovementEvents.onIdleLadder.RemoveListener(LadderAnim);
            player.PlayerMovementEvents.onMovingLadder.RemoveListener(LadderMoveAnim);
            player.PlayerMovementEvents.onIdle.RemoveListener(IdleAnim);
            player.PlayerMovementEvents.onWalking.RemoveListener(WalkAnim);
            player.PlayerMovementEvents.onRunning.RemoveListener(RunAnim);
            player.PlayerMovementEvents.onLand.RemoveListener(LandAnim);
            playerControl.PlayerControlEvents.onLoseControl.RemoveListener(IdleAnim);
            playerStats.PlayerStatsEvents.onDie.RemoveListener(DieAnim);
        }

        public void IdleAnim()
        {
            if (!player.IsGrounded || player.isJumping) return;
            ChangeAnimationState("Idle");
        }

        public void WalkAnim()
        {
            if (!player.IsGrounded) return;
            ChangeAnimationState("Walk");
        }

        public void RunAnim()
        {
            if (!player.IsGrounded) return;
            ChangeAnimationState("Run");
        }

        public void LandAnim() => ChangeAnimationState("Land");

        public void GlideAnim() => ChangeAnimationState("Glide");

        private void WallSlidingAnim() => ChangeAnimationState("Slide");

        private void WallJumpAnim() => ChangeAnimationState("WallJump");

        private void FallAnim()
        {
            if (player.isWallSliding) return; 
            ChangeAnimationState("Fall");
        }

        private void JumpAnim()
        {
            if (player.isWallSliding) return;
            ChangeAnimationState("Jump");
        }
        private void CrouchAnim()
        {
            if(rb.linearVelocity.magnitude > 0.5f) return;
            ChangeAnimationState("Crouch");
        }

        private void CrouchWalkAnim()
        {
            ChangeAnimationState("WalkCrouch");
        }

        private void LadderAnim() => ChangeAnimationState("LadderIdle");

        private void LadderMoveAnim() => ChangeAnimationState("LadderMove");

        private void DieAnim() => ChangeAnimationState("Die"); 

        private void ChangeAnimationState(string newState)
        {
            if (currentState == newState) return;

            if(!String.IsNullOrEmpty(currentState)) animator?.ResetTrigger(currentState);
            animator?.SetTrigger(newState);
            currentState = newState;
        }
    }
}