using UnityEngine;

namespace SinputSystems {
	public class InputDevice : MonoBehaviour {
		public InputDeviceSlot playerSlot = InputDeviceSlot.any;
		public bool mouseKeyboardUnified = true;

		private InputDeviceSlot detectedWhenAny = InputDeviceSlot.any;

		public bool IsUsingGamepad() {
			return IsUsingGamepad(playerSlot);
		}

		private bool IsUsingGamepad(InputDeviceSlot slot) {
			switch (slot) {
				case InputDeviceSlot.gamepad1:
				case InputDeviceSlot.gamepad2:
				case InputDeviceSlot.gamepad3:
				case InputDeviceSlot.gamepad4:
				case InputDeviceSlot.gamepad5:
				case InputDeviceSlot.gamepad6:
				case InputDeviceSlot.gamepad7:
				case InputDeviceSlot.gamepad8:
				case InputDeviceSlot.gamepad9:
				case InputDeviceSlot.gamepad10:
				case InputDeviceSlot.gamepad11:
				case InputDeviceSlot.gamepad12:
				case InputDeviceSlot.gamepad13:
				case InputDeviceSlot.gamepad14:
				case InputDeviceSlot.gamepad15:
				case InputDeviceSlot.gamepad16:
				case InputDeviceSlot.virtual1:
					return true;
				case InputDeviceSlot.keyboard:
				case InputDeviceSlot.keyboardAndMouse:
				case InputDeviceSlot.mouse:
					return false;
				case InputDeviceSlot.any:
					detectedWhenAny = Sinput.GetLastUsedDeviceSlot();
					if (mouseKeyboardUnified && (detectedWhenAny == InputDeviceSlot.mouse || detectedWhenAny == InputDeviceSlot.keyboard)) {
						detectedWhenAny = InputDeviceSlot.keyboardAndMouse;
					}
					return detectedWhenAny != InputDeviceSlot.any ? IsUsingGamepad(detectedWhenAny) : false;
			}
			return false;
		}
	}
}
