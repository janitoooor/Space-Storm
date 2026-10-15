using Game.Battle.Weapon;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Battle.UI
{
	public sealed class WeaponStatusMarker : MonoBehaviour
	{
		private const float BlinkSeconds = 0.32f;

		private static readonly Color AlertColor = new Color(1f, 0.28f, 0.22f, 1f);

		[SerializeField]
		private WeaponLoadout loadout;

		[SerializeField]
		private GameObject alertRoot;

		[SerializeField]
		private Text alertLabel;

		[SerializeField]
		private GameObject reloadRoot;

		[SerializeField]
		private Image reloadFill;

		private float blinkTime;

		private void Awake()
		{
			alertRoot.SetActive(false);
			reloadRoot.SetActive(false);
		}

		private void LateUpdate()
		{
			bool empty = loadout.IsDrawn && !loadout.IsReloading && loadout.AmmoIn(loadout.Selected) <= 0;
			bool reloading = loadout.IsDrawn && loadout.IsReloading;

			if (alertRoot.activeSelf != empty)
				alertRoot.SetActive(empty);

			if (empty)
			{
				blinkTime += Time.deltaTime;
				float wave = Mathf.PingPong(blinkTime / BlinkSeconds, 1f);
				alertLabel.color = Color.Lerp(Color.white, AlertColor, wave);
			}
			else
			{
				blinkTime = 0f;
			}

			if (reloadRoot.activeSelf != reloading)
				reloadRoot.SetActive(reloading);

			if (reloading)
				reloadFill.fillAmount = loadout.ReloadProgress;
		}
	}
}
