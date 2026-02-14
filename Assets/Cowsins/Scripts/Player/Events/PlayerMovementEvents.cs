using UnityEngine.Events; 

namespace cowsins2D
{
    [System.Serializable]
    public class PlayerMovementEvents
    {
        public UnityEvent onIdle = new UnityEvent();
        public UnityEvent onWalking = new UnityEvent();
        public UnityEvent onRunning = new UnityEvent();
        public UnityEvent onTurn = new UnityEvent();
        public UnityEvent onJump = new UnityEvent();
        public UnityEvent onStartFall = new UnityEvent();
        public UnityEvent onLand = new UnityEvent();
        public UnityEvent onWallJump = new UnityEvent();
        public UnityEvent onStartWallSliding = new UnityEvent();
        public UnityEvent onWallSliding = new UnityEvent();
        public UnityEvent onStartCrouch = new UnityEvent();
        public UnityEvent onCrouched = new UnityEvent();
        public UnityEvent onCrouchedIdle = new UnityEvent();
        public UnityEvent onCrouchWalking = new UnityEvent();
        public UnityEvent onStopCrouch = new UnityEvent();
        public UnityEvent onStartDash = new UnityEvent();
        public UnityEvent onIdleLadder = new UnityEvent();
        public UnityEvent onMovingLadder = new UnityEvent();
        public UnityEvent onStartGlide = new UnityEvent();
        public UnityEvent onGliding = new UnityEvent();
        public UnityEvent onStopGliding = new UnityEvent();
    }
}