using UnityEngine.Events;

namespace cowsins2D
{
    [System.Serializable]
    public class PlayerStatsEvents
    {
        public UnityEvent onDamage = new UnityEvent(),
            onHeal = new UnityEvent(),
            onDie = new UnityEvent(), 
            onRevive = new UnityEvent();
    }
}