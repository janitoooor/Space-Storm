using UnityEngine;

namespace Game.Battle.Weapon
{
	public sealed class WeaponRenderer : MonoBehaviour
	{
		private const float BarrelAngleOffset = 180f;
		private const float MuzzleGap = 0.08f;
		private const float RecoilDistance = 0.14f;
		private const float RecoilDuration = 0.07f;
		private const int WeaponSorting = 3;

		[SerializeField]
		private Transform aimPivot;

		[SerializeField]
		private SpriteRenderer weaponSprite;

		[SerializeField]
		private MuzzleFlash muzzle;

		private Vector3 restLocal;
		private float recoil;

		private void Awake()
			=> restLocal = weaponSprite.transform.localPosition;

		public Vector2 AimOrigin
			=> aimPivot.position;

		public void Tick()
		{
			if (recoil <= 0f)
				return;

			recoil -= Time.deltaTime;
			float blend = recoil <= 0f ? 0f : recoil / RecoilDuration;
			weaponSprite.transform.localPosition = restLocal + new Vector3(RecoilDistance * blend, 0f, 0f);
		}

		public void HideRenderer()
		{
			recoil = 0f;
			weaponSprite.transform.localPosition = restLocal;
			if (aimPivot != null)
				aimPivot.gameObject.SetActive(false);
		}

		public void Aim(Vector2 direction)
		{
			if (!aimPivot.gameObject.activeSelf)
				aimPivot.gameObject.SetActive(true);

			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			aimPivot.rotation = Quaternion.Euler(0f, 0f, angle + BarrelAngleOffset);
			weaponSprite.flipY = direction.x > 0f;
			weaponSprite.sortingOrder = WeaponSorting;
		}

		public Vector2 MuzzleOrigin()
			=> muzzle.Origin;

		public void PlayMuzzle(Vector2 direction)
			=> muzzle.Play(direction);

		public void PlayRecoil()
			=> recoil = RecoilDuration;

		public void ApplyVisual(Sprite sprite, Color color, float scale)
		{
			recoil = 0f;
			weaponSprite.transform.localPosition = restLocal;
			weaponSprite.sprite = sprite;
			weaponSprite.color = color;
			weaponSprite.transform.localScale = new Vector3(scale, scale, 1f);
			float tip = weaponSprite.transform.localPosition.x + sprite.bounds.min.x * scale - MuzzleGap;
			muzzle.Place(new Vector3(tip, 0f, 0f));
		}
	}
}
