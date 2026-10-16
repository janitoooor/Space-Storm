using Game.Battle;
using UnityEngine;

namespace Game.Battle.Character
{
    public sealed class PlayerHealth : MonoBehaviour, IDamageable
    {
        private const float FlashSeconds = 0.1f;
        private const float HurtInterval = 0.35f;

        private static readonly Color HitColor = new Color(1f, 0.55f, 0.5f, 1f);

        [SerializeField]
        private int maxHealth = 10;

        [SerializeField]
        private SpriteRenderer body;

        [SerializeField]
        private SuitAudio suitAudio;

        private Color restColor;
        private float flashTime;
        private float hurtTime;
        private int health;

        private void Awake()
        {
            restColor = body.color;
            health = maxHealth;
        }

        public void Hit(int damage)
        {
            if (hurtTime > 0f || damage <= 0)
                return;

            hurtTime = HurtInterval;
            health -= damage;
            flashTime = FlashSeconds;
            body.color = HitColor;
            suitAudio.PlayHit();
            if (health > 0)
                return;

            health = maxHealth;
        }

        private void Update()
        {
            if (hurtTime > 0f)
                hurtTime -= Time.deltaTime;

            if (flashTime <= 0f)
                return;

            flashTime -= Time.deltaTime;
            if (flashTime <= 0f)
                body.color = restColor;
        }
    }
}
