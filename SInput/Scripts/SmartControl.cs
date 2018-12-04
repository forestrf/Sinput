using System;
using System.Collections.Generic;
using UnityEngine;

namespace SinputSystems {
	public class SmartControl : BaseControl {
		//InputControls combine various inputs, and can behave as buttons or 1-dimensional axis
		//SmartControls combine various InputControls or other SmartControls, and can have a bunch of extra behaviour like normal InputManager smoothing
		//These won't be exposed to players when rebinding because they are built on other controls (and it'd be a headache to present anyway)

		public string name;
		public string displayName;

		//control constructor
		public SmartControl(string controlName) {
			name = controlName;

			values = new float[Sinput.totalPossibleDeviceSlots];
			valuePrefersDeltaUse = new bool[Sinput.totalPossibleDeviceSlots];
		}

		//values for each slot's input
		private float[] values;
		private bool[] valuePrefersDeltaUse;

		public string positiveControl;
		public BaseControl positiveControlRef { get; private set; }
		public string negativeControl;
		public BaseControl negativeControlRef { get; private set; }


		public float deadzone = 0.001f; //clip values less than this

		//public float scale =1f;
		public float[] scales;

		public bool[] inversion;

		public void Init() {
			//prepare to record values for all gamepads AND keyboard & mouse inputs
			//int possibleInputDeviceCount = System.Enum.GetValues(typeof(InputDeviceSlot)).Length;
			ResetAllValues(InputDeviceSlot.any);

			positiveControlRef = Sinput.GetControlByName(positiveControl);
			negativeControlRef = Sinput.GetControlByName(negativeControl);
		}

		public void ResetAllValues(InputDeviceSlot slot) {
			//set all values for this control to 0
			if (slot == InputDeviceSlot.any) {
				for (int i = 0; i < values.Length; i++) {
					values[i] = 0f;
					valuePrefersDeltaUse[i] = true;
				}
			}
			else {
				values[(int) slot] = 0f;
				valuePrefersDeltaUse[(int) slot] = true;
			}
		}

		private int lastUpdateFrame = -10;
		public void Update() {
			if (Time.frameCount == lastUpdateFrame) return;
			lastUpdateFrame = Time.frameCount;


			for (int slot = 0; slot < values.Length; slot++) {

				bool positivePrefersDelta, negativePrefersDelta;
				values[slot] = Sinput.AxisCheck(positiveControlRef, out positivePrefersDelta, (InputDeviceSlot) slot) - Sinput.AxisCheck(negativeControlRef, out negativePrefersDelta, (InputDeviceSlot) slot);

				if (inversion[slot]) values[slot] *= -1f;

				//Is the rawvalue this frame is from a framerate independent input like a mouse movement? if so, we don't want it smoothed and wanna force getAxis checks to return raw
				valuePrefersDeltaUse[slot] = positivePrefersDelta && negativePrefersDelta;
			}
		}

		//return current value
		public float GetAxisState(InputDeviceSlot slot = InputDeviceSlot.any) { return GetAxisState(slot, out var _); }
		public override float GetAxisState(InputDeviceSlot slot, out bool prefersDeltaUse) {
			prefersDeltaUse = true; // Defaults to true, but doesn't matter because when default, the value returned is 0
			if ((int) slot >= values.Length) return 0f; //not a slot we have any input info for

			prefersDeltaUse = valuePrefersDeltaUse[(int) slot];

			//if this input is checking a framerate independent input like a mouse, return the raw value regardless of getRawValue
			if (!prefersDeltaUse) return values[(int) slot] * scales[(int) slot];

			//deadzone clipping
			if (Math.Abs(values[(int) slot]) < deadzone) return 0f;

			//return the raw value
			return values[(int) slot] * scales[(int) slot];
		}

		//button check
		public override bool GetButtonState(ButtonAction bAction, InputDeviceSlot slot, bool getRawValue) {
			return Sinput.ButtonCheck(positiveControlRef, slot, bAction, getRawValue) || Sinput.ButtonCheck(negativeControlRef, slot, bAction, getRawValue);
		}

		public override void FillInputs(List<DeviceInput> inputs, InputDeviceSlot slot) {
			positiveControlRef.FillInputs(inputs, slot);
			negativeControlRef.FillInputs(inputs, slot);
		}
	}
}
