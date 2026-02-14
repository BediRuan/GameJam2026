namespace cowsins2D
{
    using UnityEngine;
    public interface IPlayerControl
    {
        PlayerControlEvents PlayerControlEvents { get; set; }
        bool Controllable { get; }
        void GrantControl();
        void LoseControl();
        void CheckIfCanGrantControl();
    }
}