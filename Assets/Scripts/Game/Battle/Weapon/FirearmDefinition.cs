using System;
using UnityEngine;

namespace Game.Battle.Weapon
{
	[Serializable]
	public sealed class FirearmDefinition
	{
		[SerializeField]
		private string weaponName;

		[SerializeField]
		private Sprite sprite;

		[SerializeField]
		private Sprite spriteUp;

		[SerializeField]
		private Sprite spriteDown;

		[SerializeField]
		private Color color = Color.white;

		[SerializeField]
		private float scale = 1f;

		[SerializeField]
		private float fireInterval = 0.3f;

		[SerializeField]
		private int magazineSize = 8;

		[SerializeField]
		private int damage = 1;

		[SerializeField]
		private float reloadSeconds = 1.2f;

		[SerializeField]
		private RifleBullet projectile;

		public string WeaponName => weaponName;
		public Sprite Sprite => sprite;
		public Sprite SpriteUp => spriteUp;
		public Sprite SpriteDown => spriteDown;
		public Color Color => color;
		public float Scale => scale;
		public float FireInterval => fireInterval;
		public int MagazineSize => magazineSize;
		public int Damage => damage;
		public float ReloadSeconds => reloadSeconds;
		public RifleBullet Projectile => projectile;
	}
}
