using UnityEngine;

namespace Game.Battle.Character
{
    public sealed class CameraKick : MonoBehaviour
    {
        private const float Duration = 0.08f;

        private Camera gameplayCamera;
        private Vector3 restLocal;
        private Vector3 offset;
        private float timer;

        private void Awake()
        {
            gameplayCamera = GetComponent<Camera>();
            restLocal = transform.localPosition;
        }

        public void Restore()
            => transform.localPosition = restLocal;

        public void Kick(Vector2 aimDirection, float pixels)
        {
            float world = pixels * (gameplayCamera.orthographicSize * 2f) / Screen.height;
            Vector2 back = -aimDirection * world;
            offset = new Vector3(back.x, back.y, 0f);
            timer = Duration;
        }

        public void Apply()
        {
            if (timer <= 0f)
            {
                transform.localPosition = restLocal;
                return;
            }

            timer -= Time.deltaTime;
            float blend = timer <= 0f ? 0f : timer / Duration;
            transform.localPosition = restLocal + offset * blend;
        }
    }
}
