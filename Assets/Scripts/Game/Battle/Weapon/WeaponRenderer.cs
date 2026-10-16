using UnityEngine;

namespace Game.Battle.Weapon
{
	public sealed class WeaponRenderer : MonoBehaviour
	{
		private const float MuzzleGap = 0.08f;
		private const float RecoilDistance = 0.14f;
		private const float RecoilDuration = 0.07f;
		private const float AimHalfAngle = 45f;
		private const float BarrelAngleOffset = 180f;
		private const float RightStraight = 0f;
		private const float LeftStraight = 180f;
		private const float UpStraight = 90f;
		private const float DownStraight = -90f;
		private const int SideSorting = 1;
		private const int UpSorting = -1;
		private const int DownSorting = 1;

		private static readonly Vector3 SideHold = new Vector3(0.41f, -0.58f, 0f);
		private static readonly Vector3 UpHold = new Vector3(0f, 0.34f, 0f);
		private static readonly Vector3 DownHold = new Vector3(0f, -1f, 0f);

		[SerializeField]
		private Transform aimPivot;

		[SerializeField]
		private SpriteRenderer weaponSprite;

		[SerializeField]
		private MuzzleFlash muzzle;

		private Sprite sideSprite;
		private Sprite upSprite;
		private Sprite downSprite;
		private Vector3 poseLocal;
		private Vector3 barrelLocal = Vector3.left;
		private Quaternion poseRotation = Quaternion.identity;
		private float recoil;

		private void Awake()
			=> poseLocal = weaponSprite.transform.localPosition;

		public Vector2 AimOrigin
			=> aimPivot.position;

		public void Tick()
		{
			if (recoil <= 0f)
				return;

			recoil -= Time.deltaTime;
			float blend = recoil <= 0f ? 0f : recoil / RecoilDuration;
			ApplyPose(blend);
		}

		public void HideRenderer()
		{
			recoil = 0f;
			poseRotation = Quaternion.identity;
			barrelLocal = Vector3.left;
			if (sideSprite != null)
				weaponSprite.sprite = sideSprite;
			ApplyPose(0f);
			weaponSprite.flipX = false;
			weaponSprite.flipY = false;
			if (aimPivot != null)
				aimPivot.gameObject.SetActive(false);
		}

		public void Aim(Vector2 direction)
		{
			if (!aimPivot.gameObject.activeSelf)
				aimPivot.gameObject.SetActive(true);

			aimPivot.localRotation = Quaternion.identity;
			bool vertical = Mathf.Abs(direction.y) > Mathf.Abs(direction.x);
			bool faceRight = direction.x > 0f;
			if (!vertical)
			{
				poseLocal = Hold(new Vector3(faceRight ? SideHold.x : -SideHold.x, SideHold.y, 0f));
				float straight = faceRight ? RightStraight : LeftStraight;
				poseRotation = Quaternion.Euler(0f, 0f, ClampedAim(direction, straight) + BarrelAngleOffset);
				barrelLocal = Vector3.left;
				if (sideSprite != null)
					weaponSprite.sprite = sideSprite;
				weaponSprite.flipX = false;
				weaponSprite.flipY = faceRight;
				weaponSprite.sortingOrder = SideSorting;
			}
			else if (direction.y > 0f)
			{
				poseLocal = Hold(UpHold);
				poseRotation = Quaternion.Euler(0f, 0f, ClampedAim(direction, UpStraight) - UpStraight);
				barrelLocal = Vector3.up;
				if (upSprite != null)
					weaponSprite.sprite = upSprite;
				weaponSprite.flipX = false;
				weaponSprite.flipY = false;
				weaponSprite.sortingOrder = UpSorting;
			}
			else
			{
				poseLocal = Hold(DownHold);
				poseRotation = Quaternion.Euler(0f, 0f, ClampedAim(direction, DownStraight) - DownStraight);
				barrelLocal = Vector3.down;
				if (downSprite != null)
					weaponSprite.sprite = downSprite;
				weaponSprite.flipX = false;
				weaponSprite.flipY = false;
				weaponSprite.sortingOrder = DownSorting;
			}

			ApplyPose(recoil > 0f ? recoil / RecoilDuration : 0f);
			PlaceMuzzle();
		}

		private static float ClampedAim(Vector2 direction, float straight)
		{
			float aim = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			float delta = Mathf.DeltaAngle(straight, aim);
			if (delta > AimHalfAngle)
				delta = AimHalfAngle;
			else if (delta < -AimHalfAngle)
				delta = -AimHalfAngle;

			return straight + delta;
		}

		public Vector2 MuzzleOrigin()
			=> muzzle.Origin;

		public void PlayMuzzle(Vector2 direction)
			=> muzzle.Play(direction);

		public void PlayRecoil()
			=> recoil = RecoilDuration;

		public void ApplyVisual(Sprite side, Sprite up, Sprite down, Color color, float scale)
		{
			recoil = 0f;
			sideSprite = side;
			upSprite = up != null ? up : side;
			downSprite = down != null ? down : side;
			weaponSprite.sprite = side;
			weaponSprite.color = color;
			weaponSprite.transform.localScale = new Vector3(scale, scale, 1f);
			ApplyPose(0f);
			PlaceMuzzle();
		}

		private Vector3 Hold(Vector3 bodyPosition)
			=> bodyPosition - aimPivot.localPosition;

		private void ApplyPose(float recoilBlend)
		{
			weaponSprite.transform.localRotation = poseRotation;
			Vector3 kick = poseRotation * (-barrelLocal * (RecoilDistance * recoilBlend));
			weaponSprite.transform.localPosition = poseLocal + kick;
		}

		private void PlaceMuzzle()
		{
			if (weaponSprite.sprite == null)
				return;

			Bounds bounds = weaponSprite.sprite.bounds;
			float along = Mathf.Abs(barrelLocal.x) * bounds.extents.x + Mathf.Abs(barrelLocal.y) * bounds.extents.y;
			float reach = along * weaponSprite.transform.localScale.x - MuzzleGap;
			Vector3 tip = barrelLocal * reach;
			Vector3 local = weaponSprite.transform.localPosition + weaponSprite.transform.localRotation * tip;
			muzzle.Place(local);
		}
	}
}
