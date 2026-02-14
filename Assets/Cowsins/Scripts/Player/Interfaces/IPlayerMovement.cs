namespace cowsins2D
{
    using UnityEngine;
    public interface IPlayerMovement
    {
        PlayerMovementEvents PlayerMovementEvents { get; set; }
        Transform Graphics { get; }
        bool isGliding { get; }
        bool ladderAvailable { get; }
        Ladder CurrentLadder { get; }
        bool IsOnLadderCooldown { get; }
        bool facingRight { get; }
        float currentSpeed { get; }
        float RunSpeed { get; }
        bool IsGrounded { get; }
        bool IsFalling { get; }
        float stamina { get; }
        bool UsesStamina { get; }
        float MaxStamina { get; }
        bool isJumping { get; }
        bool isWallJumping { get; }
        bool isWallSliding { get; }
        bool isCrouching { get; }
        bool isDashing { get; }
        bool InvincibleWhileDashing { get; }
        int AmountOfDashes { get; }
        float LastOnGroundTime { get; }
        void OnJumpInput();
        void OnJumpUpInput();
        bool CanDash();
    }
}