﻿using System;
using UnityEngine;

namespace SinputSystems {
	public enum ButtonAction {
		HELD, //held down
		DOWN, //pressed this frame
		UP, //released this frame
		NOTHING, //for no action

		REPEATING //for repeating presses, it can be a type of check but NOT a state a button can itself be in
	}

	public enum MouseInputType {
		None,
		MouseMoveLeft,
		MouseMoveRight,
		MouseMoveUp,
		MouseMoveDown,
		MouseHorizontal,
		MouseVertical,
		MouseScrollUp,
		MouseScrollDown,
		MouseScroll,
		MousePositionX,
		MousePositionY,
		Mouse0,
		Mouse1,
		Mouse2,
		Mouse3,
		Mouse4,
		Mouse5,
		Mouse6
	}
	public enum KeyboardInputType {
		None,
		UpArrow,
		DownArrow,
		RightArrow,
		LeftArrow,
		A,
		B,
		C,
		D,
		E,
		F,
		G,
		H,
		I,
		J,
		K,
		L,
		M,
		N,
		O,
		P,
		Q,
		R,
		S,
		T,
		U,
		V,
		W,
		X,
		Y,
		Z,
		RightShift,
		LeftShift,
		RightControl,
		LeftControl,
		RightAlt,
		LeftAlt,
		LeftCommand,
		LeftApple,
		LeftWindows,
		RightCommand,
		RightApple,
		RightWindows,
		AltGr,
		Backspace,
		Delete,
		Tab,
		Clear,
		Return,
		Pause,
		Escape,
		Space,
		Keypad0,
		Keypad1,
		Keypad2,
		Keypad3,
		Keypad4,
		Keypad5,
		Keypad6,
		Keypad7,
		Keypad8,
		Keypad9,
		KeypadPeriod,
		KeypadDivide,
		KeypadMultiply,
		KeypadMinus,
		KeypadPlus,
		KeypadEnter,
		KeypadEquals,
		Insert,
		Home,
		End,
		PageUp,
		PageDown,
		F1,
		F2,
		F3,
		F4,
		F5,
		F6,
		F7,
		F8,
		F9,
		F10,
		F11,
		F12,
		F13,
		F14,
		F15,
		Alpha0,
		Alpha1,
		Alpha2,
		Alpha3,
		Alpha4,
		Alpha5,
		Alpha6,
		Alpha7,
		Alpha8,
		Alpha9,
		Exclaim,
		DoubleQuote,
		Hash,
		Dollar,
		Ampersand,
		Quote,
		LeftParen,
		RightParen,
		Asterisk,
		Plus,
		Comma,
		Minus,
		Period,
		Slash,
		Colon,
		Semicolon,
		Less,
		Equals,
		Greater,
		Question,
		At,
		LeftBracket,
		Backslash,
		RightBracket,
		Caret,
		Underscore,
		BackQuote,
		Numlock,
		CapsLock,
		ScrollLock,
		Help,
		Print,
		SysReq,
		Break,
		Menu,
	}

	public enum UnityGamepadKeyCode {
		JoystickButton0 = KeyCode.JoystickButton0,
		JoystickButton1,
		JoystickButton2,
		JoystickButton3,
		JoystickButton4,
		JoystickButton5,
		JoystickButton6,
		JoystickButton7,
		JoystickButton8,
		JoystickButton9,
		JoystickButton10,
		JoystickButton11,
		JoystickButton12,
		JoystickButton13,
		JoystickButton14,
		JoystickButton15,
		JoystickButton16,
		JoystickButton17,
		JoystickButton18,
		JoystickButton19,

		Joystick1Button0 = KeyCode.Joystick1Button0,
		Joystick1Button1,
		Joystick1Button2,
		Joystick1Button3,
		Joystick1Button4,
		Joystick1Button5,
		Joystick1Button6,
		Joystick1Button7,
		Joystick1Button8,
		Joystick1Button9,
		Joystick1Button10,
		Joystick1Button11,
		Joystick1Button12,
		Joystick1Button13,
		Joystick1Button14,
		Joystick1Button15,
		Joystick1Button16,
		Joystick1Button17,
		Joystick1Button18,
		Joystick1Button19,

