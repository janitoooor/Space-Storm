using UnityEngine;

namespace Game.Battle.Weapon
{
	public sealed class MuzzleFlash : MonoBehaviour
	{
		private const float Duration = 0.16f;
		private const float StartScale = 0.35f;
		private const float PeakScale = 1.35f;
		private const float KickDistance = 0.12f;

		private SpriteRenderer spriteRenderer;
		private Vector3 restLocalPosition;
		private Vector2 aim;
		private float elapsed;
		private bool playing;
		private bool ready;

		public Vector2 Origin
		{
			get
			{
				EnsureReady();
				return transform.parent.TransformPoint(restLocalPosition);
			}
		}

		public void Place(Vector3 localPosition)
		{
			EnsureReady();
			restLocalPosition = localPosition;
			transform.localPosition = localPosition;
		}

		public void Play(Vector2 aimDirection)
		{
			EnsureReady();
			aim = aimDirection;
			elapsed = 0f;
			playing = true;
			gameObject.SetActive(true);
			ApplyFrame(0f);
		}

		private void OnEnable()
		{
			if (!playing)
				gameObject.SetActive(false);
		}

		private void OnDisable()
			=> playing = false;

		private void Update()
		{
			if (!playing)
				return;

			elapsed += Time.deltaTime;
			float progress = elapsed / Duration;
			if (progress >= 1f)
			{
				playing = false;
				transform.localPosition = restLocalPosition;
				gameObject.SetActive(false);
				return;
			}

			ApplyFrame(progress);
		}

		private void ApplyFrame(float progress)
		{
			float scale = Mathf.Lerp(StartScale, PeakScale, Mathf.Clamp01(progress * 3f));
			if (progress > 0.35f)
				scale = Mathf.Lerp(PeakScale, StartScale, (progress - 0.35f) / 0.65f);

			transform.localScale = new Vector3(scale, scale, 1f);
			float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler(0f, 0f, angle);
			Vector3 restWorld = transform.parent.TransformPoint(restLocalPosition);
			transform.position = restWorld - (Vector3)(aim * (KickDistance * progress));

			Color color = spriteRenderer.color;
			color.a = 1f - progress;
			spriteRenderer.color = color;
			spriteRenderer.flipX = false;
			spriteRenderer.enabled = true;
		}

		private void EnsureReady()
		{
			if (ready)
				return;

			ready = true;
			spriteRenderer = GetComponent<SpriteRenderer>();
			Animator animator = GetComponent<Animator>();
			if (animator != null)
				animator.enabled = false;
			restLocalPosition = transform.localPosition;
		}
	}
}
