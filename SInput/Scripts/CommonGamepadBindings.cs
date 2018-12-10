using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SinputSystems {
	public static class CommonGamepadMappings {

		private static CommonMapping[] controllerMappings;

		public static void ReloadCommonMaps() {
			// Must be called when gamepads are plugged in or removed, also when Sinput is first called

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
			string[] gamepads = Sinput.gamepads;
			controllerMappings = new CommonMapping[gamepads.Length];

			for (int g = 0; g < gamepads.Length; g++) {
				// For each common mapping, find which gamepad slots it applies to. Inputs built from common mappings will only check slots which match
				string gamepadName = gamepads[g];

				foreach (var commonMapping in commonMappingAssets) {
					if (commonMapping.os == thisOS && commonMapping.names.Any(n => n.ToUpper() == gamepadName)) {
						controllerMappings[g] = commonMapping;
						goto EndSearchingCommonMapping;
					}
				}
				
				for (int k = 0; true; k++) {
					bool indexFound = false;
					foreach (var commonMapping in commonMappingAssets) {
						if (commonMapping.os == thisOS) {
							if (k < commonMapping.partialNames.Count) {
								indexFound = true;
								if (gamepadName.Contains(commonMapping.partialNames[k].ToUpper())) {
									controllerMappings[g] = commonMapping;
									goto EndSearchingCommonMapping;
								}
							}
						}
					}
					if (!indexFound) break;
				}

				EndSearchingCommonMapping:
				if (null != controllerMappings[g]) {
					Debug.Log("Controller [" + gamepadName + "] was assigned to mapping [" + controllerMappings[g].name + "]");
				}
			}
		}

		public static List<DeviceInput> GetApplicableMaps(CommonGamepadInputs commonInputType, CommonXRInputs commonXRInputType) {
			//builds input mapping of type t for all known connected gamepads


			List<DeviceInput> applicableInputs = new List<DeviceInput>();

			for (int g = 0; g < controllerMappings.Length; g++) {
				if (controllerMappings[g] == null) continue;

				//if (commonMappings[i].isXRdevice) Debug.Log("Found XR device");

				//add any applicable button mappings
				for (int k = 0; k < controllerMappings[g].buttons.Count; k++) {
					bool addthis = false;
					if (!controllerMappings[g].isXRdevice && controllerMappings[g].buttons[k].buttonType != CommonGamepadInputs.NOBUTTON) {
						if (controllerMappings[g].buttons[k].buttonType == commonInputType) addthis = true;
					}
					if (controllerMappings[g].isXRdevice && controllerMappings[g].buttons[k].vrButtonType != CommonXRInputs.NOBUTTON) {
						//Debug.Log("Adding XR button from common mapping");
						if (controllerMappings[g].buttons[k].vrButtonType == commonXRInputType) addthis = true;
					}
					if (addthis) {
						//add this button input
						DeviceInput newInput = new DeviceInput(InputDeviceType.GamepadButton);
						newInput.gamepadButtonNumber = controllerMappings[g].buttons[k].buttonNumber;
						newInput.commonMappingType = commonInputType;
						newInput.displayName = controllerMappings[g].buttons[k].displayName;
						newInput.displaySprite = controllerMappings[g].buttons[k].displaySprite;

						newInput.allowedSlot = (InputDeviceSlot) (g + 1);

						applicableInputs.Add(newInput);
					}
				}
				//add any applicable axis bingings
				for (int k = 0; k < controllerMappings[g].axis.Count; k++) {
					bool addthis = false;
					if (!controllerMappings[g].isXRdevice && controllerMappings[g].axis[k].buttonType != CommonGamepadInputs.NOBUTTON) {
						if (controllerMappings[g].axis[k].buttonType == commonInputType) addthis = true;
					}
					if (controllerMappings[g].isXRdevice && controllerMappings[g].axis[k].vrButtonType != CommonXRInputs.NOBUTTON) {
						//Debug.Log("Adding XR Axis from common mapping");
						if (controllerMappings[g].axis[k].vrButtonType == commonXRInputType) addthis = true;
					}
					if (addthis) {
						//add this axis input
						DeviceInput newInput = new DeviceInput(InputDeviceType.GamepadAxis);
						newInput.gamepadAxisNumber = controllerMappings[g].axis[k].axisNumber;
						newInput.commonMappingType = commonInputType;
						newInput.displayName = controllerMappings[g].axis[k].displayName;
						newInput.displaySprite = controllerMappings[g].axis[k].displaySprite;
						newInput.invertAxis = controllerMappings[g].axis[k].invert;
						newInput.clampAxis = controllerMappings[g].axis[k].clamp;
						newInput.deadZone = controllerMappings[g].axis[k].deadZone;
						newInput.axisButtoncompareVal = controllerMappings[g].axis[k].compareVal;
						newInput.defaultAxisValue = controllerMappings[g].axis[k].defaultVal;

						newInput.allowedSlot = (InputDeviceSlot) (g + 1);

						if (controllerMappings[g].axis[k].rescaleAxis) {
							newInput.rescaleAxis = true;
							newInput.rescaleAxisMin = controllerMappings[g].axis[k].rescaleAxisMin;
							newInput.rescaleAxisMax = controllerMappings[g].axis[k].rescaleAxisMax;
						}

						applicableInputs.Add(newInput);
					}
				}
			}

			return applicableInputs;
		}
	}
}