		Joystick2Button0,
		Joystick2Button1,
		Joystick2Button2,
		Joystick2Button3,
		Joystick2Button4,
		Joystick2Button5,
		Joystick2Button6,
		Joystick2Button7,
		Joystick2Button8,
		Joystick2Button9,
		Joystick2Button10,
		Joystick2Button11,
		Joystick2Button12,
		Joystick2Button13,
		Joystick2Button14,
		Joystick2Button15,
		Joystick2Button16,
		Joystick2Button17,
		Joystick2Button18,
		Joystick2Button19,

		Joystick3Button0,
		Joystick3Button1,
		Joystick3Button2,
		Joystick3Button3,
		Joystick3Button4,
		Joystick3Button5,
		Joystick3Button6,
		Joystick3Button7,
		Joystick3Button8,
		Joystick3Button9,
		Joystick3Button10,
		Joystick3Button11,
		Joystick3Button12,
		Joystick3Button13,
		Joystick3Button14,
		Joystick3Button15,
		Joystick3Button16,
		Joystick3Button17,
		Joystick3Button18,
		Joystick3Button19,

		Joystick4Button0,
		Joystick4Button1,
		Joystick4Button2,
		Joystick4Button3,
		Joystick4Button4,
		Joystick4Button5,
		Joystick4Button6,
		Joystick4Button7,
		Joystick4Button8,
		Joystick4Button9,
		Joystick4Button10,
		Joystick4Button11,
		Joystick4Button12,
		Joystick4Button13,
		Joystick4Button14,
		Joystick4Button15,
		Joystick4Button16,
		Joystick4Button17,
		Joystick4Button18,
		Joystick4Button19,

		Joystick5Button0,
		Joystick5Button1,
		Joystick5Button2,
		Joystick5Button3,
		Joystick5Button4,
		Joystick5Button5,
		Joystick5Button6,
		Joystick5Button7,
		Joystick5Button8,
		Joystick5Button9,
		Joystick5Button10,
		Joystick5Button11,
		Joystick5Button12,
		Joystick5Button13,
		Joystick5Button14,
		Joystick5Button15,
		Joystick5Button16,
		Joystick5Button17,
		Joystick5Button18,
		Joystick5Button19,

		Joystick6Button0,
		Joystick6Button1,
		Joystick6Button2,
		Joystick6Button3,
		Joystick6Button4,
		Joystick6Button5,
		Joystick6Button6,
		Joystick6Button7,
		Joystick6Button8,
		Joystick6Button9,
		Joystick6Button10,
		Joystick6Button11,
		Joystick6Button12,
		Joystick6Button13,
		Joystick6Button14,
		Joystick6Button15,
		Joystick6Button16,
		Joystick6Button17,
		Joystick6Button18,
		Joystick6Button19,

		Joystick7Button0,
		Joystick7Button1,
		Joystick7Button2,
		Joystick7Button3,
		Joystick7Button4,
		Joystick7Button5,
		Joystick7Button6,
		Joystick7Button7,
		Joystick7Button8,
		Joystick7Button9,
		Joystick7Button10,
		Joystick7Button11,
		Joystick7Button12,
		Joystick7Button13,
		Joystick7Button14,
		Joystick7Button15,
		Joystick7Button16,
		Joystick7Button17,
		Joystick7Button18,
		Joystick7Button19,

		Joystick8Button0,
		Joystick8Button1,
		Joystick8Button2,
		Joystick8Button3,
		Joystick8Button4,
		Joystick8Button5,
		Joystick8Button6,
		Joystick8Button7,
		Joystick8Button8,
		Joystick8Button9,
		Joystick8Button10,
		Joystick8Button11,
		Joystick8Button12,
		Joystick8Button13,
		Joystick8Button14,
		Joystick8Button15,
		Joystick8Button16,
		Joystick8Button17,
		Joystick8Button18,
		Joystick8Button19,

