using Game.Battle.Weapon;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Battle.Audio
{
	public sealed class GameMusic : MonoBehaviour
	{
		private const float MusicVolume = 0.36f;
		private const float FadeSeconds = 1.5f;
		private const float CombatHoldSeconds = 4.5f;
		private const float LoopDetectSlack = 0.2f;
		private const int MusicPriority = 200;

		[SerializeField]
		private WeaponLoadout loadout;

		[SerializeField]
		private Stem[] exploration;

		[SerializeField]
		private AudioClip combatFallback;

		[SerializeField]
		private AudioClip[] menu;

		private AudioSource current;
		private AudioSource next;
		private float fade = 1f;
		private float heardTime;
		private float combatRemaining;
		private float resumeTime;
		private float parkedTime;
		private float replayTimer;
		private int stemIndex;
		private int heardShots;
		private bool combat;
		private bool paused;
		private AudioClip resumeClip;

		private void Awake()
		{
			current = CreateSource("MusicA");
			next = CreateSource("MusicB");
		}

		private void Start()
		{
			if (exploration == null || exploration.Length == 0)
				return;

			PlayImmediate(exploration[0].Calm, 0f);
		}

		private void OnDestroy()
		{
			if (paused)
				Time.timeScale = 1f;
		}

		private void Update()
		{
			if (exploration == null || exploration.Length == 0)
				return;

			TickPause();
			if (!paused)
				TickCombat();

			TickFade(Time.unscaledDeltaTime);
			if (!paused && fade >= 1f)
				TickLoop();

			KeepCurrentPlaying(Time.unscaledDeltaTime);
		}

		private void KeepCurrentPlaying(float delta)
		{
			if (current.clip == null || current.isPlaying)
			{
				replayTimer = 0.5f;
				return;
			}

			replayTimer -= delta;
			if (replayTimer > 0f)
				return;

			replayTimer = 0.5f;
			current.Play();
		}

		private void TickPause()
		{
			Keyboard keyboard = Keyboard.current;
			if (keyboard == null || !keyboard.pKey.wasPressedThisFrame)
				return;

			if (paused)
			{
				paused = false;
				Time.timeScale = 1f;
				Crossfade(resumeClip, resumeTime);
				return;
			}

			paused = true;
			Time.timeScale = 0f;
			resumeClip = DesiredClip();
			resumeTime = TimeOf(resumeClip);
			Crossfade(PickMenu(), 0f);
		}

		private void TickCombat()
		{
			if (loadout == null)
				return;

			int shots = loadout.ShotsFired;
			if (shots != heardShots)
			{
				heardShots = shots;
				combatRemaining = CombatHoldSeconds;
				if (!combat)
					EnterCombat();

				return;
			}

			if (!combat)
				return;

			combatRemaining -= Time.deltaTime;
			if (combatRemaining > 0f)
				return;

			ExitCombat();
		}

		private void EnterCombat()
		{
			combat = true;
			Stem stem = exploration[stemIndex];
			if (stem.Action != null)
			{
				Crossfade(stem.Action, AudibleTime());
				return;
			}

			parkedTime = AudibleTime();
			Crossfade(combatFallback != null ? combatFallback : stem.Calm, 0f);
		}

		private void ExitCombat()
		{
			combat = false;
			Stem stem = exploration[stemIndex];
			if (stem.Action != null)
			{
				Crossfade(stem.Calm, AudibleTime());
				return;
			}

			Crossfade(stem.Calm, parkedTime);
		}

		private void TickFade(float delta)
		{
			if (fade >= 1f)
				return;

			fade = Mathf.Clamp01(fade + delta / FadeSeconds);
			current.volume = MusicVolume * (1f - fade);
			next.volume = MusicVolume * fade;
			if (fade < 1f)
				return;

			current.Stop();
			AudioSource finished = current;
			current = next;
			next = finished;
			heardTime = current.time;
		}

		private void TickLoop()
		{
			if (combat || current.clip == null || !current.isPlaying)
				return;

			if (current.time + LoopDetectSlack >= heardTime)
			{
				heardTime = current.time;
				return;
			}

			stemIndex++;
			if (stemIndex >= exploration.Length)
				stemIndex = 0;

			Crossfade(exploration[stemIndex].Calm, 0f);
		}

		private AudioClip DesiredClip()
		{
			Stem stem = exploration[stemIndex];
			if (!combat)
				return stem.Calm;

			if (stem.Action != null)
				return stem.Action;

			return combatFallback != null ? combatFallback : stem.Calm;
		}

		private AudioClip PickMenu()
		{
			if (menu == null || menu.Length == 0)
				return DesiredClip();

			int index = Random.Range(0, menu.Length);
			return menu[index];
		}

		private float AudibleTime()
		{
			if (fade < 1f && next.isPlaying)
				return next.time;

			return current.time;
		}

		private float TimeOf(AudioClip clip)
		{
			if (clip == null)
				return 0f;

			if (current.clip == clip)
				return current.time;

			if (next.clip == clip && next.isPlaying)
				return next.time;

			return 0f;
		}

		private void PlayImmediate(AudioClip clip, float time)
		{
			if (clip == null)
				return;

			current.clip = clip;
			current.volume = MusicVolume;
			current.Play();
			if (time > 0.05f)
				current.time = Seek(clip, time);
			next.Stop();
			next.volume = 0f;
			fade = 1f;
			heardTime = current.time;
		}

		private void Crossfade(AudioClip clip, float time)
		{
			if (clip == null)
				return;

			if (fade >= 1f && current.clip == clip && current.isPlaying)
				return;

			if (fade < 1f && next.clip == clip)
				return;

			next.clip = clip;
			next.volume = 0f;
			next.Play();
			if (time > 0.05f)
				next.time = Seek(clip, time);
			fade = 0f;
		}

		private static float Seek(AudioClip clip, float time)
		{
			if (clip.length <= 0.05f || time <= 0f)
				return 0f;

			float limit = clip.length - 0.05f;
			return time > limit ? limit : time;
		}

		private AudioSource CreateSource(string sourceName)
		{
			GameObject child = new GameObject(sourceName);
			child.transform.SetParent(transform, false);
			AudioSource source = child.AddComponent<AudioSource>();
			source.playOnAwake = false;
			source.loop = true;
			source.spatialBlend = 0f;
			source.dopplerLevel = 0f;
			source.priority = MusicPriority;
			source.bypassReverbZones = true;
			source.volume = 0f;
			return source;
		}

		[System.Serializable]
		private struct Stem
		{
			[SerializeField]
			private AudioClip calm;

			[SerializeField]
			private AudioClip action;

			public AudioClip Calm => calm;

			public AudioClip Action => action;
		}
	}
}
