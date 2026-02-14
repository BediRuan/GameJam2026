using UnityEngine;
namespace cowsins2D
{
    public class PlayerStates : MonoBehaviour
    {
        PlayerBaseState _currentState;
        PlayerStateFactory _states;

        public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
        public PlayerStateFactory _States { get { return _states; } set { _states = value; } }

        public PlayerMovement PlayerMovement { get; private set; }
        public IPlayerStats PlayerStats { get; private set; }
        public IPlayerControl PlayerControl { get; private set; }
        public PlayerAnimator PlayerAnimator { get; private set; }
        public Rigidbody2D Rigidbody2D { get; private set; }
        public PlayerDependencies PlayerDependencies { get; private set; }

        static PlayerStates _instance;
        public static PlayerStates instance
        {
            get
            {
                return _instance;
            }
        }

        private void Start()
        {
            GetReferences();

            _instance = this;

            _states = new PlayerStateFactory(this);
            _currentState = _states.Default();
            _currentState.EnterState();
        }

        private void Update()
        {
            if (PlayerStats.Health <= 0) ForceChangeState(_States.Die());
            _currentState.UpdateState();
        }

        private void FixedUpdate()
        {
            _currentState.FixedUpdateState();
        }

        public void ForceChangeState(PlayerBaseState state)
        {
            CurrentState.ExitState();
            CurrentState = state;
            CurrentState.EnterState();
        }

        private void GetReferences()
        {
            PlayerDependencies = GetComponent<PlayerDependencies>();
            PlayerMovement = (PlayerMovement)PlayerDependencies.PlayerMovement;
            PlayerStats = PlayerDependencies.PlayerStats;
            PlayerControl = PlayerDependencies.PlayerControl;
            PlayerAnimator = GetComponent<PlayerAnimator>();
            Rigidbody2D = PlayerDependencies.Rigidbody;
        }
    }
}