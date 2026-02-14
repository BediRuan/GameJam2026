using UnityEngine;

namespace cowsins2D
{
    [DisallowMultipleComponent]
    public class PlayerDependencies : MonoBehaviour
    {
        [SerializeField] private UIController UIController;
        [SerializeField] private CheckPointManager checkpointManager;
        [SerializeField] private ExperienceManager experienceManager;
        [SerializeField] private InputManager inputManager;

        public IPlayerMovement PlayerMovement { get; private set; }
        public IWeaponController WeaponController { get; private set; }
        public IPlayerStats PlayerStats { get; private set; }
        public IPlayerControl PlayerControl { get; private set; }
        public IInventoryManager InventoryManager { get; private set; }
        public IInteractionManager InteractionManager { get; private set; }
        public Rigidbody2D Rigidbody { get; private set; }
        public UIController _UIController => UIController;
        public CheckPointManager CheckpointManager => checkpointManager;
        public ExperienceManager ExperienceManager => experienceManager;
        public InputManager InputManager => inputManager;

        private void Awake()
        {
            PlayerMovement = GetDependency<IPlayerMovement>();
            WeaponController = GetDependency<IWeaponController>();
            PlayerStats = GetDependency<IPlayerStats>();
            PlayerControl = GetDependency<IPlayerControl>();
            InteractionManager = GetDependency<IInteractionManager>();
            InventoryManager = GetDependency<IInventoryManager>();
            Rigidbody = GetDependency<Rigidbody2D>();
        }

        public T GetDependency<T>() where T : class
        {
            var comp = GetComponent(typeof(T));
            if (comp == null)
            {
                Debug.LogWarning($"Dependency of type {typeof(T).Name} not found on {gameObject.name}.", this);
                return null;
            }

            var asT = comp as T;
            if (asT == null)
            {
                Debug.LogWarning($"Found component for {typeof(T).Name} but could not cast on {gameObject.name}.", this);
            }

            return asT;
        }
    }
}
