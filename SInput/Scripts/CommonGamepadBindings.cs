using System.Collections.Generic;
using UnityEngine;

namespace SinputSystems {
	public static class CommonGamepadMappings {

		private static List<CommonMapping> commonMappings;
		private static List<InputDeviceSlot>[] mappingSlots;

		public static void ReloadCommonMaps() {
			//called when gamepads are plugged in or removed, also when Sinput is first called

			//Debug.Log("Loading common mapping");

			OSFamily thisOS;
			switch (Application.platform) {
				case RuntimePlatform.OSXEditor: thisOS = OSFamily.MacOSX; break;
				case RuntimePlatform.OSXPlayer: thisOS = OSFamily.MacOSX; break;
				case RuntimePlatform.WindowsEditor: thisOS = OSFamily.Windows; break;
				case RuntimePlatform.WindowsPlayer: thisOS = OSFamily.Windows; break;
				case RuntimePlatform.LinuxEditor: thisOS = OSFamily.Linux; break;
				case RuntimePlatform.LinuxPlayer: thisOS = OSFamily.Linux; break;
				case RuntimePlatform.Android: thisOS = OSFamily.Android; break;
				case RuntimePlatform.IPhonePlayer: thisOS = OSFamily.IOS; break;
				case RuntimePlatform.PS4: thisOS = OSFamily.PS4; break;
				case RuntimePlatform.PSP2: thisOS = OSFamily.PSVita; break;
				case RuntimePlatform.XboxOne: thisOS = OSFamily.XboxOne; break;
				case RuntimePlatform.Switch: thisOS = OSFamily.Switch; break;
				default: thisOS = OSFamily.Other; break;
			}

			CommonMapping[] commonMappingAssets = Resources.LoadAll<CommonMapping>("");
			commonMappings = new List<CommonMapping>();
			string[] gamepads = Sinput.gamepads;
			for (int i = 0; i < commonMappingAssets.Length; i++) {
				//Debug.Log("HELLOOOOO");
				//if (commonMappingAssets[i].isXRdevice) Debug.Log("XR deviiiiiice");

				if (commonMappingAssets[i].os == thisOS) {
					bool gamepadConnected = false;
					bool partialMatch = false;
					for (int k = 0; k < commonMappingAssets[i].names.Count; k++) {
						for (int g = 0; g < gamepads.Length; g++) {
							if (commonMappingAssets[i].names[k].ToUpper() == gamepads[g]) gamepadConnected = true;
						}
					}

					for (int k = 0; k < commonMappingAssets[i].partialNames.Count; k++) {
						for (int g = 0; g < gamepads.Length; g++) {
							if (gamepads[g].Contains(commonMappingAssets[i].partialNames[k].ToUpper())) partialMatch = true;
						}
					}

					if (gamepadConnected) commonMappings.Add(commonMappingAssets[i]);
					if (partialMatch && !gamepadConnected) commonMappings.Add(commonMappingAssets[i]);
				}
			}



			//for each common mapping, find which gamepad slots it applies to
			//inputs built from common mappings will only check slots which match
			mappingSlots = new List<InputDeviceSlot>[commonMappings.Count];
			for (int i = 0; i < mappingSlots.Length; i++) {
				mappingSlots[i] = new List<InputDeviceSlot>();
			}
			//string[] gamepads = Sinput.GetGamepads();
			for (int i = 0; i < commonMappings.Count;) {
				for (int k = 0; k < commonMappings[i].names.Count; k++) {
					for (int g = 0; g < gamepads.Length; g++) {
						if (gamepads[g] == commonMappings[i].names[k].ToUpper()) {
							mappingSlots[i].Add((InputDeviceSlot) (g + 1));
							goto NextCommonMapping;
						}
					}
				}
				// If we reach this, this mapping still needs a slot
				for (int g = 0; g < gamepads.Length; g++) {
					// Check for partial name matches with this gamepad slot
					for (int k = 0; k < commonMappings[i].partialNames.Count; k++) {
						if (gamepads[g].Contains(commonMappings[i].partialNames[k].ToUpper())) {
							mappingSlots[i].Add((InputDeviceSlot) (g + 1));
							goto NextCommonMapping;
						}
					}
				}

				NextCommonMapping:
				i++;
			}
		}

