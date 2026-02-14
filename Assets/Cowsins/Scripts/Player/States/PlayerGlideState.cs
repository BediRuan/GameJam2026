using UnityEngine;
namespace cowsins2D
{
    public class PlayerGlideState : PlayerBaseState
    {
        private float timer;
        public PlayerGlideState(PlayerStates currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) {}

        public override void EnterState()
        {
            player.StartGlide();

            if (player.GlideDurationMethod != GlideDurationMethod.None)
                timer = player.MaximumGlideTime;

            inputManager.onJump += HandleJumpInput;
            inputManager.onDash += Dash;
        }

        public override void UpdateState()
        {
            if (!playerControl.Controllable) return;

            player.PlayerMovementEvents.onStartGlide?.Invoke();

            player.GlideVerticalMovement();
            player.CheckCollisions();
            CheckSwitchState();

            if (player.HandleOrientationWhileGliding) player.orientatePlayer?.Invoke();

            if (player.GlideDurationMethod == GlideDurationMethod.None) return;

            RunGlideTimer();
        }

        public override void FixedUpdateState()
        {
            if (!playerControl.Controllable) return;
            player.Movement();

        }

        public override void ExitState()
        {
            player.StopGlide();
            inputManager.onJump -= HandleJumpInput;
            inputManager.onDash -= Dash;
        }

        public override void CheckSwitchState()
        {
            if (player.LastOnGroundTime > 0 || !inputManager.PlayerInputs.Jump) SwitchState(_factory.Default());

            if (player.GlideDurationMethod == GlideDurationMethod.None) return;

            if (timer <= 0) SwitchState(_factory.Default());
        }

        private void RunGlideTimer() => timer -= Time.deltaTime;

    }
}