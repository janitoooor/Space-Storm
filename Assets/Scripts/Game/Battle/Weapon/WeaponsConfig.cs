using UnityEngine;

namespace Game.Battle.Weapon
{
	[CreateAssetMenu(fileName = "weapons", menuName = "Game/Weapons")]
	public sealed class WeaponsConfig : ScriptableObject
	{
		[SerializeField]
		private FirearmDefinition[] firearms;

		public int Count => firearms == null ? 0 : firearms.Length;

		public FirearmDefinition Get(int index)
			=> firearms[index];
	}
}
