using System;
using System.Collections.Generic;
using UnityEngine;

namespace SinputSystems {
	// If this class inherits from ScriptableObject, Instantiate of this easily makes a clone
	public class Control : BaseControl {
		//name of control
		public string name;

		//is this control a hold or a toggle type
		public bool isToggle = false;

		//list of inputs we will check when the control is polled
		public readonly List<DeviceInput> inputs = new List<DeviceInput>();
		public readonly List<CommonGamepadInputs> commonMappings = new List<CommonGamepadInputs>();

		public Control(string controlName) {
			name = controlName;
		}

		private readonly ControlState[] controlStates = ((Func<ControlState[]>) (() => {
			ControlState[] controlStates = new ControlState[Sinput.TotalPossibleDeviceSlots];
			for (int i = 0; i < controlStates.Length; i++) {
				controlStates[i] = new ControlState();
			}
			return controlStates;
		}))();

		//called no more than once a frame from Sinput.SinputUpdate
		public void Update() {
			//Update for devices that are probably connected
			for (int i = 1; i <= Sinput.connectedGamepads; i++) {
				UpdateControlState(controlStates[i], (InputDeviceSlot) i);
			}
			for (int i = (int) InputDeviceSlot.gamepad16 + 1; i < controlStates.Length; i++) {
				UpdateControlState(controlStates[i], (InputDeviceSlot) i);
			}

			UpdateAnyControlState();//checked other slots, now check the 'any' slot
		}

		void UpdateControlState(ControlState controlState, InputDeviceSlot slot) {
			var wasHeld = controlState.held;
			controlState.held = false;

			controlState.value = 0f;
			float controlStateValueAbs = 0f;
			controlState.valuePrefersDeltaUse = true;

			foreach (var input in inputs) {
				var v = input.AxisCheck(slot);
				var vAbs = Math.Abs(v);

				//update axis-as-button and button state (When checking axis we also check for button state)
				switch (input.inputType) {
					case InputDeviceType.GamepadAxis:
						controlState.held |= v > input.axisButtoncompareVal;
						break;
					case InputDeviceType.Mouse:
						controlState.held |= vAbs > 0.5f;
						break;
					case InputDeviceType.Keyboard:
					case InputDeviceType.GamepadButton:
						controlState.held |= v == 1;
						break;
					case InputDeviceType.Virtual:
						// Meh. Would be better to unify GetVirtualButton and GetVirtualAxis
						controlState.held |= VirtualInputs.GetVirtualButton(input.virtualInputID);
						break;
					case InputDeviceType.XR:
						// TO DO
						break;
				}

				if (vAbs > controlStateValueAbs) {
					//this is the value we're going with
					controlState.value = v;
					controlStateValueAbs = vAbs;
					//now find out if what set this value was something we shouldn't multiply by deltaTime
					controlState.valuePrefersDeltaUse =
						input.inputType != InputDeviceType.Mouse ||
						input.mouseInputType < MouseInputType.MouseMoveLeft ||
						input.mouseInputType > MouseInputType.MouseScroll;
				}
			}

			UpdateButtonStates(controlState, wasHeld);
		}

		void UpdateAnyControlState() {
			ControlState controlState = controlStates[0];

			var wasHeld = controlState.held;
			controlState.held = false;

			controlState.value = 0f;
			float controlStateValueAbs = 0;

			for (int i = 1; i < controlStates.Length; i++) {
				if (i > Sinput.connectedGamepads && i < (int) InputDeviceSlot.gamepad16 + 1) {
					i = (int) InputDeviceSlot.gamepad16 + 1;
				}

				var v = controlStates[i].value;
				var vAbs = Math.Abs(v);

				if (vAbs > controlStateValueAbs) {
					//this is the value we're going with
					controlState.value = v;
					controlStateValueAbs = vAbs;
					//now find out if what set this value was something we shouldn't multiply by deltaTime
					controlState.valuePrefersDeltaUse = controlStates[i].valuePrefersDeltaUse;
				}

				//check if this control is held
				controlState.held |= controlStates[i].held;

				if (controlStates[i].held) {
					Sinput.SetLastUsedDeviceSlot((InputDeviceSlot) i);
				}
			}

			UpdateButtonStates(controlState, wasHeld);
		}

