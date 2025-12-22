using UnityEngine;

namespace Game.Battle.Weapon
{
	public sealed class WeaponRenderer : MonoBehaviour
	{
		[SerializeField]
		private GameObject weaponR;

		[SerializeField]
		private GameObject weaponL;

		public void ShowRenderer()
			=> gameObject.SetActive(true);

		public void HideRenderer()
			=>  gameObject.SetActive(false);

		public void SetFlip(bool isFlip)
		{
			weaponR.gameObject.SetActive(!isFlip);
			weaponL.gameObject.SetActive(isFlip);
		}
	}
}