using UnityEngine;

namespace Palengke.BangSak.Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class PrototypeCameraFollowController : MonoBehaviour
    {
        public const string ComponentId = "prototype_camera_follow";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "phase20_bounded_follow";

        [Header("Component Contract")]
        [SerializeField]
        private string componentId = ComponentId;

        [SerializeField]
        private int componentVersion = ComponentVersion;

        [SerializeField]
        private string componentVariant = ComponentVariant;

        [Header("Follow")]
        [SerializeField]
        private Transform target;

        [SerializeField]
        private PrototypeMapLayoutController mapLayout;

        [SerializeField]
        [Min(0f)]
        private float followSmoothing = 10f;

        [SerializeField]
        private float cameraZ = -10f;

        [SerializeField]
        private bool snapOnAwake = true;

        private Camera attachedCamera;

        public string ComponentIdValue => componentId;

        public int ComponentVersionValue => componentVersion;

        public string ComponentVariantValue => componentVariant;

        public Transform Target => target;

        public PrototypeMapLayoutController MapLayout => mapLayout;

        public bool HasFollowTarget => target != null;

        private void Awake()
        {
            attachedCamera = GetComponent<Camera>();

            if (snapOnAwake && target != null)
            {
                transform.position = ResolveClampedCameraPosition(target.position);
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var nextPosition = ResolveClampedCameraPosition(target.position);
            if (followSmoothing <= 0f)
            {
                transform.position = nextPosition;
                return;
            }

            var t = 1f - Mathf.Exp(-followSmoothing * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, nextPosition, t);
        }

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
        }

        public void SetMapLayout(PrototypeMapLayoutController layout)
        {
            mapLayout = layout;
        }

        public Vector3 ResolveClampedCameraPosition(Vector3 targetPosition)
        {
            var desired = new Vector3(targetPosition.x, targetPosition.y, cameraZ);

            if (mapLayout == null)
            {
                return desired;
            }

            var bounds = mapLayout.CameraBounds;
            var halfHeight = ResolveOrthographicHalfHeight();
            var halfWidth = ResolveOrthographicHalfWidth(halfHeight);

            var minX = bounds.min.x + halfWidth;
            var maxX = bounds.max.x - halfWidth;
            var minY = bounds.min.y + halfHeight;
            var maxY = bounds.max.y - halfHeight;

            return new Vector3(
                ClampOrCenter(desired.x, minX, maxX, bounds.center.x),
                ClampOrCenter(desired.y, minY, maxY, bounds.center.y),
                cameraZ);
        }

        private float ResolveOrthographicHalfHeight()
        {
            var cameraToUse = attachedCamera != null ? attachedCamera : GetComponent<Camera>();
            return cameraToUse != null && cameraToUse.orthographic ? cameraToUse.orthographicSize : 0f;
        }

        private float ResolveOrthographicHalfWidth(float halfHeight)
        {
            var cameraToUse = attachedCamera != null ? attachedCamera : GetComponent<Camera>();
            var aspect = cameraToUse == null || cameraToUse.aspect <= 0f ? 16f / 9f : cameraToUse.aspect;
            return halfHeight * aspect;
        }

        private static float ClampOrCenter(float value, float min, float max, float center) =>
            min > max ? center : Mathf.Clamp(value, min, max);
    }
}
