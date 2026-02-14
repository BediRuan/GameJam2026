using UnityEngine.Events;

namespace cowsins2D
{
    [System.Serializable]
    public class WeaponControllerEvents
    {
        public UnityEvent onShoot = new UnityEvent(),
            onStartReload = new UnityEvent(),
            onStopReload = new UnityEvent(),
            onUnholster = new UnityEvent();
    }
}