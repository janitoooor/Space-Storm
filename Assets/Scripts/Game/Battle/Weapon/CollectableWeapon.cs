using UnityEngine;

namespace Game.Battle.Weapon
{
	public abstract class CollectableWeapon: MonoBehaviour, ICollectableWeapon
	{
		private const int GroundSorting = 0;

		public abstract WeaponType weaponType { get;}
		public abstract float weaponAnimationIndex { get; }
		public abstract int WeaponIndex { get; }

		private void Awake()
		{
			SpriteRenderer renderer = GetComponent<SpriteRenderer>();
			renderer.sortingOrder = GroundSorting;
		}
		
		public void Hide()
			=> gameObject.SetActive(false);
	}
}