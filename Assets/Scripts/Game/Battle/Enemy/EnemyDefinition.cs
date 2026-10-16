using System;
using UnityEngine;

namespace Game.Battle.Enemy
{
	[Serializable]
	public sealed class EnemyDefinition
	{
		[SerializeField]
		private string enemyName;

		[SerializeField]
		private EnemyPattern pattern;

		[SerializeField]
		private float speed = 3f;

		[SerializeField]
		private int health = 8;

		[SerializeField]
		private int damage = 1;

		[SerializeField]
		private float noticeDistance = 6f;

		[SerializeField]
		private float attackDistance = 1.1f;

		[SerializeField]
		private float contactInterval = 0.7f;

		[SerializeField]
		private AudioClip contactClip;

		public string EnemyName => enemyName;

		public EnemyPattern Pattern => pattern;

		public float Speed => speed;

		public int Health => health;

		public int Damage => damage;

		public float NoticeDistance => noticeDistance;

		public float AttackDistance => attackDistance;

		public float ContactInterval => contactInterval;

		public AudioClip ContactClip => contactClip;
	}
}
