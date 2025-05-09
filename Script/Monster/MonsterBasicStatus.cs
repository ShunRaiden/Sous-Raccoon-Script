using UnityEngine;

namespace SousRaccoon.Monster
{
    public class MonsterBasicStatus : MonsterStatus
    {
        [SerializeField] private MonsterBasicMovement movement;

        protected override void Start()
        {
            base.Start();
            movement = GetComponent<MonsterBasicMovement>();
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            movement.StartStun();
        }

        public override void Die()
        {
            movement.StartDie();
            base.Die();
        }
    }
}
