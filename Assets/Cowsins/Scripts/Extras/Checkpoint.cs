using UnityEngine;
using UnityEngine.Events;

namespace cowsins2D
{
    public class Checkpoint : Trigger
    {
        public override void EnterTrigger(GameObject target)
        {
            if(!target.CompareTag("Player")) return;
            PlayerDependencies playerDependencies = target.GetComponent<PlayerDependencies>();

            if(playerDependencies == null ) return;

            // Store a new checkpoint in the checkpoint manager
            playerDependencies.CheckpointManager.SetCheckpoint(this.transform);
            Destroy(this);
            base.EnterTrigger(target);
        }
    }
}