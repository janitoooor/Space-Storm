using UnityEngine;

namespace Game.Battle.Weapon
{
	public sealed class WeaponRenderer : MonoBehaviour
	{
		[SerializeField]
		private GameObject[] weaponsR;

		[SerializeField]
		private GameObject[] weaponsL;

		[SerializeField]
		private SpriteRenderer[] spriteRenderersHand;
		
		public void ShowRenderer()
			=> gameObject.SetActive(true);

		public void HideRenderer()
			=>  gameObject.SetActive(false);

		public void SetFlip(bool isFlip)
		{
			foreach (var weaponR in weaponsR)
				weaponR.gameObject.SetActive(!isFlip);

			foreach (var weaponL in weaponsL)
				weaponL.gameObject.SetActive(isFlip);

			foreach (var spriteRenderer in spriteRenderersHand)
				spriteRenderer.flipX = isFlip;
		}
	}
}