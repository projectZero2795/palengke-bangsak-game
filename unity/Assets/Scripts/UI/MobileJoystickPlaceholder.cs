using Palengke.BangSak.Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Palengke.BangSak.UI
{
    public sealed class MobileJoystickPlaceholder : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField]
        private RectTransform baseTransform;

        [SerializeField]
        private RectTransform handleTransform;

        [SerializeField]
        private PlayerMovementController targetPlayer;

        [SerializeField]
        [Min(1f)]
        private float handleRange = 54f;

        private Vector2 input;

        public Vector2 InputVector => input;

        private void Awake()
        {
            CenterHandle();
        }

        private void Start()
        {
            SafeAreaCanvasLayout.MoveIntoSafeArea(baseTransform);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            CenterHandle();
        }
#endif

        public void SetTarget(PlayerMovementController player)
        {
            targetPlayer = player;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UpdateInput(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateInput(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            input = Vector2.zero;
            CenterHandle();

            if (targetPlayer != null)
            {
                targetPlayer.SetExternalInput(Vector2.zero);
            }
        }

        private void UpdateInput(PointerEventData eventData)
        {
            if (baseTransform == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                baseTransform,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint);

            var clamped = Vector2.ClampMagnitude(localPoint, handleRange);
            input = clamped / handleRange;

            if (handleTransform != null)
            {
                handleTransform.anchoredPosition = clamped;
            }

            if (targetPlayer != null)
            {
                targetPlayer.SetExternalInput(input);
            }
        }

        private void CenterHandle()
        {
            if (handleTransform == null)
            {
                return;
            }

            var centeredAnchor = new Vector2(0.5f, 0.5f);
            handleTransform.anchorMin = centeredAnchor;
            handleTransform.anchorMax = centeredAnchor;
            handleTransform.pivot = centeredAnchor;
            handleTransform.anchoredPosition = Vector2.zero;
        }
    }
}