		private void UpdateButtonStates(ControlState controlState, bool wasHeld) {
			//held state
			controlState.pressed = !wasHeld && controlState.held;
			controlState.released = wasHeld && !controlState.held;

			//toggled state
			controlState.togglePressed = false;
			controlState.toggleReleased = false;
			if (controlState.pressed) {
				controlState.toggleHeld = !controlState.toggleHeld;
				controlState.togglePressed = controlState.toggleHeld;
				controlState.toggleReleased = !controlState.toggleHeld;
			}

			//repeating press state
			controlState.repeatPressed = false;
			if (controlState.pressed) {
				controlState.repeatPressed = true;//repeat press returns true on first frame down
				controlState.repeatTime = Sinput.buttonRepeatWait + Sinput.buttonRepeat;
			}
			if (controlState.held) {
				controlState.repeatTime -= Time.deltaTime;
				if (controlState.repeatTime < 0f) {
					controlState.repeatTime = Sinput.buttonRepeat;
					controlState.repeatPressed = true;
				}
			}
			else {
				controlState.repeatTime = 0f;
			}
		}

		public void ResetControlStates() {
			//set all values for this control to 0
			for (int i = 0; i < controlStates.Length; i++) {
				controlStates[i].Reset();
			}
		}

		//button checks
		public override bool GetButtonState(ButtonAction bAction, InputDeviceSlot slot, bool getRaw) {
			if (!getRaw && isToggle) {
				if (bAction == ButtonAction.HELD) return controlStates[(int) slot].toggleHeld;
				if (bAction == ButtonAction.DOWN) return controlStates[(int) slot].togglePressed;
				if (bAction == ButtonAction.UP) return controlStates[(int) slot].toggleReleased;
			}
			else {
				if (bAction == ButtonAction.HELD) return controlStates[(int) slot].held;
				if (bAction == ButtonAction.DOWN) return controlStates[(int) slot].pressed;
				if (bAction == ButtonAction.UP) return controlStates[(int) slot].released;
			}
			if (bAction == ButtonAction.REPEATING) return controlStates[(int) slot].repeatPressed;

			return false;
		}

		//axis checks
		public override float GetAxisState(InputDeviceSlot slot, out bool prefersDeltaUse) {
			prefersDeltaUse = controlStates[(int) slot].valuePrefersDeltaUse;
			return controlStates[(int) slot].value;
		}
		public bool GetAxisStateDeltaPreference(InputDeviceSlot slot) {
			return controlStates[(int) slot].valuePrefersDeltaUse;
		}


		public void AddKeyboardInput(KeyCode keyCode) {
			DeviceInput input = new DeviceInput(InputDeviceType.Keyboard);
			input.keyboardKeyCode = keyCode;
			input.commonMappingType = CommonGamepadInputs.NOBUTTON;//don't remove this input when gamepads are unplugged/replugged
			inputs.Add(input);
		}

		public void AddGamepadInput(CommonGamepadInputs gamepadButtonOrAxis) { AddGamepadInput(gamepadButtonOrAxis, true); }
		private void AddGamepadInput(CommonGamepadInputs gamepadButtonOrAxis, bool isNewBinding) {
			Sinput.CheckGamepads();

			if (isNewBinding) commonMappings.Add(gamepadButtonOrAxis);
			List<DeviceInput> applicableMapInputs = CommonGamepadMappings.GetApplicableMaps(gamepadButtonOrAxis);

			AddGamepadInputs(applicableMapInputs);
		}
		private void AddGamepadInputs(List<DeviceInput> applicableMapInputs) {
			//find which common mapped inputs apply here, but already have custom binding loaded, and disregard those common mappings
			for (int ai = 0; ai < applicableMapInputs.Count; ai++) {
				bool samePad = false;
				foreach (var input in inputs) {
					if (input.inputType == InputDeviceType.GamepadAxis || input.inputType == InputDeviceType.GamepadButton) {
						if (input.isCustom) {
							if (applicableMapInputs[ai].allowedSlot == input.allowedSlot) {
								// We already have a custom bound control for this input, we don't need more
								//if I wanna be copying input display names, here's the place to do it
								//TODO: decide if I wanna do this
								//pro: it's good if the common mapping is accurate but the user wants to rebind
								//con: it's bad if the common mapping is bad or has a generic gamepad name and so it mislables different inputs
								//maybe I should do this, but with an additional check so it's not gonna happen with say, a device labelled "wireless controller"?
								samePad = true;
								break;
							}
						}
					}
				}
				// Add if common mapping still apply
				if (!samePad) {
					inputs.Add(applicableMapInputs[ai]);
				}
			}
		}

