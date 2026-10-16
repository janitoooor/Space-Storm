using UnityEngine;

namespace Game.Battle.Enemy
{
	[CreateAssetMenu(fileName = "enemies", menuName = "Game/Enemies")]
	public sealed class EnemiesConfig : ScriptableObject
	{
		[SerializeField]
		private EnemyDefinition[] enemies;

		public int Count => enemies == null ? 0 : enemies.Length;

		public EnemyDefinition Get(int index)
			=> enemies[index];
	}
}
