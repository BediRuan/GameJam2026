namespace cowsins2D
{
    using UnityEngine;
    public interface IPlayerStats
    {
        PlayerStatsEvents PlayerStatsEvents { get; set; }
        float Health { get; }
        float Shield { get; }
        float MaxHealth { get; }
        float MaxShield { get; }
        bool IsDead { get; }
        void Damage(float amount);
        void Heal(float amount);
        void UseJumpPad();
    }
}