namespace Game.Battle.Weapon
{
	public interface ICollectableWeapon : IWeapon
	{
		int WeaponIndex { get; }

		void Hide();
	}
}