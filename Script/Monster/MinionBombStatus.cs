using UnityEngine;

namespace SousRaccoon.Monster
{
    public class MinionBombStatus : MinionBaseStatus
    {
        [SerializeField] private MinionBombMovement movement;

        protected override void Start()
        {
            base.Start();
            movement = GetComponent<MinionBombMovement>();
        }

        public override void Die()
        {
            movement.bombCharge.SetActive(false);
            movement.bombExplode.SetActive(false);
            movement.StartDie();
            base.Die();
        }
    }
}