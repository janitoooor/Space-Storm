namespace Game.Battle.Weapon
{
    public sealed class Riffle: CollectableWeapon
    {
        public override WeaponType weaponType => WeaponType.Riffle;
        public override float weaponAnimationIndex => 1f;
    }
}