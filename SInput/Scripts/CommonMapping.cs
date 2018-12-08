﻿using System.Collections.Generic;
using UnityEngine;
//using SinputSystems;

namespace SinputSystems {
	[CreateAssetMenu(fileName = "New Common Mapping", menuName = "Sinput/Common Mapping", order = 1)]
	public class CommonMapping : ScriptableObject {

		public List<string> names = new List<string>();//names of gamepads that this mapping can apply to
		public List<string> partialNames = new List<string>();//if an exact name match isn't found for a pad, use this mapping if it has a partial match with this

		public OSFamily os = OSFamily.Windows;

		public List<GamepadButtonInput> buttons = new List<GamepadButtonInput>();
		public List<GamepadAxisInput> axis = new List<GamepadAxisInput>();


		public bool isXRdevice = false;//use commongamepadinputs or commonvrinputs


		[System.Serializable]
		public struct GamepadButtonInput {
			public CommonGamepadInputs buttonType;
			public CommonXRInputs vrButtonType;
			public int buttonNumber;
			public string displayName;
			public Sprite displaySprite;
		}

		[System.Serializable]
		public struct GamepadAxisInput {
			public CommonGamepadInputs buttonType;
			public CommonXRInputs vrButtonType;
			public int axisNumber;
			public bool invert;
			public bool clamp; // Applied AFTER invert, to keep input result between 0 and 1
			public float deadZone; // Applied last

			// For using the axis as a button
			public bool compareGreater; // True is ([axisVal]>compareVal), false is ([axisVal]<compareVal)
			public float compareVal; // How var does have to go to count as "pressed" as a button

			public bool rescaleAxis;
			public float rescaleAxisMin;
			public float rescaleAxisMax;

			public float defaultVal; // All GetAxis() checks will return default value until a measured change occurs, since readings before then can be wrong


			public string displayName;
			public Sprite displaySprite;
		}
	}
}