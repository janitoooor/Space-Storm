using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Battle.Weapon
{
	public sealed class WeaponLoadout : MonoBehaviour
	{
		[SerializeField]
		private WeaponsConfig weapons;

		[SerializeField]
		private WeaponRenderer weaponRenderer;

		[SerializeField]
		private CharacterShooter shooter;

		private const int None = -1;

		private int[] ammo;
		private bool[] owned;
		private int[] pickupOrder;
		private int pickupCount;
		private int selected = None;
		private float reloadRemaining;
		private bool armed;
		private bool emptyLatch;
		private int emptySignal;
		private int reloadStarted;
		private int reloadFinished;
		private int shotsFired;

		private bool suppressed;

		public int EmptySignal => emptySignal;
		public int ShotsFired => shotsFired;
		public int ReloadStarted => reloadStarted;
		public int ReloadFinished => reloadFinished;

		public bool IsArmed => armed;
		public bool IsDrawn => selected >= 0;
		public int Selected => selected;
		public bool IsReloading => reloadRemaining > 0f;
		public int Count => weapons.Count;
		public int PickupCount => pickupCount;

		public float ReloadProgress
		{
			get
			{
			if (!IsDrawn || reloadRemaining <= 0f)
				return 0f;

				float duration = weapons.Get(selected).ReloadSeconds;
				if (duration <= 0f)
					return 1f;

				return 1f - reloadRemaining / duration;
			}
		}

		public Vector2 AimOrigin
			=> weaponRenderer.AimOrigin;

		public FirearmDefinition Get(int index)
			=> weapons.Get(index);

		public int AmmoIn(int index)
			=> ammo[index];

		public bool Owns(int index)
			=> index >= 0 && index < owned.Length && owned[index];

		public int WeaponAtSlot(int slot)
			=> pickupOrder[slot];

		private void Awake()
		{
			int count = weapons.Count;
			ammo = new int[count];
			owned = new bool[count];
			pickupOrder = new int[count];
			RifleBullet[] prefabs = new RifleBullet[count];
			for (int i = 0; i < count; i++)
				prefabs[i] = weapons.Get(i).Projectile;

			shooter.Bind(prefabs);
		}

		private void Update()
		{
			if (Time.timeScale <= 0f)
				return;

			shooter.Tick();
			weaponRenderer.Tick();
			if (!armed)
				return;

			TickReload();
			TickInput();
		}

		public bool Collect(int index)
		{
			if (index < 0 || index >= owned.Length || owned[index])
				return false;

			owned[index] = true;
			pickupOrder[pickupCount] = index;
			pickupCount++;
			armed = true;
			ammo[index] = weapons.Get(index).MagazineSize;
			if (IsReloading)
				return true;

			selected = index;
			reloadRemaining = 0f;
			emptyLatch = false;
			shooter.ClearCooldown();
			ApplySelected();
			return true;
		}

		public void SetSuppressed(bool value)
			=> suppressed = value;

		public void Hide()
			=> weaponRenderer.HideRenderer();

		public void Aim(Vector2 direction)
			=> weaponRenderer.Aim(direction);

		public bool TryShoot(Vector2 direction)
		{
			if (!IsDrawn || IsReloading || suppressed)
				return false;

			FirearmDefinition firearm = weapons.Get(selected);
			if (ammo[selected] <= 0)
				return false;

			if (!shooter.TryFire(selected, direction, weaponRenderer.MuzzleOrigin(), firearm.Damage, firearm.FireInterval))
				return false;

			ammo[selected]--;
			shotsFired++;
			weaponRenderer.PlayMuzzle(direction);
			weaponRenderer.PlayRecoil();
			emptyLatch = false;
			if (ammo[selected] <= 0)
				emptySignal++;
			return true;
		}

		public bool ConsumeEmptyClick()
		{
			if (!IsDrawn || IsReloading || suppressed || emptyLatch || ammo[selected] > 0)
				return false;

			emptyLatch = true;
			emptySignal++;
			return true;
		}

		public void ReleaseTrigger()
			=> emptyLatch = false;

		private void TickInput()
		{
			Keyboard keyboard = Keyboard.current;
			if (keyboard == null)
				return;

			if (keyboard.digit1Key.wasPressedThisFrame)
				SelectSlot(0);
			else if (keyboard.digit2Key.wasPressedThisFrame)
				SelectSlot(1);
			else if (keyboard.digit3Key.wasPressedThisFrame)
				SelectSlot(2);

			if (keyboard.escapeKey.wasPressedThisFrame)
				Holster();

			if (keyboard.rKey.wasPressedThisFrame)
				BeginReload();
		}

		private void SelectSlot(int slot)
		{
			if (slot < 0 || slot >= pickupCount)
				return;

			Select(pickupOrder[slot]);
		}

		private void Select(int index)
		{
			if (IsReloading || !Owns(index))
				return;

			if (index == selected)
			{
				Holster();
				return;
			}

			selected = index;
			reloadRemaining = 0f;
			emptyLatch = false;
			shooter.ClearCooldown();
			ApplySelected();
		}

		private void Holster()
		{
			if (!IsDrawn || IsReloading)
				return;

			selected = None;
			reloadRemaining = 0f;
			emptyLatch = false;
			shooter.ClearCooldown();
			weaponRenderer.HideRenderer();
		}

		private void BeginReload()
		{
			if (!IsDrawn || IsReloading || suppressed)
				return;

			FirearmDefinition firearm = weapons.Get(selected);
			if (ammo[selected] >= firearm.MagazineSize)
				return;

			reloadRemaining = firearm.ReloadSeconds;
			reloadStarted++;
		}

		private void TickReload()
		{
			if (reloadRemaining <= 0f)
				return;

			reloadRemaining -= Time.deltaTime;
			if (reloadRemaining > 0f)
				return;

			reloadRemaining = 0f;
			ammo[selected] = weapons.Get(selected).MagazineSize;
			reloadFinished++;
		}

		private void ApplySelected()
		{
			FirearmDefinition firearm = weapons.Get(selected);
			weaponRenderer.ApplyVisual(firearm.Sprite, firearm.SpriteUp, firearm.SpriteDown, firearm.Color, firearm.Scale);
		}
	}
}