		Joystick9Button0,
		Joystick9Button1,
		Joystick9Button2,
		Joystick9Button3,
		Joystick9Button4,
		Joystick9Button5,
		Joystick9Button6,
		Joystick9Button7,
		Joystick9Button8,
		Joystick9Button9,
		Joystick9Button10,
		Joystick9Button11,
		Joystick9Button12,
		Joystick9Button13,
		Joystick9Button14,
		Joystick9Button15,
		Joystick9Button16,
		Joystick9Button17,
		Joystick9Button18,
		Joystick9Button19,

		Joystick10Button0,
		Joystick10Button1,
		Joystick10Button2,
		Joystick10Button3,
		Joystick10Button4,
		Joystick10Button5,
		Joystick10Button6,
		Joystick10Button7,
		Joystick10Button8,
		Joystick10Button9,
		Joystick10Button10,
		Joystick10Button11,
		Joystick10Button12,
		Joystick10Button13,
		Joystick10Button14,
		Joystick10Button15,
		Joystick10Button16,
		Joystick10Button17,
		Joystick10Button18,
		Joystick10Button19,

		Joystick11Button0,
		Joystick11Button1,
		Joystick11Button2,
		Joystick11Button3,
		Joystick11Button4,
		Joystick11Button5,
		Joystick11Button6,
		Joystick11Button7,
		Joystick11Button8,
		Joystick11Button9,
		Joystick11Button10,
		Joystick11Button11,
		Joystick11Button12,
		Joystick11Button13,
		Joystick11Button14,
		Joystick11Button15,
		Joystick11Button16,
		Joystick11Button17,
		Joystick11Button18,
		Joystick11Button19,

	}

	public enum InputDeviceType {
		Keyboard,
		GamepadButton,
		GamepadAxis,
		Mouse,
		Virtual,
		XR
	}

	public enum OSFamily {
		Other,
		MacOSX,
		Windows,
		Linux,
		Android,
		IOS,
		PS4,
		PSVita,
		XboxOne,
		Switch
	}

	public enum InputDeviceSlot {
		gamepad1 = 1,
		gamepad2 = 2,
		gamepad3 = 3,
		gamepad4 = 4,
		gamepad5 = 5,
		gamepad6 = 6,
		gamepad7 = 7,
		gamepad8 = 8,
		gamepad9 = 9,
		gamepad10 = 10,
		gamepad11 = 11,
		gamepad12 = 12,
		gamepad13 = 13,
		gamepad14 = 14,
		gamepad15 = 15,
		gamepad16 = 16,
		keyboardAndMouse = 17,
		keyboard = 18,
		mouse = 19,
		virtual1 = 20,
		any = 0,
	}

	public enum CommonGamepadInputs {
		NOBUTTON = 0,
		A = 1,
		B = 2,
		X = 3,
		Y = 4,
		LB = 5,
		RB = 6,
		LT = 7,
		RT = 8,
		L3 = 9,
		R3 = 10,
		DPAD_LEFT = 11,
		DPAD_RIGHT = 12,
		DPAD_UP = 13,
		DPAD_DOWN = 14,
		LSTICK_LEFT = 15,
		LSTICK_RIGHT = 16,
		LSTICK_UP = 17,
		LSTICK_DOWN = 18,
		RSTICK_LEFT = 19,
		RSTICK_RIGHT = 20,
		RSTICK_UP = 21,
		RSTICK_DOWN = 22,
		START = 23,
		BACK = 24,//AKA select/menu/whatever
		HOME = 25,//AKA system
	}

