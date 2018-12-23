using System;
using UnityEngine;

namespace SinputSystems.Examples {
	public class SinputDebug : MonoBehaviour {
		public ControlScheme controlScheme;

		private void Awake() {
			Sinput.LoadControlScheme(controlScheme, true);
		}

		private void OnGUI() {
			var gamepads = Sinput.gamepads;
			GUILayout.Label("Gamepad count: " + gamepads.Length);
			if (Sinput.currentControlScheme != null) {
				GUILayout.Label("Current control scheme name: " + Sinput.currentControlScheme.name);

				GUILayout.BeginHorizontal();
				for (int i = 0; i < gamepads.Length; i++) {
					GUILayout.BeginVertical("box");
					GUILayout.Label("Gamepad index: " + i);
					GUILayout.Label("Gamepad name: " + gamepads[i]);
					if (i < CommonGamepadMappings.controllerMappings.Length && null != CommonGamepadMappings.controllerMappings[i]) {
						GUILayout.Label("Gamepad layout: " + CommonGamepadMappings.controllerMappings[i].name);
					}
					else {
						GUILayout.Label("NO Gamepad layout");
					}

					GUILayout.BeginHorizontal();
					GUILayout.BeginVertical("box");
					GUILayout.Label("Mapped readings");
					foreach (var control in Sinput.controls) {
						Label(control.name, control.GetAxisState((InputDeviceSlot) (i + 1), out var _));
					}
					foreach (var smartControl in Sinput.smartControls) {
						Label(smartControl.name, smartControl.GetAxisState((InputDeviceSlot) (i + 1), out var _));
					}
					GUILayout.EndVertical();

					GUILayout.BeginVertical("box");
					GUILayout.Label("Raw Button readings");
					for (int j = 0; j < Sinput.MAX_BUTTONS_PER_GAMEPAD; j++) {
						Label("Button " + j, Input.GetKey(SInputEnums.GetGamepadKeyCode(i, j)) ? 1 : 0);
					}
					GUILayout.EndVertical();

					GUILayout.BeginVertical("box");
					GUILayout.Label("Raw Axis readings");
					for (int j = 0; j < Sinput.MAX_AXIS_PER_GAMEPAD; j++) {
						Label("Axis " + j, Input.GetAxisRaw(SInputEnums.GetGamepadAxisString(i, j)));
					}
					GUILayout.EndVertical();
					GUILayout.EndHorizontal();

					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}
			else {
				GUILayout.Label("No Current control scheme loaded");
			}
		}

		private static void Label(string name, float value) {
			var originalColor = GUI.color;
			GUI.color = Color.Lerp(Color.white, Color.green, Math.Abs(value));
			GUILayout.Label(name + ": " + value.ToString("0.000"));
			GUI.color = originalColor;
		}
	}
}