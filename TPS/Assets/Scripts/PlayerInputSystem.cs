using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PlayerControl
{
	public class PlayerInputSystem : MonoBehaviour
	{
		[Header("Character Input Values")]
        [HideInInspector] public Vector2 move;
        [HideInInspector] public Vector2 look;
        [HideInInspector] public bool jump;
        [HideInInspector] public bool sprint;
        [HideInInspector] public bool aim;
		//public bool shoot;
        [HideInInspector] public bool pickUp;
        [HideInInspector] public bool openStatusPanel;
        [HideInInspector] public bool openBuildMenu;
        [HideInInspector] public bool ping;

        [Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

        [Header("Voice Input Settings")]
        public bool voiceInput; // true 表示正在按下语音键

        public InputActionReference shootAction;
        [HideInInspector] public bool shootPressed;
        [HideInInspector] public bool shootHeld;
        [HideInInspector] public bool shootReleased;
        private void Awake()
        {
            shootAction.action.started += ctx => shootPressed = true;
            shootAction.action.performed += ctx => shootHeld = true;
            shootAction.action.canceled += ctx =>
            {
                shootHeld = false;
                shootReleased = true;
            };

            shootAction.action.Enable();
        }

        private void LateUpdate()
        {
            shootPressed = false;
            shootReleased = false;
        }

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

        public void OnAim(InputValue value)
        {
            AimInput(value.isPressed);
        }

        //public void OnShoot(InputValue value)
        //{

        //    ShootInput(value.isPressed);
        //}

        public void OnVoiceInput(InputValue value)
        {
            VoiceInput(value.isPressed);
        }

        public void OnPickUp(InputValue value)
        {
            PickUpInput(value.isPressed);
        }

        public void OnOpenStatusPanel(InputValue value)
        {
            OpenStatusPanelInput(value.isPressed);
        }

        public void OnOpenBuildMenu(InputValue value)
        {
            OpenBuildMenuInput(value.isPressed);
        }

        public void OnPing(InputValue value)
        {
            PingInput(value.isPressed);
        }

#endif


        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		//鼠标右键瞄准
        public void AimInput(bool newAimState)
        {
            aim = newAimState;
        }

        //public void ShootInput(bool newShootState)
        //{
        //    shoot = newShootState;
        //}

        public void VoiceInput(bool newVoiceInputState)
        {
            voiceInput = newVoiceInputState;
        }

		public void PickUpInput(bool newPickUpInputState)
		{
			pickUp = newPickUpInputState;

        }

        public void OpenStatusPanelInput(bool newStatusPanelInputState)
        {
            openStatusPanel = newStatusPanelInputState;

        }

        public void OpenBuildMenuInput(bool newOpenBuildMenuInputState)
        {
            openBuildMenu = newOpenBuildMenuInputState;

        }

        public void PingInput(bool newPingInputState)
        {
            ping = newPingInputState;

        }

        private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}


    }

}