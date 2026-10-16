using UnityEngine;

namespace Game.Battle.Enemy
{
	public sealed class EnemyHealth : MonoBehaviour, IDamageable
	{
		private const float FlashSeconds = 0.08f;

		private static readonly Color HitColor = new Color(1f, 0.45f, 0.4f, 1f);

		private SpriteRenderer spriteRenderer;
		private Color restColor;
		private float flashTime;
		private int health;
		private bool dead;

		public bool IsDead => dead;

		public void Bind(int amount, SpriteRenderer renderer)
		{
			spriteRenderer = renderer;
			restColor = renderer.color;
			health = amount;
			dead = amount <= 0;
		}

		public void Hit(int damage)
		{
			if (dead || spriteRenderer == null)
				return;

			health -= damage;
			flashTime = FlashSeconds;
			spriteRenderer.color = HitColor;
			if (health > 0)
				return;

			dead = true;
			spriteRenderer.color = restColor;
		}

		private void Update()
		{
			if (dead || flashTime <= 0f || spriteRenderer == null)
				return;

			flashTime -= Time.deltaTime;
			if (flashTime <= 0f)
				spriteRenderer.color = restColor;
		}
	}
}
