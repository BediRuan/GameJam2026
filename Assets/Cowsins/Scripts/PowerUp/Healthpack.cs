using UnityEngine;
namespace cowsins2D
{
    public class Healthpack : PowerUp
    {
        [SerializeField] private float healthAdded;

        // this method gets called when the power up is triggered
        public override void TriggerAction(GameObject target)
        {
            IPlayerStats playerStats = target.GetComponent<IPlayerStats>();
            playerStats?.Heal(healthAdded);
            base.TriggerAction(target);
        }
    }
}