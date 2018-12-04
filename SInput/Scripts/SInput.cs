using System;
using System.Collections.Generic;
using UnityEngine;
// Don't use `using SinputSystems;` to avoid conflicts


public static class Sinput {

	//Fixed number of gamepad things unity can handle, used mostly by GamepadDebug and InputManagerReplacementGenerator.
	//Sinput can handle as many of these as you want to throw at it buuuuut, unty can only handle so many and Sinput is wrapping unity input for now
	//You can try bumping up the range of these but you might have trouble
	//(EG, you can probably get axis of gamepads in slots over 8, but maybe not buttons?)
	public static int MAXCONNECTEDGAMEPADS { get { return 11; } }
	public static int MAXAXISPERGAMEPAD { get { return 28; } }
	public static int MAXBUTTONSPERGAMEPAD { get { return 20; } }


	//are keyboard & mouse used by two seperate players (distinct=true) or by a single player (distinct=false)
	private static bool keyboardAndMouseAreDistinct = false;
	/// <summary>
	/// Total possible device slots that Sinput may detect. (Including keyboard, mouse, virtual, and any slots)
	/// </summary>
	public static readonly int totalPossibleDeviceSlots = Enum.GetValues(typeof(SinputSystems.InputDeviceSlot)).Length;

	//overall mouse sensitivity
	/// <summary>
	/// Overall mouse sensitivity (effects all Controls bound to mouse movements)
	/// </summary>
	public static float mouseSensitivity = 1f;

	/// <summary>
	/// Name of control scheme used when saving/loading custom control schemes
	/// <para>unless you're doing fancy stuff like switching between various control schemes, this is probably best left alone.</para>
	/// </summary>
	public static string controlSchemeName = "ControlScheme";

	//the control scheme, set it with SetControlScheme()
	private static Dictionary<string, SinputSystems.BaseControl> ControlsDict = new Dictionary<string, SinputSystems.BaseControl>();

	//the control scheme, set it with SetControlScheme()
	private static SinputSystems.Control[] _controls;
	/// <summary>
	/// Returns a copy of the current Sinput control list
	/// <para>Note: This is not the fastest thing so don't go calling it in a loop every frame, make yourself a local copy.</para>
	/// </summary>
	public static SinputSystems.Control[] controls {
		get {
			//make a copy of the controls so we're definitely not returning something that will effect _controls
			SinputSystems.Control[] returnControlList = new SinputSystems.Control[_controls.Length];
			for (int i = 0; i < _controls.Length; i++) {
				returnControlList[i] = new SinputSystems.Control(_controls[i].name);
				for (int k = 0; k < _controls[i].commonMappings.Count; k++) {
					returnControlList[i].commonMappings.Add(_controls[i].commonMappings[k]);
				}

				returnControlList[i].inputs = new List<SinputSystems.DeviceInput>();
				for (int k = 0; k < _controls[i].inputs.Count; k++) {
					returnControlList[i].inputs.Add(_controls[i].inputs[k]);
				}
			}

			return returnControlList;
		}
		//set { Init(); _controls = value; }
	}

	/// <summary>
	/// Fill <paramref name="inputs"/> with the inputs that represent the specified control.
	/// The inputs are ordered by last input device used to first used (TO DO).
	/// The first element in the array matches the current input in use.
	/// Useful to get the input's name and prompt sprite.
	/// </summary>
	public static void FillInputsForControl(List<SinputSystems.DeviceInput> inputs, string controlName, SinputSystems.InputDeviceSlot playerSlot) {
		GetControlByName(controlName).FillInputs(inputs, playerSlot);
	}

	public static SinputSystems.SmartControl[] smartControls { get; private set; }

	//gamepads list is checked every GetButton/GetAxis call, when it updates all common mapped inputs are reapplied appropriately
	static int nextGamepadCheck = -99;
	private static string[] _gamepads = new string[0];
	/// <summary>
	/// List of connected gamepads that Sinput is aware of.
	/// </summary>
	public static string[] gamepads { get { CheckGamepads(); return _gamepads; } }
	/// <summary>
	/// Number of connected gamepads that Sinput is aware of.
	/// </summary>
	public static int connectedGamepads { get { return _gamepads.Length; } }

