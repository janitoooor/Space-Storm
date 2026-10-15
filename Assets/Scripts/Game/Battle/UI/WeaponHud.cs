using Game.Battle.Weapon;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Battle.UI
{
	public sealed class WeaponHud : MonoBehaviour
	{
		[SerializeField]
		private WeaponLoadout loadout;

		[SerializeField]
		private CanvasGroup canvasGroup;

		[SerializeField]
		private Image[] icons;

		[SerializeField]
		private GameObject[] frames;

		[SerializeField]
		private Text[] ammoLabels;

		[SerializeField]
		private Text magazineLabel;

		private const int UnknownSelection = -2;
		private const int UnknownOwned = -1;
		private const int EmptySlot = -1;

		private static readonly Color PlateRest = new Color(0.1f, 0.1f, 0.12f, 0.92f);
		private static readonly Color PlateEmpty = new Color(0.42f, 0.1f, 0.12f, 0.95f);

		private int shownSelected = UnknownSelection;
		private int shownPickupCount = UnknownOwned;
		private int[] shownOrder;
		private bool[] shownEmpty;
		private bool shownArmed;
		private bool shownMagazine;
		private int shownMagazineAmmo = UnknownOwned;
		private GameObject[] slots;
		private Image[] plates;

		private void Awake()
		{
			slots = new GameObject[icons.Length];
			plates = new Image[icons.Length];
			shownOrder = new int[icons.Length];
			shownEmpty = new bool[icons.Length];
			for (int i = 0; i < icons.Length; i++)
			{
				Transform slot = icons[i].transform.parent;
				slots[i] = slot.gameObject;
				slots[i].transform.SetSiblingIndex(i);
				slots[i].SetActive(false);
				plates[i] = slot.Find("Plate").GetComponent<Image>();
				plates[i].color = PlateRest;
				ammoLabels[i].gameObject.SetActive(false);
				shownOrder[i] = EmptySlot;
			}

			magazineLabel.gameObject.SetActive(false);
		}

		private void LateUpdate()
		{
			bool armed = loadout.IsArmed;
			if (armed != shownArmed)
			{
				shownArmed = armed;
				canvasGroup.alpha = armed ? 1f : 0f;
			}

			if (!armed)
				return;

			int count = loadout.PickupCount;
			if (slots.Length < count)
				count = slots.Length;

			bool orderChanged = count != shownPickupCount;
			if (!orderChanged)
			{
				for (int i = 0; i < count; i++)
				{
					if (shownOrder[i] != loadout.WeaponAtSlot(i))
						orderChanged = true;
				}
			}

			if (orderChanged)
			{
				shownPickupCount = count;
				shownSelected = UnknownSelection;
				for (int i = 0; i < slots.Length; i++)
				{
					bool show = i < count;
					slots[i].SetActive(show);
					if (!show)
					{
						shownOrder[i] = EmptySlot;
						shownEmpty[i] = false;
						plates[i].color = PlateRest;
						continue;
					}

					int weapon = loadout.WeaponAtSlot(i);
					shownOrder[i] = weapon;
					icons[i].sprite = loadout.Get(weapon).Sprite;
				}
			}

			for (int i = 0; i < count; i++)
			{
				bool empty = loadout.AmmoIn(loadout.WeaponAtSlot(i)) <= 0;
				if (empty == shownEmpty[i])
					continue;

				shownEmpty[i] = empty;
				plates[i].color = empty ? PlateEmpty : PlateRest;
			}

			bool showMagazine = loadout.IsDrawn && !loadout.IsReloading;
			if (showMagazine != shownMagazine)
			{
				shownMagazine = showMagazine;
				magazineLabel.gameObject.SetActive(showMagazine);
			}

			int selected = loadout.Selected;
			if (selected != shownSelected)
			{
				shownSelected = selected;
				shownMagazineAmmo = UnknownOwned;
				for (int i = 0; i < frames.Length; i++)
					frames[i].SetActive(i < count && loadout.WeaponAtSlot(i) == selected);
			}

			if (!showMagazine)
				return;

			int ammo = loadout.AmmoIn(selected);
			if (ammo == shownMagazineAmmo)
				return;

			shownMagazineAmmo = ammo;
			int magazine = loadout.Get(selected).MagazineSize;
			magazineLabel.text = $"{ammo}/{magazine}";
		}
	}
}
