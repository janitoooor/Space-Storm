using UnityEngine;

namespace Game.Battle.Weapon
{
	public sealed class WeaponPickup : CollectableWeapon
	{
		[SerializeField]
		private int weaponIndex;

		public override WeaponType weaponType => WeaponType.Empty;
		public override float weaponAnimationIndex => 0f;
		public override int WeaponIndex => weaponIndex;
	}
}
