namespace Game.Battle.Weapon
{
	public sealed class EmptyWeapon: IWeapon
	{
		public WeaponType weaponType => WeaponType.Empty;
		
		public float weaponAnimationIndex => 0;
	}
}