using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SinputSystems {
	public static class CommonGamepadMappings {
		private static readonly OSFamily ThisOS = GetThisOS();
		public static CommonMapping[] controllerMappings { get; private set; }

		private static OSFamily GetThisOS() {
			switch (Application.platform) {
				case RuntimePlatform.OSXEditor:
				case RuntimePlatform.OSXPlayer: return OSFamily.MacOSX;
				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.WindowsPlayer: return OSFamily.Windows;
				case RuntimePlatform.LinuxEditor:
				case RuntimePlatform.LinuxPlayer: return OSFamily.Linux;
				case RuntimePlatform.Android: return OSFamily.Android;
				case RuntimePlatform.IPhonePlayer: return OSFamily.IOS;
				case RuntimePlatform.PS4: return OSFamily.PS4;
				case RuntimePlatform.PSP2: return OSFamily.PSVita;
				case RuntimePlatform.XboxOne: return OSFamily.XboxOne;
				case RuntimePlatform.Switch: return OSFamily.Switch;
			}
			return OSFamily.Other;
		}

		// Must be called when Sinput is first called and after gamepads are plugged in or removed
		public static void ReloadCommonMaps() {
			Debug.Log("(Re?)Loading common mappings for gamepads");

			CommonMapping[] commonMappingAssets = Resources.LoadAll<CommonMapping>("");
			string[] gamepads = Sinput.gamepads;
			controllerMappings = new CommonMapping[gamepads.Length];

			CommonMapping defaultMapping = null;
			foreach (var commonMapping in commonMappingAssets) {
				if (commonMapping.name == "DefaultMapping") {
					defaultMapping = commonMapping;
					break;
				}
			}

			for (int g = 0; g < gamepads.Length; g++) {
				// For each common mapping, find which gamepad slots it applies to. Inputs built from common mappings will only check slots which match
				foreach (var commonMapping in commonMappingAssets) {
					if ((commonMapping.operatingSystem & ThisOS) != 0 && commonMapping.names.Any(n => n.ToUpper() == gamepads[g])) {
						controllerMappings[g] = commonMapping;
						goto EndSearchingCommonMapping;
					}
				}

				for (int k = 0; true; k++) {
					bool indexFound = false;
					foreach (var commonMapping in commonMappingAssets) {
						if ((commonMapping.operatingSystem & ThisOS) != 0) {
							if (k < commonMapping.partialNames.Count) {
								indexFound = true;
								if (gamepads[g].Contains(commonMapping.partialNames[k].ToUpper())) {
									controllerMappings[g] = commonMapping;
									goto EndSearchingCommonMapping;
								}
							}
						}
					}
					if (!indexFound) break;
				}

				if (null == controllerMappings[g]) {
					Debug.Log("Controller [" + gamepads[g] + "] has no matching mapping. Using default (may not match at all. Default's default is xbox mapping).");
					if (null != defaultMapping) {
						controllerMappings[g] = defaultMapping;
					}
					else {
						Debug.Log("Default common mapping for gamepads not found (DefaultMapping).");
						continue;
					}
				}

				EndSearchingCommonMapping:
				Debug.Log("Controller [" + gamepads[g] + "] assigned to mapping [" + controllerMappings[g].name + "]");
			}
		}

		public static List<DeviceInput> GetApplicableMaps(CommonGamepadInputs commonInputType) {
			//builds input mapping of type t for all known connected gamepads


			List<DeviceInput> applicableInputs = new List<DeviceInput>();

			for (int g = 0; g < controllerMappings.Length; g++) {
				if (controllerMappings[g] == null) continue;

				//if (commonMappings[i].isXRdevice) Debug.Log("Found XR device");

				//add any applicable button mappings
				for (int k = 0; k < controllerMappings[g].buttons.Count; k++) {
					bool addthis = false;
					if (controllerMappings[g].buttons[k].buttonType == commonInputType) addthis = true;
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
					if (controllerMappings[g].axis[k].buttonType == commonInputType) addthis = true;
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
