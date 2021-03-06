using System;
using UnityEngine;

namespace SinputSystems{

	public class SmartControl : ISerializationCallbackReceiver {
		//InputControls combine various inputs, and can behave as buttons or 1-dimensional axis
		//SmartControls combine various InputControls or other SmartControls, and can have a bunch of extra behaviour like normal InputManager smoothing
		//These won't be exposed to players when rebinding because they are built on other controls (and it'd be a headache to present anyway)

		public string name;
		public int nameHashed { get; private set; }
		public string displayName;

		//control constructor
		public SmartControl(string controlName){
			name = controlName;
			Hash();
		}

		public void OnBeforeSerialize() { }
		public void OnAfterDeserialize() { Hash(); }

		public void Hash() {
			nameHashed = Animator.StringToHash(name);
			positiveControlHashed = Animator.StringToHash(positiveControl);
			negativeControlHashed = Animator.StringToHash(negativeControl);
		}

		//values for each slot's input
		private float[] rawValues;
		private float[] controlValues;
		private bool[] valuePrefersDeltaUse;

		public string positiveControl;
		public int positiveControlHashed { get; private set; }
		public string negativeControl;
		public int negativeControlHashed { get; private set; }


		public float deadzone=0.001f; //clip values less than this

		public float gravity=3; //how quickly the value shifts to zero
		public float speed=3; //how quickly does the value shift towards it's target
		public bool snap=false;//if value is negative and input is positive, snaps to zero

		//public float scale =1f;
		public float[] scales;

		public bool[] inversion;

		public void Init(){
			//prepare to record values for all gamepads AND keyboard & mouse inputs
			//int possibleInputDeviceCount = System.Enum.GetValues(typeof(InputDeviceSlot)).Length;
			rawValues = new float[Sinput.totalPossibleDeviceSlots];
			controlValues = new float[Sinput.totalPossibleDeviceSlots];
			valuePrefersDeltaUse = new bool[Sinput.totalPossibleDeviceSlots];
			ResetAllValues(InputDeviceSlot.any);

			
		}

		public void ResetAllValues(InputDeviceSlot slot) {
			//set all values for this control to 0
			if (slot == InputDeviceSlot.any) {
				for (int i = 0; i < controlValues.Length; i++) {
					rawValues[i] = 0f;
					controlValues[i] = 0f;
					valuePrefersDeltaUse[i] = true;
				}
			} else {
				rawValues[(int)slot] = 0f;
				controlValues[(int)slot] = 0f;
				valuePrefersDeltaUse[(int)slot] = true;
			}
		}

		private int lastUpdateFrame = -10;
		public void Update(){
			if (Time.frameCount == lastUpdateFrame) return;
			lastUpdateFrame = Time.frameCount;

			
			for (int slot=0; slot<rawValues.Length; slot++){
				
				bool positivePrefersDelta, negativePrefersDelta;
				rawValues[slot] = Sinput.AxisCheck(positiveControlHashed, out positivePrefersDelta, (InputDeviceSlot) slot) - Sinput.AxisCheck(negativeControlHashed, out negativePrefersDelta, (InputDeviceSlot) slot);

				if (inversion[slot]) rawValues[slot] *= -1f;

				//Is the rawvalue this frame is from a framerate independent input like a mouse movement? if so, we don't want it smoothed and wanna force getAxis checks to return raw
				valuePrefersDeltaUse[slot] = positivePrefersDelta && negativePrefersDelta;
			}

			for (int slot=0; slot<controlValues.Length; slot++){
				if (!valuePrefersDeltaUse[slot]) {
					//we're forcing things to be unsmoothed for now, zero smoothed input now so when we stop smoothing, it doesn't seem weird
					controlValues[slot] = 0f;
				} else { 
					//shift to zero
					if (gravity > 0f) {
						if (rawValues[slot] == 0f || (rawValues[slot] < controlValues[slot] && controlValues[slot] > 0f) || (rawValues[slot] > controlValues[slot] && controlValues[slot] < 0f)) {
							if (controlValues[slot] > 0f) {
								controlValues[slot] -= gravity * Time.deltaTime;
								if (controlValues[slot] < 0f) controlValues[slot] = 0f;
								if (controlValues[slot] < rawValues[slot]) controlValues[slot] = rawValues[slot];
							} else if (controlValues[slot] < 0f) {
								controlValues[slot] += gravity * Time.deltaTime;
								if (controlValues[slot] > 0f) controlValues[slot] = 0f;
								if (controlValues[slot] > rawValues[slot]) controlValues[slot] = rawValues[slot];
							}
						}
					}

					//snapping
					if (snap) {
						if (rawValues[slot] > 0f && controlValues[slot] < 0f) controlValues[slot] = 0f;
						if (rawValues[slot] < 0f && controlValues[slot] > 0f) controlValues[slot] = 0f;
					}

					//move value towards target value
					if (rawValues[slot] < 0f) {
						if (controlValues[slot] > rawValues[slot]) {
							controlValues[slot] -= speed * Time.deltaTime;
							if (controlValues[slot] < rawValues[slot]) controlValues[slot] = rawValues[slot];
						}
					}
					if (rawValues[slot] > 0f) {
						if (controlValues[slot] < rawValues[slot]) {
							controlValues[slot] += speed * Time.deltaTime;
							if (controlValues[slot] > rawValues[slot]) controlValues[slot] = rawValues[slot];
						}
					}

					if (speed == 0f) controlValues[slot] = rawValues[slot];
				}
			}

		}

		//return current value
		public float GetValue(InputDeviceSlot slot = InputDeviceSlot.any, bool getRawValue = false) { return GetValue(slot, false, out var _); }
		public float GetValue(InputDeviceSlot slot, bool getRawValue, out bool prefersDeltaUse) {
			prefersDeltaUse = true; // Defaults to true, but doesn't matter because when default, the value returned is 0
			if ((int)slot>=controlValues.Length) return 0f; //not a slot we have any input info for

			prefersDeltaUse = valuePrefersDeltaUse[(int) slot];

			//if this input is checking a framerate independent input like a mouse, return the raw value regardless of getRawValue
			if (!prefersDeltaUse) return rawValues[(int)slot] * scales[(int)slot];

			//deadzone clipping
			if (Math.Abs(controlValues[(int)slot]) < deadzone) return 0f;

			if (getRawValue) {
				//return the raw value
				return rawValues[(int)slot] * scales[(int)slot];
			}

			//return the smoothed value
			return controlValues[(int)slot]*scales[(int)slot];
		}

		//button check
		public bool ButtonCheck(ButtonAction bAction){ return ButtonCheck(bAction, InputDeviceSlot.any); }
		public bool ButtonCheck(ButtonAction bAction, InputDeviceSlot slot){
			if (bAction == ButtonAction.DOWN && (Sinput.GetButtonDown(positiveControlHashed, slot) || Sinput.GetButtonDown(negativeControlHashed, slot))) return true;
			if (bAction == ButtonAction.HELD && (Sinput.GetButton(positiveControlHashed, slot)     || Sinput.GetButton(negativeControlHashed, slot)))     return true;
			if (bAction == ButtonAction.UP   && (Sinput.GetButtonUp(positiveControlHashed, slot)   || Sinput.GetButtonUp(negativeControlHashed, slot)))   return true;
			return false;
		}

	}

}
