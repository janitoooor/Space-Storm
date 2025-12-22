using UnityEngine;

namespace Game.Battle.Weapon
{
	public abstract class CollectableWeapon: MonoBehaviour, ICollectableWeapon
	{
		public abstract WeaponType weaponType { get;}
		public abstract float weaponAnimationIndex { get; }
		
		public void Hide()
			=> gameObject.SetActive(false);
	}
}