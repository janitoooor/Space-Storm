using Game.Battle.Character;
using UnityEngine;

namespace Game.Battle.Enemy
{
	public sealed class EnemyAgent : MonoBehaviour
	{
		private const int CastCapacity = 8;
		private const int IdlePose = 0;
		private const int WalkPose = 1;
		private const int AttackPose = 2;
		private const float PoseLockSeconds = 0.05f;
		private const float MoveEpsilon = 0.0001f;
		private const float SideBlend = 1f;
		private const float UpBlend = 1f;

		private static readonly int MovementX = Animator.StringToHash("movementX");
		private static readonly int MovementY = Animator.StringToHash("movementY");

		[SerializeField]
		private EnemiesConfig config;

		[SerializeField]
		private int definitionIndex;

		private readonly RaycastHit2D[] hits = new RaycastHit2D[CastCapacity];

		private EnemyDefinition definition;
		private EnemyHealth health;
		private Animator animator;
		private SpriteRenderer spriteRenderer;
		private AudioSource audioSource;
		private Rigidbody2D body;
		private CircleCollider2D circle;
		private Transform player;
		private PlayerHealth playerHealth;
		private ContactFilter2D blocking;
		private float contactTime;
		private float castRadius;
		private float poseLock;
		private int pose = -1;
		private bool attacking;
		private bool deathPlayed;
		private bool facingRight;

		private void Awake()
		{
			definition = config.Get(definitionIndex);
			animator = GetComponent<Animator>();
			spriteRenderer = GetComponent<SpriteRenderer>();
			audioSource = GetComponent<AudioSource>();
			body = GetComponent<Rigidbody2D>();
			circle = GetComponent<CircleCollider2D>();
			health = GetComponent<EnemyHealth>();
			health.Bind(definition.Health, spriteRenderer);
			float scale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
			castRadius = circle.radius * scale;
			blocking.useLayerMask = true;
			blocking.useTriggers = false;
			blocking.layerMask = Physics2D.AllLayers;
			if (audioSource == null)
				audioSource = gameObject.AddComponent<AudioSource>();

			audioSource.playOnAwake = false;
			audioSource.spatialBlend = 0f;
			body.useFullKinematicContacts = true;
			PlayerHealth found = FindAnyObjectByType<PlayerHealth>();
			playerHealth = found;
			player = found.transform;
		}

		private void Update()
		{
			if (Time.timeScale <= 0f)
				return;

			if (health.IsDead)
			{
				PlayDeath();
				return;
			}

			if (contactTime > 0f)
				contactTime -= Time.deltaTime;

			Vector2 offset = (Vector2)player.position - body.position;
			float distance = offset.magnitude;
			if (distance > definition.NoticeDistance)
			{
				Show(Vector2.zero, false);
				return;
			}

			if (definition.Pattern == EnemyPattern.Flee)
			{
				Show(offset.sqrMagnitude <= MoveEpsilon ? Vector2.left : -offset, false);
				return;
			}

			if (distance <= definition.AttackDistance)
			{
				Show(offset, true);
				TryHurt();
				return;
			}

			Show(offset, false);
		}

		private void FixedUpdate()
		{
			if (Time.timeScale <= 0f || health.IsDead || attacking)
				return;

			Vector2 offset = (Vector2)player.position - body.position;
			float distance = offset.magnitude;
			if (distance > definition.NoticeDistance || distance <= MoveEpsilon)
				return;

			Vector2 direction = definition.Pattern == EnemyPattern.Flee ? -offset / distance : offset / distance;
			float step = definition.Speed * Time.fixedDeltaTime;
			if (Blocked(direction, step))
				return;

			body.MovePosition(body.position + direction * step);
		}

		private void OnCollisionStay2D(Collision2D collision)
		{
			if (health == null || health.IsDead || definition.Pattern != EnemyPattern.Flee)
				return;

			PlayerHealth touched;
			if (!collision.collider.TryGetComponent(out touched))
				return;

			TryHurt();
		}

		private void TryHurt()
		{
			if (contactTime > 0f)
				return;

			contactTime = definition.ContactInterval;
			playerHealth.Hit(definition.Damage);
			if (definition.ContactClip != null)
				audioSource.PlayOneShot(definition.ContactClip);
		}

		private void PlayDeath()
		{
			if (deathPlayed)
				return;

			deathPlayed = true;
			attacking = false;
			circle.enabled = false;
			animator.Play("die", 0, 0f);
		}

		private void Show(Vector2 travel, bool bite)
		{
			if (poseLock > 0f)
				poseLock -= Time.deltaTime;

			bool moving = travel.sqrMagnitude > MoveEpsilon && !bite;
			int desired = IdlePose;
			if (bite || (moving && definition.Pattern == EnemyPattern.Flee))
				desired = AttackPose;
			else if (moving)
				desired = WalkPose;

			string stateName = desired == AttackPose ? "attack" : desired == WalkPose ? "walk" : "idle";
			AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
			if (pose != desired || (poseLock <= 0f && !state.IsName(stateName)))
			{
				animator.Play(stateName, 0, 0f);
				pose = desired;
				poseLock = PoseLockSeconds;
			}

			attacking = bite;

			if (moving || bite)
				Face(travel);

			animator.SetFloat(MovementX, facingRight || travel.y <= Mathf.Abs(travel.x) ? SideBlend : 0f);
			animator.SetFloat(MovementY, !facingRight && travel.y > Mathf.Abs(travel.x) ? UpBlend : 0f);
		}

		private void Face(Vector2 travel)
		{
			if (Mathf.Abs(travel.y) > Mathf.Abs(travel.x) && travel.y > 0f)
			{
				facingRight = false;
				spriteRenderer.flipX = false;
				return;
			}

			if (Mathf.Abs(travel.x) <= MoveEpsilon)
				return;

			facingRight = travel.x > 0f;
			spriteRenderer.flipX = facingRight;
		}

		private bool Blocked(Vector2 direction, float distance)
		{
			int count = body.Cast(direction, blocking, hits, distance);
			for (int i = 0; i < count; i++)
			{
				Collider2D collider = hits[i].collider;
				if (collider == null || collider == circle)
					continue;

				if (collider.GetComponent<PlayerHealth>() != null)
					continue;

				if (collider.GetComponent<EnemyAgent>() != null)
					continue;

				return true;
			}

			return false;
		}
	}
}
