namespace cowsins2D
{
    using UnityEngine;
    public interface IWeaponController
    {
        WeaponControllerEvents WeaponControllerEvents { get; set; }
        Weapon_SO weapon {  get; set; }
        int currentWeapon { get; set; }
        WeaponIdentification[] inventory { get; }
        WeaponIdentification id { get; }
        Weapon_SO[] InitialWeapons { get; }
        Transform WeaponHolder { get; }
        bool canShoot { get; }
        bool reloading { get; }
        float heatAmount { get; }
        bool isWeaponHidden { get; }
        Vector2 aimingOrientation { get; }
        void UnholsterWeapon();
        void InstantiateWeapon(Weapon_SO newWeapon, int index);
        void InstantiateWeapon(Weapon_SO newWeapon, int index, int? currentBullets, int? totalBullets);
        void CheckForBullets();
        void SetWeapon(Weapon_SO newWeapon);
        void ReleaseWeapon(int index);
    }
}