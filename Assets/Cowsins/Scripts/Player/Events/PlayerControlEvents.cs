using UnityEngine.Events;

namespace cowsins2D
{
    [System.Serializable]
    public class PlayerControlEvents
    {
        public UnityEvent onGrantControl = new UnityEvent(),
            onLoseControl = new UnityEvent();
    }
}