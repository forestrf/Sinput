using System;
using UnityEngine;

namespace SinputSystems {
	public class DeviceInput {
		public readonly InputDeviceType inputType;

		public string displayName;
		public Sprite displaySprite;

		// Custom bound stuff
		public bool isCustom = false;
		public string deviceName = "";

		public string GetDisplayName() {
			if (inputType == InputDeviceType.Keyboard) {
				return keyboardKeyCode.ToString();
			}
			if (inputType == InputDeviceType.Mouse) {
				return mouseInputType.ToString();
			}
			return displayName;
		}


		public DeviceInput(InputDeviceType type) {
			inputType = type;

			if (type == InputDeviceType.Keyboard) allowedSlot = InputDeviceSlot.keyboardAndMouse;
			if (type == InputDeviceType.Mouse) allowedSlot = InputDeviceSlot.keyboardAndMouse;
			if (type == InputDeviceType.Virtual) allowedSlot = InputDeviceSlot.virtual1;
		}

		//////////// ~ keyboard specific stuff ~ ////////////
		public KeyCode keyboardKeyCode; //keycode for if this input is controlled by a keyboard key

		//////////// ~ mouse specific stuff ~ ////////////
		public MouseInputType mouseInputType;

		//////////// ~ gamepad specific stuff ~ ////////////
		public InputDeviceSlot allowedSlot = (InputDeviceSlot) (-1); // Slots that this input is allowed to check
		public CommonGamepadInputs commonMappingType; //if this is set, this input is a preset/default
		public int gamepadButtonNumber; // Button number for if this input is controlled by a gamepad button

		public int gamepadAxisNumber; // Axis number for if this input is controlled by a gamepad axis
		public bool invertAxis;
		public bool clampAxis;
		public bool rescaleAxis; // For rescaling input axis from something else to 0-1
		public float rescaleAxisMin;
		public float rescaleAxisMax;
		public float deadZone; // Setting to allow each gamepad to have its own deadzone.
		public float neutralValue; // TO DO. In the 0-1 range, what is the value that represent neutral/released

		// Stuff for treating axis like a button
		//ButtonAction[] axisButtonState; // State of the axis for when used as a button, updated on the first button checks of a frame. list contains state of this axis for each gamepad slot
		public float axisButtoncompareVal; // Axis button is 'pressed' if (axisValue [compareType] compareVal)

		// All GetAxis() checks will return default value until a measured change occurs, since readings before then can be wrong
		private int useDefaultAxisValueCountdown = 2;
		private float measuredAxisValue = float.NaN;
		public float defaultAxisValue;

		//////////// ~ virtual specific stuff ~ ////////////
		public string virtualInputID;
		//private ButtonAction virtualInputState;

		public bool CheckSlot(InputDeviceSlot slot) {
			if (slot == InputDeviceSlot.any) return true;
			switch (inputType) {
				case InputDeviceType.GamepadAxis:
				case InputDeviceType.GamepadButton:
					return slot >= InputDeviceSlot.gamepad1 && slot <= InputDeviceSlot.gamepad16;
				case InputDeviceType.Keyboard:
				case InputDeviceType.Mouse:
					return slot == InputDeviceSlot.keyboardAndMouse;
				case InputDeviceType.Virtual:
					return slot == InputDeviceSlot.virtual1;
				//case InputDeviceType.XR:
			}
			return false;
		}