		/*public void AddXRInput(CommonXRInputs xrInputType) {
			AddXRInput(xrInputType, true);
		}
		private void AddXRInput(CommonXRInputs xrInputType, bool isNewBinding) {

			if (isNewBinding) commonXRMappings.Add(xrInputType);

			DeviceInput input = new DeviceInput(InputDeviceType.XR);
			input.commonMappingType = CommonGamepadInputs.NOBUTTON;
			input.commonXRMappingType = xrInputType;
			inputs.Add(input);
		}*/

		public void AddMouseInput(MouseInputType mouseInputType) {
			DeviceInput input = new DeviceInput(InputDeviceType.Mouse);
			input.mouseInputType = mouseInputType;
			input.commonMappingType = CommonGamepadInputs.NOBUTTON;
			inputs.Add(input);
		}

		public void AddVirtualInput(string virtualInputID) {
			DeviceInput input = new DeviceInput(InputDeviceType.Virtual);
			input.virtualInputID = virtualInputID;
			input.commonMappingType = CommonGamepadInputs.NOBUTTON;
			inputs.Add(input);
			VirtualInputs.AddInput(virtualInputID);
		}

		public void ReapplyCommonBindings() {
			//connected gamepads have changed, so we want to remove all old common bindings, and replace them now new mapping information has been loaded
			for (int i = 0; i < inputs.Count; i++) {
				if (inputs[i].commonMappingType != CommonGamepadInputs.NOBUTTON) {
					inputs.RemoveAt(i);
					i--;
				}
			}


			for (int i = 0; i < commonMappings.Count; i++) {
				AddGamepadInput(commonMappings[i], false);
			}

			// Also recheck allowed slots for custom bound pads (their inputs have a device name, common bound stuff don't)
			// Need to do this anyway so we can check if common & custom bindings are about to match on the same slot
			string[] gamepads = Sinput.gamepads;
			for (int i = 0; i < inputs.Count; i++) {
				if (inputs[i].deviceName != "") {
					for (int g = 0; g < gamepads.Length; g++) {
						if (gamepads[g] == inputs[i].deviceName.ToUpper()) {
							inputs[i].allowedSlot = (InputDeviceSlot) (g + 1);
							break;
						}
					}
				}
			}

			// Reset unused gamepads
			for (int i = Sinput.connectedGamepads + 1; i < 17; i++) {
				controlStates[i].Reset();
			}
		}

		public void SetAllowedInputSlots() {
			//custom gamepad inputs need to know which gamepad slots they can look at to match the gamepad they are for
			for (int i = 0; i < inputs.Count; i++) {
				if (inputs[i].isCustom) {
					if (inputs[i].inputType == InputDeviceType.GamepadAxis || inputs[i].inputType == InputDeviceType.GamepadButton) {
						//Debug.Log("Finding slot for gamepad: " + controls[c].inputs[i].displayName + " of " + controls[c].inputs[i].deviceName);
						//find applicable gamepad slots for this device
						for (int g = 0; g < Sinput.connectedGamepads; g++) {
							if (Sinput.gamepads[g] == inputs[i].deviceName.ToUpper()) {
								inputs[i].allowedSlot = (InputDeviceSlot) (g + 1);
								break;
							}
						}
					}
				}
			}
		}

		public override void FillInputs(List<DeviceInput> inputs, InputDeviceSlot slot) {
			foreach (var input in this.inputs) {
				if (input.CheckSlot(slot)) {
					if (slot == InputDeviceSlot.any || input.allowedSlot == (InputDeviceSlot) (-1) || input.allowedSlot == slot) {
						if (!inputs.Contains(input)) {
							inputs.Add(input);
						}
					}
				}
			}
		}
	}

	//state of control, for a frame, for one slot
	class ControlState {
		//basic cacheing of all relevant inputs for this slot
		public float value;
		public bool held;
		public bool released;
		public bool pressed;

		//for toggle checks
		public bool toggleHeld;
		public bool toggleReleased;
		public bool togglePressed;

		//for checking if the value is something that should be multiplied by deltaTime or not
		public bool valuePrefersDeltaUse = true;

		//for Sinput.ButtonPressRepeat() checks
		public bool repeatPressed;
		public float repeatTime;

		public void Reset() {
			value = 0f;
			held = false;
			released = false;
			pressed = false;
			repeatPressed = false;
			valuePrefersDeltaUse = true;
			repeatTime = 0f;
			toggleHeld = false;
			togglePressed = false;
			toggleReleased = false;
		}
	}
}
