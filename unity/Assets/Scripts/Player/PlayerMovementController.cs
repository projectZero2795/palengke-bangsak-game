using UnityEngine;

namespace Palengke.BangSak.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMovementController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField]
        [Min(0f)]
        private float movementSpeed = 4f;

        [SerializeField]
        private bool readKeyboardInput = true;

        private Rigidbody2D body;
        private Vector2 externalInput;
        private Vector2 currentInput;

        public float MovementSpeed
        {
            get => movementSpeed;
            set => movementSpeed = Mathf.Max(0f, value);
        }

        public Vector2 CurrentInput => currentInput;

        public bool ReadsKeyboardInput => readKeyboardInput;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            ConfigureBody();
        }

        private void Update()
        {
            var keyboardInput = readKeyboardInput ? ReadKeyboardInput() : Vector2.zero;
            currentInput = ResolveInput(keyboardInput, externalInput);
        }

        private void FixedUpdate()
        {
            Move(currentInput, Time.fixedDeltaTime);
        }

        public void SetExternalInput(Vector2 input)
        {
            externalInput = ClampInput(input);
        }

        public void SetKeyboardInputEnabled(bool enabled)
        {
            readKeyboardInput = enabled;
            if (!enabled)
            {
                currentInput = Vector2.zero;
            }
        }

        public Vector2 ResolveInput(Vector2 keyboardInput, Vector2 joystickInput)
        {
            var chosenInput = joystickInput.sqrMagnitude > 0.01f ? joystickInput : keyboardInput;
            return ClampInput(chosenInput);
        }

        public Vector2 GetMovementDelta(Vector2 input, float deltaTime)
        {
            return ClampInput(input) * movementSpeed * Mathf.Max(0f, deltaTime);
        }

        private void Move(Vector2 input, float deltaTime)
        {
            if (body == null)
            {
                return;
            }

            body.MovePosition(body.position + GetMovementDelta(input, deltaTime));
        }

        private static Vector2 ReadKeyboardInput()
        {
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        private static Vector2 ClampInput(Vector2 input)
        {
            return input.sqrMagnitude > 1f ? input.normalized : input;
        }

        private void ConfigureBody()
        {
            if (body == null)
            {
                return;
            }

            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }
}