		public float AxisCheck(InputDeviceSlot slot) {
			if (!CheckSlot(slot)) return 0;

			//keyboard checks
			if (inputType == InputDeviceType.Keyboard) {
				return Input.GetKey(keyboardKeyCode) ? 1 : 0;
			}

			//gamepad button and axis checks
			if (inputType == InputDeviceType.GamepadButton || inputType == InputDeviceType.GamepadAxis) {
				//if checking any slot, call this function for each possible slot
				if (slot == InputDeviceSlot.any) {
					float greatestV = 0;
					for (int i = (int) InputDeviceSlot.gamepad1; i < (int) InputDeviceSlot.gamepad1 + Sinput.connectedGamepads; i++) {
						greatestV = Math.Max(greatestV, Math.Abs(AxisCheck((InputDeviceSlot) i)));
					}
					return greatestV;
				}

				int slotIndex = ((int) slot) - 1;



				// Don't check slots without a connected gamepad
				if (Sinput.connectedGamepads <= slotIndex) return 0;

				// Make sure the gamepad in this slot is one this input is allowed to check (eg don't check PS4 pad bindings for an XBOX pad)
				if (slot != allowedSlot) return 0;

				//button as axis checks
				if (inputType == InputDeviceType.GamepadButton) {
					//button check now
					return Input.GetKey(SInputEnums.GetGamepadKeyCode(slotIndex, gamepadButtonNumber)) ? 1 : 0;
				}

				// Gamepad axis check
				if (inputType == InputDeviceType.GamepadAxis) {
					float axisValue = Input.GetAxisRaw(SInputEnums.GetAxisString(slotIndex, gamepadAxisNumber - 1));
					if (invertAxis) axisValue *= -1;
					if (rescaleAxis) {
						// Some gamepad axis are -1 to 1 or something when you want them as 0 to 1, EG; triggers on XBONE pad on OSX
						axisValue = Mathf.InverseLerp(rescaleAxisMin, rescaleAxisMax, axisValue);
					}

					if (clampAxis) axisValue = Mathf.Clamp01(axisValue);

					// For this to work, axisValue must range from 0 to 1. Another option would be cuttoff instead of rescaling to the deadzone (or rescaleAxisMin and rescaleAxisMax could be used to set the deadzone)
					axisValue = Mathf.InverseLerp(deadZone, 1, axisValue);

					// We return every axis' default value unless we measure a change first
					// This prevents weird snapping and false button presses if the pad is reporting a weird value to start with
					if (useDefaultAxisValueCountdown > 0) {
						if (axisValue != measuredAxisValue) {
							measuredAxisValue = axisValue;
							useDefaultAxisValueCountdown--;
						}
						if (useDefaultAxisValueCountdown > 0) axisValue = defaultAxisValue;
					}

					return axisValue;
				}

				return 0;
			}


			// Virtual device axis input checks
			if (inputType == InputDeviceType.Virtual) {
				return VirtualInputs.GetVirtualAxis(virtualInputID);
			}

			// Mouseaxis button checks (these don't happen)
			if (inputType == InputDeviceType.Mouse) {
				switch (mouseInputType) {
					case MouseInputType.MouseHorizontal:
						return Input.GetAxisRaw("Mouse Horizontal") * Sinput.mouseSensitivity;
					case MouseInputType.MouseVertical:
						return Input.GetAxisRaw("Mouse Vertical") * Sinput.mouseSensitivity;
					case MouseInputType.MouseMoveRight:
						return Math.Max(Input.GetAxisRaw("Mouse Horizontal") * Sinput.mouseSensitivity, 0);
					case MouseInputType.MouseMoveLeft:
						return -Math.Min(Input.GetAxisRaw("Mouse Horizontal") * Sinput.mouseSensitivity, 0);
					case MouseInputType.MouseMoveUp:
						return Math.Max(Input.GetAxisRaw("Mouse Vertical") * Sinput.mouseSensitivity, 0);
					case MouseInputType.MouseMoveDown:
						return -Math.Min(Input.GetAxisRaw("Mouse Vertical") * Sinput.mouseSensitivity, 0);
					case MouseInputType.MouseScroll:
						return Input.GetAxisRaw("Mouse Scroll");
					case MouseInputType.MouseScrollUp:
						return Math.Max(Input.GetAxisRaw("Mouse Scroll"), 0);
					case MouseInputType.MouseScrollDown:
						return -Math.Min(Input.GetAxisRaw("Mouse Scroll"), 0);
					case MouseInputType.MousePositionX:
						return Input.mousePosition.x;
					case MouseInputType.MousePositionY:
						return Input.mousePosition.y;
					default:
						//it's a click type mouse input, or None. SInputEnums.GetMouseButton can handle any MouseInputType
						return Input.GetKey(SInputEnums.GetMouseButton(mouseInputType)) ? 1 : 0;
				}
			}

			return 0;
		}
	}
}