	//XR stuff
	private static SinputSystems.XR.SinputXR xr = new SinputSystems.XR.SinputXR();

	//public static ControlScheme controlScheme;
	private static bool schemeLoaded = false;
	/// <summary>
	/// Load a Control Scheme asset.
	/// </summary>
	/// <param name="schemeName"></param>
	/// <param name="loadCustomControls"></param>
	public static void LoadControlScheme(string schemeName, bool loadCustomControls) {
		schemeLoaded = false;
		//Debug.Log("load scheme name!");
		var projectControlSchemes = Resources.LoadAll<SinputSystems.ControlScheme>("");

		int schemeIndex = -1;
		for (int i = 0; i < projectControlSchemes.Length; i++) {
			if (projectControlSchemes[i].name == schemeName) schemeIndex = i;
		}
		if (schemeIndex == -1) {
			Debug.LogError("Couldn't find control scheme \"" + schemeName + "\" in project resources.");
			return;
		}
		//controlScheme = (ControlScheme)projectControlSchemes[schemeIndex];
		LoadControlScheme(projectControlSchemes[schemeIndex], loadCustomControls);
	}
	/// <summary>
	/// Load a Control Scheme.
	/// </summary>
	/// <param name="scheme"></param>
	/// <param name="loadCustomControls"></param>
	public static void LoadControlScheme(SinputSystems.ControlScheme scheme, bool loadCustomControls) {
		//Debug.Log("load scheme asset!");

		schemeLoaded = false;

		//make sure we know what gamepads are connected
		//and load their common mappings if they are needed
		CheckGamepads(true);

		//Generate controls from controlScheme asset
		List<SinputSystems.Control> loadedControls = new List<SinputSystems.Control>();
		for (int i = 0; i < scheme.controls.Count; i++) {
			SinputSystems.Control newControl = new SinputSystems.Control(scheme.controls[i].name);

			for (int k = 0; k < scheme.controls[i].keyboardInputs.Count; k++) {
				newControl.AddKeyboardInput((KeyCode) Enum.Parse(typeof(KeyCode), scheme.controls[i].keyboardInputs[k].ToString()));
			}
			for (int k = 0; k < scheme.controls[i].gamepadInputs.Count; k++) {
				newControl.AddGamepadInput(scheme.controls[i].gamepadInputs[k]);
			}
			for (int k = 0; k < scheme.controls[i].mouseInputs.Count; k++) {
				newControl.AddMouseInput(scheme.controls[i].mouseInputs[k]);
			}
			for (int k = 0; k < scheme.controls[i].virtualInputs.Count; k++) {
				newControl.AddVirtualInput(scheme.controls[i].virtualInputs[k]);
			}
			for (int k = 0; k < scheme.controls[i].xrInputs.Count; k++) {
				newControl.AddGamepadInput(scheme.controls[i].xrInputs[k]);
			}

			loadedControls.Add(newControl);
			if (ControlsDict.ContainsKey(newControl.name)) {
				Debug.LogError("A duplicated name was found in the control. A Control scheme must not repeat control names");
			}
			ControlsDict.Add(newControl.name, newControl);
		}
		_controls = loadedControls.ToArray();

		//Generate smartControls from controlScheme asset
		List<SinputSystems.SmartControl> loadedSmartControls = new List<SinputSystems.SmartControl>();
		for (int i = 0; i < scheme.smartControls.Count; i++) {
			SinputSystems.SmartControl newControl = new SinputSystems.SmartControl(scheme.smartControls[i].name);

			newControl.positiveControl = scheme.smartControls[i].positiveControl;
			newControl.negativeControl = scheme.smartControls[i].negativeControl;
			newControl.deadzone = scheme.smartControls[i].deadzone;
			//newControl.scale = scheme.smartControls[i].scale;

			newControl.inversion = new bool[totalPossibleDeviceSlots];
			newControl.scales = new float[totalPossibleDeviceSlots];
			for (int k = 0; k < totalPossibleDeviceSlots; k++) {
				newControl.inversion[k] = scheme.smartControls[i].invert;
				newControl.scales[k] = scheme.smartControls[i].scale;
			}

			loadedSmartControls.Add(newControl);
			if (ControlsDict.ContainsKey(newControl.name)) {
				Debug.LogError("A duplicated name was found in the control. A Control scheme must not repeat control names");
			}
			ControlsDict.Add(newControl.name, newControl);
		}
		smartControls = loadedSmartControls.ToArray();
		for (int i = 0; i < smartControls.Length; i++) smartControls[i].Init();

		//now load any saved control scheme with custom rebound inputs
		if (loadCustomControls && SinputSystems.SinputFileIO.SaveDataExists(controlSchemeName)) {
			//Debug.Log("Found saved binding!");
			_controls = SinputSystems.SinputFileIO.LoadControls(_controls, controlSchemeName);
		}

		//make sure controls have any gamepad-relevant stuff set correctly
		RefreshGamepadControls();

		schemeLoaded = true;
		lastUpdateFrame = -99;
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Initialize() {
		// Create a gameobject that will call SinputUpdate every frame before any other script (-32000 script execution order)
		var goUpdater = new GameObject("Sinput updater");
		goUpdater.hideFlags = HideFlags.HideAndDontSave;
		goUpdater.AddComponent<SinputSystems.SInputUpdater>();

		SinputUpdate();
	}

	static int lastUpdateFrame = -99;
	/// <summary>
	/// Update Sinput.
	/// <para>This is called by all other Sinput functions so it is not necessary for you to call it in most circumstances.</para>
	/// </summary>
	public static void SinputUpdate() {
		if (lastUpdateFrame == Time.frameCount) return;

		lastUpdateFrame = Time.frameCount;

		if (!schemeLoaded) LoadControlScheme("MainControlScheme", true);

		//check if connected gamepads have changed
		CheckGamepads();

		//update XR stuff
		xr.Update();

		//update controls
		if (null != _controls) {
			for (int i = 0; i < _controls.Length; i++) {
				_controls[i].Update();//resetAxisButtonStates);
			}
		}

		//update our smart controls
		if (null != smartControls) {
			for (int i = 0; i < smartControls.Length; i++) {
				smartControls[i].Update();
			}
		}
	}

	/// <summary>
	/// tells Sinput to return false/0f for any input checks until the wait time has passed
	/// </summary>
	public static void ResetInputs(SinputSystems.InputDeviceSlot slot) {
		//reset smartControl values
		if (smartControls != null) {
			for (int i = 0; i < smartControls.Length; i++) {
				smartControls[i].ResetAllValues(slot);
			}
		}
	}


	//update gamepads
	static int lastCheckedGamepadRefreshFrame = -99;
	/// <summary>
	/// Checks whether connected gamepads have changed.
	/// <para>This is called before every input check so it is uneccesary for you to use it.</para>
	/// </summary>
	public static void CheckGamepads(bool refreshGamepadsNow = false) {
		if (Time.frameCount == lastCheckedGamepadRefreshFrame && !refreshGamepadsNow) return;
		lastCheckedGamepadRefreshFrame = Time.frameCount;

		//Debug.Log("checking gamepads");

		var tempInputGamepads = Input.GetJoystickNames();
		if (connectedGamepads != tempInputGamepads.Length) refreshGamepadsNow = true; //number of connected gamepads has changed
		if (!refreshGamepadsNow && nextGamepadCheck < Time.frameCount) {
			//this check is for the rare case gamepads get re-ordered in a single frame & the length of GetJoystickNames() stays the same
			nextGamepadCheck = Time.frameCount + 500;
			for (int i = 0; i < connectedGamepads; i++) {
				if (!_gamepads[i].Equals(tempInputGamepads[i], StringComparison.InvariantCultureIgnoreCase)) refreshGamepadsNow = true;
			}
		}
		if (refreshGamepadsNow) {
			//Debug.Log("Refreshing gamepads");

			//connected gamepads have changed, lets update them
			_gamepads = tempInputGamepads; // reuse array given that we already have generated it using Input.GetJoystickNames()
			for (int i = 0; i < _gamepads.Length; i++) {
				_gamepads[i] = tempInputGamepads[i].ToUpper();
			}

			//reload common mapping information for any new gamepads
			SinputSystems.CommonGamepadMappings.ReloadCommonMaps();

			//refresh control information relating to gamepads
			if (schemeLoaded) RefreshGamepadControls();

			//xr stuff too
			xr.UpdateJoystickIndeces();

			refreshGamepadsNow = false;
		}
	}

	private static void RefreshGamepadControls() {
		//if (null != _controls) {
		for (int i = 0; i < _controls.Length; i++) {
			//reapply common bindings
			_controls[i].ReapplyCommonBindings();

			//reset axis button states
			//_controls[i].ResetAxisButtonStates();

			//make sure inputs are linked to correct gamepad slots
			_controls[i].SetAllowedInputSlots();
		}
		//}
		//if (null != smartControls) {
		for (int i = 0; i < smartControls.Length; i++) {
			smartControls[i].Init();
		}
		//}
	}


	public static SinputSystems.BaseControl GetControlByName(string name) {
		return GetControlByName<SinputSystems.BaseControl>(name);
	}
	public static T GetControlByName<T>(string name) where T : SinputSystems.BaseControl {
		if (null == name || "" == name) return null;

		SinputSystems.BaseControl found;
		if (!ControlsDict.TryGetValue(name, out found)) {
			Debug.LogError("Sinput Error: Control \"" + name + "\" not found in list of Controls or SmartControls.");
			return null;
		}
		T casted = found as T;
		if (null == casted) {
			Debug.LogError("Sinput Error: Control \"" + name + "\" is not of type " + typeof(T).Name + ".");
		}
		return casted;
	}


	/// <summary>
	/// like GetButtonDown() but returns ~which~ keyboard/gamepad input slot pressed the control
	/// <para>will return InputDeviceSlot.any if no device pressed the button this frame</para>
	/// <para>use it for 'Pres A to join!' type multiplayer, and instantiate a player for the returned slot (if it isn't DeviceSlot.any)</para>
	/// </summary>
	public static SinputSystems.InputDeviceSlot GetSlotPress(string controlName) {
		return GetSlotPress(GetControlByName(controlName));
	}
	/// <summary>
	/// like GetButtonDown() but returns ~which~ keyboard/gamepad input slot pressed the control
	/// <para>will return InputDeviceSlot.any if no device pressed the button this frame</para>
	/// <para>use it for 'Pres A to join!' type multiplayer, and instantiate a player for the returned slot (if it isn't DeviceSlot.any)</para>
	/// </summary>
	public static SinputSystems.InputDeviceSlot GetSlotPress(SinputSystems.BaseControl controlWithName) {
		//like GetButtonDown() but returns ~which~ keyboard/gamepad input slot pressed the control
		//use it for 'Pres A to join!' type multiplayer, and instantiate a player for the returned slot (if it isn't DeviceSlot.any)

		if (keyboardAndMouseAreDistinct) {
			if (ButtonCheck(controlWithName, SinputSystems.InputDeviceSlot.keyboard, SinputSystems.ButtonAction.DOWN)) return SinputSystems.InputDeviceSlot.keyboard;
			if (ButtonCheck(controlWithName, SinputSystems.InputDeviceSlot.mouse, SinputSystems.ButtonAction.DOWN)) return SinputSystems.InputDeviceSlot.mouse;
		}
		else {
			if (ButtonCheck(controlWithName, SinputSystems.InputDeviceSlot.keyboardAndMouse, SinputSystems.ButtonAction.DOWN)) return SinputSystems.InputDeviceSlot.keyboardAndMouse;
			if (ButtonCheck(controlWithName, SinputSystems.InputDeviceSlot.keyboard, SinputSystems.ButtonAction.DOWN)) return SinputSystems.InputDeviceSlot.keyboardAndMouse;
			if (ButtonCheck(controlWithName, SinputSystems.InputDeviceSlot.mouse, SinputSystems.ButtonAction.DOWN)) return SinputSystems.InputDeviceSlot.keyboardAndMouse;
		}

		for (int i = (int) SinputSystems.InputDeviceSlot.gamepad1; i <= (int) SinputSystems.InputDeviceSlot.gamepad11; i++) {
			if (ButtonCheck(controlWithName, (SinputSystems.InputDeviceSlot) i, SinputSystems.ButtonAction.DOWN)) return (SinputSystems.InputDeviceSlot) i;
		}

		if (ButtonCheck(controlWithName, SinputSystems.InputDeviceSlot.virtual1, SinputSystems.ButtonAction.DOWN)) return SinputSystems.InputDeviceSlot.virtual1;

		return SinputSystems.InputDeviceSlot.any;
	}


	//Button control checks
	/// <summary>
	/// Returns true if a Sinput Control or Smart Control is Held this frame
	/// </summary>
	public static bool GetButton(string controlName, SinputSystems.InputDeviceSlot slot) { return ButtonCheck(GetControlByName(controlName), slot, SinputSystems.ButtonAction.HELD); }

	/// <summary>
	/// Returns true if a Sinput Control or Smart Control was Pressed this frame
	/// </summary>
	public static bool GetButtonDown(string controlName, SinputSystems.InputDeviceSlot slot) { return ButtonCheck(GetControlByName(controlName), slot, SinputSystems.ButtonAction.DOWN); }

	/// <summary>
	/// Returns true if a Sinput Control or Smart Control was Released this frame
	/// </summary>
	public static bool GetButtonUp(string controlName, SinputSystems.InputDeviceSlot slot) { return ButtonCheck(GetControlByName(controlName), slot, SinputSystems.ButtonAction.UP); }

	/// <summary>
	/// Returns true if a Sinput Control or Smart Control is Held this frame, regardless of the Control's toggle setting.
	/// </summary>
	public static bool GetButtonRaw(string controlName, SinputSystems.InputDeviceSlot slot) { return ButtonCheck(GetControlByName(controlName), slot, SinputSystems.ButtonAction.HELD, true); }

	/// <summary>
	/// Returns true if a Sinput Control or Smart Control was Pressed this frame, regardless of the Control's toggle setting.
	/// </summary>
	public static bool GetButtonDownRaw(string controlName, SinputSystems.InputDeviceSlot slot) { return ButtonCheck(GetControlByName(controlName), slot, SinputSystems.ButtonAction.DOWN, true); }

	/// <summary>
	/// Returns true if a Sinput Control or Smart Control was Released this frame, regardless of the Control's toggle setting.
	/// </summary>
	public static bool GetButtonUpRaw(string controlName, SinputSystems.InputDeviceSlot slot) { return ButtonCheck(GetControlByName(controlName), slot, SinputSystems.ButtonAction.UP, true); }

	//repeating button checks
	/// <summary>
	/// How long a Control must be held before GetButtonDownRepeating() starts repeating
	/// </summary>
	public static float buttonRepeatWait = 0.75f;
	/// <summary>
	/// How quickly GetButtonDownRepeating() will repeat.
	/// </summary>
	public static float buttonRepeat = 0.1f;

	/// <summary>
	/// Returns true if a Sinput Control or Smart Control was Pressed this frame, or if it has been held long enough to start repeating.
	/// <para>Use this for menu scrolling inputs</para>
	/// </summary>
	public static bool GetButtonDownRepeating(string controlName, SinputSystems.InputDeviceSlot slot) { return ButtonCheck(GetControlByName(controlName), slot, SinputSystems.ButtonAction.REPEATING); }

	public static bool ButtonCheck(SinputSystems.BaseControl control, SinputSystems.InputDeviceSlot slot, SinputSystems.ButtonAction bAction, bool getRawValue = false) {
		if (null == control) return false;

		//Debug.LogError("Sinput Error: Control \"" + control.name + "\" not found in list of controls or SmartControls.");

		else return control.GetButtonState(bAction, slot, getRawValue);
	}


	//Axis control checks
	/// <summary>
	/// Returns the raw value of a Sinput Control or Smart Control.
	/// </summary>
	public static float GetAxisRaw(string controlName, SinputSystems.InputDeviceSlot slot) { return AxisCheck(GetControlByName(controlName), out var _, slot); }

	internal static float AxisCheck(SinputSystems.BaseControl control, SinputSystems.InputDeviceSlot slot) {
		bool _;
		return AxisCheck(control, out _, slot);
	}
	internal static float AxisCheck(SinputSystems.BaseControl control, out bool prefersDeltaUse, SinputSystems.InputDeviceSlot slot) {
		prefersDeltaUse = true; // Defaults to true, but doesn't matter because when default, the value returned is 0

		if (null == control) return 0f;

		return control.GetAxisState(slot, out prefersDeltaUse);
	}

	//vector checks
	/// <summary>
	/// Returns a Vector2 made with GetAxis() values applied to x and y
	/// </summary>
	public static Vector2 GetVector(string controlNameA, string controlNameB, SinputSystems.InputDeviceSlot slot, bool normalClip = true) { return Vector2Check(GetControlByName(controlNameA), GetControlByName(controlNameB), slot, normalClip); }

	static Vector2 Vector2Check(SinputSystems.BaseControl controlA, SinputSystems.BaseControl controlB, SinputSystems.InputDeviceSlot slot, bool normalClip) {
		Vector2 returnVec2;
		returnVec2.x = AxisCheck(controlA, slot);
		returnVec2.y = AxisCheck(controlB, slot);

		if (normalClip) {
			var magnitude = returnVec2.magnitude;
			if (magnitude > 1f) {
				returnVec2 = returnVec2 / magnitude; // Normalize reusing magnitude (optimization)
			}
		}

		return returnVec2;
	}

	/// <summary>
	/// Returns a Vector3 made with GetAxis() values applied to x, y, and z
	/// </summary>
	public static Vector3 GetVector(string controlNameA, string controlNameB, string controlNameC, SinputSystems.InputDeviceSlot slot, bool normalClip = true) { return Vector3Check(GetControlByName(controlNameA), GetControlByName(controlNameB), GetControlByName(controlNameC), slot, normalClip); }

	static Vector3 Vector3Check(SinputSystems.BaseControl controlA, SinputSystems.BaseControl controlB, SinputSystems.BaseControl controlC, SinputSystems.InputDeviceSlot slot, bool normalClip) {
		Vector3 returnVec3;
		returnVec3.x = AxisCheck(controlA, slot);
		returnVec3.y = AxisCheck(controlB, slot);
		returnVec3.z = AxisCheck(controlC, slot);

		if (normalClip) {
			var magnitude = returnVec3.magnitude;
			if (magnitude > 1f) {
				returnVec3 = returnVec3 / magnitude; // Normalize reusing magnitude (optimization)
			}
		}

		return returnVec3;
	}

	//frame delta preference
	/// <summary>
	/// Returns false if the value returned by GetAxis(controlName) on this frame should NOT be multiplied by delta time.
	/// <para>For example, this will return true for gamepad stick values, false for mouse movement values</para>
	/// </summary>
	public static bool PrefersDeltaUse(string controlName, SinputSystems.InputDeviceSlot slot) { return PrefersDeltaUse(GetControlByName(controlName), slot); }
	/// <summary>
	/// Returns false if the value returned by GetAxis(controlName) on this frame should NOT be multiplied by delta time.
	/// <para>For example, this will return true for gamepad stick values, false for mouse movement values</para>
	/// </summary>
	public static bool PrefersDeltaUse(SinputSystems.BaseControl control, SinputSystems.InputDeviceSlot slot) {
		if (null == control) return false;

		bool preferDelta;
		var value = control.GetAxisState(slot, out preferDelta);

		return preferDelta;
	}


	/// <summary>
	/// sets whether a control treats GetButton() calls with press or with toggle behaviour
	/// </summary>
	public static void SetToggle(string controlName, bool toggle) { SetToggle(GetControlByName<SinputSystems.Control>(controlName), toggle); }
	/// <summary>
	/// sets whether a control treats GetButton() calls with press or with toggle behaviour
	/// </summary>
	public static void SetToggle(SinputSystems.Control control, bool toggle) {
		if (null == control) return;

		control.isToggle = toggle;
	}

	/// <summary>
	/// returns true if a control treats GetButton() calls with toggle behaviour
	/// </summary>
	public static bool GetToggle(string controlName) { return GetToggle(GetControlByName<SinputSystems.Control>(controlName)); }
	/// <summary>
	/// returns true if a control treats GetButton() calls with toggle behaviour
	/// </summary>
	public static bool GetToggle(SinputSystems.Control control) {
		if (null == control) return false;

		return control.isToggle;
	}


	/// <summary>
	/// set a smart control to be inverted or not
	/// </summary>
	public static void SetInverted(string smartControlName, bool invert, SinputSystems.InputDeviceSlot slot) { SetInverted(GetControlByName<SinputSystems.SmartControl>(smartControlName), invert, slot); }
	/// <summary>
	/// set a smart control to be inverted or not
	/// </summary>
	public static void SetInverted(SinputSystems.SmartControl smartControl, bool invert, SinputSystems.InputDeviceSlot slot) {
		if (null == smartControl) return;

		if (slot == SinputSystems.InputDeviceSlot.any) {
			for (int k = 0; k < totalPossibleDeviceSlots; k++) {
				smartControl.inversion[k] = invert;
			}
		}
		else {
			smartControl.inversion[(int) slot] = invert;
		}
	}

	/// <summary>
	/// returns true if a smart control is inverted
	/// </summary>
	public static bool GetInverted(string smartControlName, SinputSystems.InputDeviceSlot slot) { return GetInverted(GetControlByName<SinputSystems.SmartControl>(smartControlName), slot); }
	/// <summary>
	/// returns true if a smart control is inverted
	/// </summary>
	public static bool GetInverted(SinputSystems.SmartControl smartControl, SinputSystems.InputDeviceSlot slot) {
		if (null == smartControl) return false;

		return smartControl.inversion[(int) slot];
	}


	/// <summary>
	/// sets scale ("sensitivity") of a smart control
	/// </summary>
	public static void SetScale(string smartControlName, float scale, SinputSystems.InputDeviceSlot slot) { SetScale(GetControlByName<SinputSystems.SmartControl>(smartControlName), scale, slot); }
	/// <summary>
	/// sets scale ("sensitivity") of a smart control
	/// </summary>
	public static void SetScale(SinputSystems.SmartControl smartControl, float scale, SinputSystems.InputDeviceSlot slot) {
		if (null == smartControl) return;

		if (slot == SinputSystems.InputDeviceSlot.any) {
			for (int k = 0; k < totalPossibleDeviceSlots; k++) {
				smartControl.scales[k] = scale;
			}
		}
		else {
			smartControl.scales[(int) slot] = scale;
		}
	}

	/// <summary>
	/// gets scale of a smart control
	/// </summary>
	public static float GetScale(string smartControlName, SinputSystems.InputDeviceSlot slot) { return GetScale(GetControlByName<SinputSystems.SmartControl>(smartControlName), slot); }
	/// <summary>
	/// gets scale of a smart control
	/// </summary>
	public static float GetScale(SinputSystems.SmartControl smartControl, SinputSystems.InputDeviceSlot slot) {
		if (null == smartControl) return 0f;

		return smartControl.scales[(int) slot];
	}
}
