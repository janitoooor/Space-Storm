using UnityEngine;

namespace Game.Battle.Weapon
{
	public sealed class CharacterShooter : MonoBehaviour
	{
		private const int PoolSize = 6;

		private RifleBullet[][] pools;
		private int[] nextBullet;
		private Collider2D ownerCollider;
		private float cooldown;

		private void Awake()
			=> ownerCollider = GetComponent<Collider2D>();

		public void Bind(RifleBullet[] prefabs)
		{
			if (pools != null)
				return;

			pools = new RifleBullet[prefabs.Length][];
			nextBullet = new int[prefabs.Length];
			for (int i = 0; i < prefabs.Length; i++)
			{
				pools[i] = new RifleBullet[PoolSize];
				for (int n = 0; n < PoolSize; n++)
				{
					RifleBullet bullet = Instantiate(prefabs[i], transform);
					bullet.gameObject.SetActive(false);
					pools[i][n] = bullet;
				}
			}
		}

		public void Tick()
		{
			if (cooldown > 0f)
				cooldown -= Time.deltaTime;
		}

		public void ClearCooldown()
			=> cooldown = 0f;

		public bool TryFire(int weaponIndex, Vector2 direction, Vector2 origin, int damage, float fireInterval)
		{
			if (cooldown > 0f || pools == null)
				return false;

			cooldown = fireInterval;
			RifleBullet[] pool = pools[weaponIndex];
			RifleBullet bullet = pool[nextBullet[weaponIndex]];
			nextBullet[weaponIndex] = (nextBullet[weaponIndex] + 1) % PoolSize;
			bullet.Launch(origin, direction, damage, ownerCollider);
			return true;
		}
	}
}
