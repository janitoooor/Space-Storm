using Game.Battle;
using UnityEngine;

namespace Game.Battle.Weapon
{
	public sealed class RifleBullet : MonoBehaviour
	{
		private const int HitCapacity = 8;
		private const float ViewportPadding = 0.04f;

		[SerializeField]
		private float speed = 14f;

		private readonly RaycastHit2D[] hits = new RaycastHit2D[HitCapacity];

		private Rigidbody2D body;
		private CircleCollider2D circle;
		private Camera gameplayCamera;
		private ContactFilter2D blocking;
		private Rigidbody2D ignoredBody;
		private Vector2 direction;
		private float castRadius;
		private int damage;

		private void Awake()
		{
			body = GetComponent<Rigidbody2D>();
			circle = GetComponent<CircleCollider2D>();
			gameplayCamera = Camera.main;
			float scale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
			castRadius = circle.radius * scale;
		}

		public void Launch(Vector2 origin, Vector2 direction, int damage, Collider2D ignored)
		{
			this.direction = direction;
			this.damage = damage;
			ignoredBody = ignored.attachedRigidbody;
			LayerMask mask = ignored.includeLayers;
			blocking.useLayerMask = true;
			blocking.useTriggers = false;
			blocking.layerMask = mask.value == 0 ? Physics2D.AllLayers : mask;
			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
			transform.SetParent(null, false);
			transform.SetPositionAndRotation(origin, rotation);
			gameObject.SetActive(true);
			body.position = origin;
			body.rotation = angle;
		}

		private void FixedUpdate()
		{
			float distance = speed * Time.fixedDeltaTime;
			if (HitAlong(distance))
				return;

			body.MovePosition(body.position + direction * distance);
		}

		private void LateUpdate()
		{
			if (gameplayCamera == null)
				gameplayCamera = Camera.main;

			if (gameplayCamera == null)
				return;

			Vector3 viewport = gameplayCamera.WorldToViewportPoint(body.position);
			if (viewport.x < -ViewportPadding || viewport.x > 1f + ViewportPadding
				|| viewport.y < -ViewportPadding || viewport.y > 1f + ViewportPadding)
				gameObject.SetActive(false);
		}

		private bool HitAlong(float distance)
		{
			int count = Physics2D.CircleCast(body.position, castRadius, direction, blocking, hits, distance);
			float nearest = float.MaxValue;
			int nearestIndex = -1;
			for (int i = 0; i < count; i++)
			{
				Collider2D collider = hits[i].collider;
				if (collider == null)
					continue;

				if (ignoredBody != null && collider.attachedRigidbody == ignoredBody)
					continue;

				if (hits[i].distance >= nearest)
					continue;

				nearest = hits[i].distance;
				nearestIndex = i;
			}

			if (nearestIndex < 0)
				return false;

			Target target;
			if (hits[nearestIndex].collider.TryGetComponent(out target))
				target.Hit(damage);

			gameObject.SetActive(false);
			return true;
		}
	}
}
