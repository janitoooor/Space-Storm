namespace Game.Battle.Weapon
{
	public interface IWeapon
	{
		WeaponType weaponType { get; }
		float weaponAnimationIndex { get; }
	}
}