using Game.Battle;
using UnityEngine;

namespace Game.Battle.Character
{
    public sealed class SuitAudio : MonoBehaviour, IHitFeedback
    {
        private const float WalkStepInterval = 0.46f;
        private const float RunStepInterval = 0.3f;
        private const float StepPitchSpread = 0.04f;
        private const float PistolVolume = 0.4f;
        private const float RifleVolume = 0.5f;
        private const float HeavyVolume = 0.62f;
        private const float StepVolume = 0.22f;
        private const float PickupVolume = 0.5f;
        private const float EmptyVolume = 0.4f;
        private const float HitVolume = 0.45f;
        private const float ReloadVolume = 0.5f;
        private const float ReloadReadyVolume = 0.38f;
        private const int PistolIndex = 0;
        private const int HeavyIndex = 2;

        [SerializeField]
        private AudioSource shots;

        [SerializeField]
        private AudioSource steps;

        [SerializeField]
        private AudioSource cues;

        [SerializeField]
        private AudioClip pistolShot;

        [SerializeField]
        private AudioClip rifleShot;

        [SerializeField]
        private AudioClip heavyShot;

        [SerializeField]
        private AudioClip stepClip;

        [SerializeField]
        private AudioClip hitClip;

        [SerializeField]
        private AudioClip pickupClip;

        [SerializeField]
        private AudioClip emptyClip;

        [SerializeField]
        private AudioClip reloadClip;

        [SerializeField]
        private AudioClip reloadReadyClip;

        private float stepTimer;

        public void PlayShot(int weaponIndex)
        {
            AudioClip clip = rifleShot;
            float volume = RifleVolume;
            if (weaponIndex == PistolIndex)
            {
                clip = pistolShot;
                volume = PistolVolume;
            }
            else if (weaponIndex >= HeavyIndex)
            {
                clip = heavyShot;
                volume = HeavyVolume;
            }

            shots.pitch = 1f;
            shots.PlayOneShot(clip, volume);
        }

        public void PlayHit()
            => cues.PlayOneShot(hitClip, HitVolume);

        public void PlayPickup()
            => cues.PlayOneShot(pickupClip, PickupVolume);

        public void PlayEmpty()
            => cues.PlayOneShot(emptyClip, EmptyVolume);

        public void PlayReload()
            => cues.PlayOneShot(reloadClip, ReloadVolume);

        public void PlayReloadReady()
            => cues.PlayOneShot(reloadReadyClip, ReloadReadyVolume);

        public void TickSteps(bool moving, bool sprinting)
        {
            if (!moving)
            {
                stepTimer = 0f;
                return;
            }

            stepTimer -= Time.deltaTime;
            if (stepTimer > 0f)
                return;

            stepTimer = sprinting ? RunStepInterval : WalkStepInterval;
            steps.pitch = 1f + Random.Range(-StepPitchSpread, StepPitchSpread);
            steps.PlayOneShot(stepClip, StepVolume);
        }
    }
}