		public static List<DeviceInput> GetApplicableMaps(CommonGamepadInputs commonInputType, CommonXRInputs commonXRInputType) {
			//builds input mapping of type t for all known connected gamepads


			List<DeviceInput> applicableInputs = new List<DeviceInput>();

			for (int i = 0; i < commonMappings.Count; i++) {

				//if (commonMappings[i].isXRdevice) Debug.Log("Found XR device");

				//add any applicable button mappings
				for (int k = 0; k < commonMappings[i].buttons.Count; k++) {
					bool addthis = false;
					if (!commonMappings[i].isXRdevice && commonMappings[i].buttons[k].buttonType != CommonGamepadInputs.NOBUTTON) {
						if (commonMappings[i].buttons[k].buttonType == commonInputType) addthis = true;
					}
					if (commonMappings[i].isXRdevice && commonMappings[i].buttons[k].vrButtonType != CommonXRInputs.NOBUTTON) {
						//Debug.Log("Adding XR button from common mapping");
						if (commonMappings[i].buttons[k].vrButtonType == commonXRInputType) addthis = true;
					}
					if (addthis) {
						//add this button input
						DeviceInput newInput = new DeviceInput(InputDeviceType.GamepadButton);
						newInput.gamepadButtonNumber = commonMappings[i].buttons[k].buttonNumber;
						newInput.commonMappingType = commonInputType;
						newInput.displayName = commonMappings[i].buttons[k].displayName;
						newInput.displaySprite = commonMappings[i].buttons[k].displaySprite;

						newInput.allowedSlots.AddRange(mappingSlots[i]);

						applicableInputs.Add(newInput);
					}
				}
				//add any applicable axis bingings
				for (int k = 0; k < commonMappings[i].axis.Count; k++) {
					bool addthis = false;
					if (!commonMappings[i].isXRdevice && commonMappings[i].axis[k].buttonType != CommonGamepadInputs.NOBUTTON) {
						if (commonMappings[i].axis[k].buttonType == commonInputType) addthis = true;
					}
					if (commonMappings[i].isXRdevice && commonMappings[i].axis[k].vrButtonType != CommonXRInputs.NOBUTTON) {
						//Debug.Log("Adding XR Axis from common mapping");
						if (commonMappings[i].axis[k].vrButtonType == commonXRInputType) addthis = true;
					}
					if (addthis) {
						//add this axis input
						DeviceInput newInput = new DeviceInput(InputDeviceType.GamepadAxis);
						newInput.gamepadAxisNumber = commonMappings[i].axis[k].axisNumber;
						newInput.commonMappingType = commonInputType;
						newInput.displayName = commonMappings[i].axis[k].displayName;
						newInput.displaySprite = commonMappings[i].axis[k].displaySprite;
						newInput.invertAxis = commonMappings[i].axis[k].invert;
						newInput.clampAxis = commonMappings[i].axis[k].clamp;
						newInput.deadZone = commonMappings[i].axis[k].deadZone;
						newInput.axisButtoncompareVal = commonMappings[i].axis[k].compareVal;
						newInput.defaultAxisValue = commonMappings[i].axis[k].defaultVal;

						newInput.allowedSlots.AddRange(mappingSlots[i]);

						if (commonMappings[i].axis[k].rescaleAxis) {
							newInput.rescaleAxis = true;
							newInput.rescaleAxisMin = commonMappings[i].axis[k].rescaleAxisMin;
							newInput.rescaleAxisMax = commonMappings[i].axis[k].rescaleAxisMax;
						}

						applicableInputs.Add(newInput);
					}
				}

			}

			return applicableInputs;
		}
	}
}
