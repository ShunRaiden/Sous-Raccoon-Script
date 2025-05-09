using UnityEngine;

namespace SousRaccoon.Monster
{
    public class MonsterAttackerStatus : MonsterStatus
    {
        [SerializeField] private MonsterAttackerMovement movement;

        protected override void Start()
        {
            base.Start();
            movement = GetComponent<MonsterAttackerMovement>();
        }
        public override void Die()
        {
            movement.StartDie();
            base.Die();
        }
    }
}
