using UnityEngine.Events;

namespace cowsins2D
{
    [System.Serializable]
    public class InteractionManagerEvents
    {
        public UnityEvent onInteract = new UnityEvent(), 
            onDrop = new UnityEvent();
    }
}