using UnityEngine;

namespace Game.Battle
{
	public sealed class Target : MonoBehaviour, IDamageable
	{
		private const float FlashSeconds = 0.08f;
		private const float RespawnSeconds = 0.7f;

		private static readonly Color HitColor = new Color(1f, 0.95f, 0.4f, 1f);

		[SerializeField]
		private int maxHealth = 10;

		private SpriteRenderer spriteRenderer;
		private Collider2D hitCollider;
		private Color restColor;
		private float flashTime;
		private float respawnTime;
		private int health;

		private void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			hitCollider = GetComponent<Collider2D>();
			restColor = spriteRenderer.color;
			health = maxHealth;
		}

		public void Hit(int damage)
		{
			if (respawnTime > 0f)
				return;

			health -= damage;
			flashTime = FlashSeconds;
			spriteRenderer.color = HitColor;
			if (health > 0)
				return;

			health = maxHealth;
			respawnTime = RespawnSeconds;
			spriteRenderer.enabled = false;
			hitCollider.enabled = false;
		}

		private void Update()
		{
			if (respawnTime > 0f)
			{
				respawnTime -= Time.deltaTime;
				if (respawnTime > 0f)
					return;

				spriteRenderer.enabled = true;
				hitCollider.enabled = true;
				spriteRenderer.color = restColor;
				return;
			}

			if (flashTime <= 0f)
				return;

			flashTime -= Time.deltaTime;
			if (flashTime <= 0f)
				spriteRenderer.color = restColor;
		}
	}
}
