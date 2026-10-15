using System.Collections.Generic;
using Game.Battle;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Battle.Character
{
    [DefaultExecutionOrder(50)]
    public sealed class VisionCone : MonoBehaviour
    {
        private const int RayCount = 80;
        private const int HitCapacity = 16;
        private const int MaskSorting = 8;
        private const int DarkSorting = 9;
        private const int ActorSorting = 12;
        private const float HalfAngle = 50f;
        private const float ViewRange = 40f;
        private const float AimEpsilon = 0.0001f;
        private const float CoverPadding = 4f;
        private const float CameraPlane = 10f;
        private const string MaskShaderName = "Game/VisionMask";
        private const string DarkShaderName = "Game/VisionDark";
        private const string BulletTag = "Bullet";

        private static readonly Vector2 DefaultLook = Vector2.left;
        private static readonly Color Darkness = new Color(0f, 0f, 0f, 0.9f);
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        [SerializeField]
        private CharacterController character;

        [SerializeField]
        private SpriteRenderer cone;

        private readonly RaycastHit2D[] hits = new RaycastHit2D[HitCapacity];
        private readonly List<Vector3> vertices = new List<Vector3>(RayCount + 1);
        private readonly int[] triangles = new int[(RayCount - 1) * 3];

        private ContactFilter2D obstacles;
        private Mesh viewMesh;
        private Mesh coverMesh;
        private Material maskMaterial;
        private Material darkMaterial;
        private Transform cover;
        private SpriteRenderer[] actors;
        private Camera gameplayCamera;

        private void Awake()
        {
            if (cone != null)
                cone.enabled = false;

            obstacles.useTriggers = false;
            obstacles.useLayerMask = true;
            obstacles.layerMask = LayerMask.GetMask("Default");
            BuildTriangles();
            CacheActors();
            CreateView();
            CreateCover();
        }

        private void OnDestroy()
        {
            if (viewMesh != null)
                Destroy(viewMesh);

            if (coverMesh != null)
                Destroy(coverMesh);

            if (maskMaterial != null)
                Destroy(maskMaterial);

            if (darkMaterial != null)
                Destroy(darkMaterial);
        }

        private void LateUpdate()
        {
            if (Time.timeScale <= 0f)
                return;

            KeepActorsVisible();
            FitCover();

            Vector2 look = character.LookDirection;
            if (look.sqrMagnitude < AimEpsilon)
                look = DefaultLook;

            FillView(look);
        }

        private void BuildTriangles()
        {
            for (int i = 0; i < RayCount - 1; i++)
            {
                int write = i * 3;
                triangles[write] = 0;
                triangles[write + 1] = i + 1;
                triangles[write + 2] = i + 2;
            }
        }

        private void CacheActors()
        {
            SpriteRenderer[] found = character.GetComponentsInChildren<SpriteRenderer>(true);
            int count = 0;
            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] == cone || found[i].sortingOrder < 0)
                    continue;

                count++;
            }

            actors = new SpriteRenderer[count];
            int index = 0;
            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] == cone || found[i].sortingOrder < 0)
                    continue;

                actors[index] = found[i];
                index++;
            }
        }

        private void CreateView()
        {
            viewMesh = new Mesh();
            viewMesh.name = "VisionView";
            viewMesh.MarkDynamic();

            maskMaterial = new Material(Shader.Find(MaskShaderName));
            GameObject viewObject = new GameObject("VisionView");
            viewObject.transform.SetParent(transform, false);
            viewObject.hideFlags = HideFlags.HideAndDontSave;
            MeshFilter filter = viewObject.AddComponent<MeshFilter>();
            filter.sharedMesh = viewMesh;
            MeshRenderer renderer = viewObject.AddComponent<MeshRenderer>();
            PrepareRenderer(renderer, maskMaterial, MaskSorting);
        }

        private void CreateCover()
        {
            coverMesh = new Mesh();
            coverMesh.name = "VisionCover";
            Vector3[] corners =
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f)
            };
            int[] coverTriangles = { 0, 2, 1, 2, 3, 1 };
            coverMesh.SetVertices(corners);
            coverMesh.SetTriangles(coverTriangles, 0);
            coverMesh.RecalculateBounds();

            darkMaterial = new Material(Shader.Find(DarkShaderName));
            darkMaterial.SetColor(ColorProperty, Darkness);
            GameObject coverObject = new GameObject("VisionCover");
            coverObject.hideFlags = HideFlags.HideAndDontSave;
            cover = coverObject.transform;
            MeshFilter filter = coverObject.AddComponent<MeshFilter>();
            filter.sharedMesh = coverMesh;
            MeshRenderer renderer = coverObject.AddComponent<MeshRenderer>();
            PrepareRenderer(renderer, darkMaterial, DarkSorting);
        }

        private static void PrepareRenderer(MeshRenderer renderer, Material material, int sortingOrder)
        {
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private void KeepActorsVisible()
        {
            for (int i = 0; i < actors.Length; i++)
                actors[i].sortingOrder = ActorSorting;
        }

        private void FitCover()
        {
            if (gameplayCamera == null)
                gameplayCamera = Camera.main;

            if (gameplayCamera == null)
                return;

            if (cover.parent != gameplayCamera.transform)
                cover.SetParent(gameplayCamera.transform, false);

            cover.localPosition = new Vector3(0f, 0f, CameraPlane);
            cover.localRotation = Quaternion.identity;
            float height = gameplayCamera.orthographicSize * 2f + CoverPadding;
            float width = gameplayCamera.orthographicSize * 2f * gameplayCamera.aspect + CoverPadding;
            cover.localScale = new Vector3(width, height, 1f);
        }

        private void FillView(Vector2 look)
        {
            transform.localRotation = Quaternion.identity;
            Vector2 origin = transform.position;
            float lookAngle = Mathf.Atan2(look.y, look.x) * Mathf.Rad2Deg;
            float step = HalfAngle * 2f / (RayCount - 1);
            vertices.Clear();
            vertices.Add(Vector3.zero);

            for (int i = 0; i < RayCount; i++)
            {
                float radians = (lookAngle - HalfAngle + step * i) * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                float distance = ObstacleDistance(origin, direction);
                Vector3 world = origin + direction * distance;
                vertices.Add(transform.InverseTransformPoint(world));
            }

            viewMesh.SetVertices(vertices);
            viewMesh.SetTriangles(triangles, 0);
            viewMesh.RecalculateBounds();
        }

        private float ObstacleDistance(Vector2 origin, Vector2 direction)
        {
            int count = Physics2D.Raycast(origin, direction, obstacles, hits, ViewRange);
            float nearest = ViewRange;
            for (int i = 0; i < count; i++)
            {
                Collider2D collider = hits[i].collider;
                if (collider == null || collider.isTrigger)
                    continue;

                if (collider.CompareTag(BulletTag))
                    continue;

                if (collider.GetComponent<Target>() != null)
                    continue;

                if (collider.transform.IsChildOf(character.transform))
                    continue;

                if (hits[i].distance < nearest)
                    nearest = hits[i].distance;
            }

            return nearest;
        }
    }
}
