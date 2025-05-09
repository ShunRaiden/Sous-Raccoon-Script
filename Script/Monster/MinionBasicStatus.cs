using UnityEngine;

namespace SousRaccoon.Monster
{
    public class MinionBasicStatus : MinionBaseStatus
    {
        [SerializeField] private MinionBasicMovement movement;

        protected override void Start()
        {
            base.Start();
            movement = GetComponent<MinionBasicMovement>();
        }

        public override void Die()
        {
            movement.StartDie();
            base.Die();
        }
    }
}