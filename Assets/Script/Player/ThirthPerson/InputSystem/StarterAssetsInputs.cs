using UnityEngine;
using Fusion;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : NetworkBehaviour
	{
		internal string currentControlScheme;
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool aim;
		public bool shoot;
		public bool reload;
		public bool skill;
		public bool throwGrenade;
		public bool kneel;
		public bool light;
		public bool lockMouse;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

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

        public void OnShoot(InputValue value) 
        {
            ShootInput(value.isPressed);
        }
		
		public void OnReload(InputValue value) 
		{
			ReloadInput(value.isPressed);
		}

		public void OnSkill(InputValue value)
		{
			SkillInput(value.isPressed);
		}
		
		public void OnThrowGrenade(InputValue value) 
		{
			ThrowGrenadeInput(value.isPressed);
		}

		public void OnKneel(InputValue value)
		{
			KneelInput(value.isPressed);
		}

		public void OnLight(InputValue value) 
		{
			LightInput(value.isPressed);
		}

		public void OnLockMouse(InputValue value)
		{
			LockMouseInput(value.isPressed);
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

		public void AimInput(bool newAimState)
        {
            aim = newAimState;
        }

        public void ShootInput(bool newShootState) {
            shoot = newShootState;
        }

		public void ReloadInput(bool newReloadState)
		{
			reload = newReloadState;
		}

		public void SkillInput(bool newSkillState)
		{
			skill = newSkillState;
		}

		public void ThrowGrenadeInput(bool newThrowGrenadeState)
		{
			throwGrenade = newThrowGrenadeState;
		}

		public void KneelInput(bool newKneelState)
		{
			kneel = newKneelState;
		}

		public void LightInput(bool newLightState)
		{
			light = newLightState;
		}

		public void LockMouseInput(bool newLockMouseState)
		{
			lockMouse = newLockMouseState;
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