	public enum CommonXRInputs {
		NOBUTTON = 0,
		BUTTON1_L = 1,
		BUTTON1_R = 2,
		BUTTON2_L = 3,
		BUTTON2_R = 4,

		STICK_PRESS_L = 5,
		STICK_PRESS_R = 6,
		STICK_TOUCH_L = 7,
		STICK_TOUCH_R = 8,

		STICK_LEFT_L = 9,
		STICK_RIGHT_L = 10,
		STICK_UP_L = 11,
		STICK_DOWN_L = 12,
		STICK_LEFT_R = 13,
		STICK_RIGHT_R = 14,
		STICK_UP_R = 15,
		STICK_DOWN_R = 16,

		TRIGGER_L = 17,
		TRIGGER_R = 18,

		GRIP_L = 19,
		GRIP_R = 20,

		STICK_X_L = 21,
		STICK_Y_L = 22,
		STICK_X_R = 23,
		STICK_Y_R = 24,

		FINGER_INDEX_L = 25,
		FINGER_INDEX_R = 26,
		FINGER_MIDDLE_L = 27,
		FINGER_MIDDLE_R = 28,
		FINGER_RING_L = 29,
		FINGER_RING_R = 30,
		FINGER_PINKY_L = 31,
		FINGER_PINKY_R = 32,
	}

	public static class SInputEnums {
		public static readonly KeyboardInputType[] KeyboardInputTypes = (KeyboardInputType[]) Enum.GetValues(typeof(KeyboardInputType));
		public static readonly MouseInputType[] MouseInputTypes = (MouseInputType[]) Enum.GetValues(typeof(MouseInputType));

		private static readonly string[] AxisStrings = ((Func<string[]>) (() => {
			var strs = new string[Sinput.MAXCONNECTEDGAMEPADS * Sinput.MAXAXISPERGAMEPAD];
			for (int j = 0; j < Sinput.MAXCONNECTEDGAMEPADS; j++) {
				for (int a = 0; a < Sinput.MAXAXISPERGAMEPAD; a++) {
					strs[j * Sinput.MAXAXISPERGAMEPAD + a] = string.Format("J_{0}_{1}", j + 1, a + 1);
				}
			}
			return strs;
		}))();

		/// <summary>
		/// Get the KeyCode that corresponds to a specific gamepad number and button
		/// </summary>
		/// <param name="slotIndex">0-index based (starts from 0)</param>
		/// <param name="gamepadButtonNumber">0-index based (starts from 0)</param>
		public static KeyCode GetGamepadKeyCode(int slotIndex, int gamepadButtonNumber) {
			const UnityGamepadKeyCode FirstButton = UnityGamepadKeyCode.Joystick1Button0;
			const int ButtonsPerGamepad = UnityGamepadKeyCode.Joystick2Button0 - UnityGamepadKeyCode.Joystick1Button0;

			if (slotIndex < 0 || slotIndex >= 16 || gamepadButtonNumber < 0 || gamepadButtonNumber >= 20) return KeyCode.None;
			return (KeyCode) FirstButton + slotIndex * ButtonsPerGamepad + gamepadButtonNumber;
		}

		/// <summary>
		/// Get the KeyCode that corresponds to mouse buttons
		/// </summary>
		public static KeyCode GetMouseButton(MouseInputType mouseInputType) {
			if (mouseInputType >= MouseInputType.Mouse0 && mouseInputType <= MouseInputType.Mouse6)
				return KeyCode.Mouse0 + (mouseInputType - MouseInputType.Mouse0);
			return KeyCode.None;
		}

		/// <summary>
		/// Get a string that can be used with <see cref="Input.GetAxisRaw(string)"/>
		/// </summary>
		/// <param name="joystick">0-index based (starts from 0)</param>
		/// <param name="axis">0-index based (starts from 0)</param>
		public static string GetAxisString(int joystick, int axis) {
			return AxisStrings[joystick * Sinput.MAXAXISPERGAMEPAD + axis];
		}
	}
}
