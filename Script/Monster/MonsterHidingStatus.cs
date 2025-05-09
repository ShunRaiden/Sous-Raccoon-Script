using UnityEngine;

namespace SousRaccoon.Monster
{
    public class MonsterHidingStatus : MonsterStatus
    {
        [SerializeField] private MonsterHidingMovement movement;

        protected override void Start()
        {
            base.Start();
            movement = GetComponent<MonsterHidingMovement>();
        }
        public override void TakeDamage(int damage)
        {
            if (movement.canTakeDamage)
            {
                base.TakeDamage(damage);
                movement.StartStun();
            }
        }

        public override void Die()
        {
            base.Die();
            movement.StartDie();
        }
    }
}
