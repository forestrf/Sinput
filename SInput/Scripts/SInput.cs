using System;
using System.Collections.Generic;
using UnityEngine;
// Don't use `using SinputSystems;` to avoid conflicts

public static class Sinput {

	//Fixed number of gamepad things unity can handle, used mostly by GamepadDebug and InputManagerReplacementGenerator.
	//Sinput can handle as many of these as you want to throw at it buuuuut, unty can only handle so many and Sinput is wrapping unity input for now
	//You can try bumping up the range of these but you might have trouble
	//(EG, you can probably get axis of gamepads in slots over 8, but maybe not buttons?)
	public static readonly int MAXCONNECTEDGAMEPADS = 11;
	public static readonly int MAXAXISPERGAMEPAD = 28;
	public static readonly int MAXBUTTONSPERGAMEPAD = 20;


	/// <summary>
	/// Total possible device slots that Sinput may detect. (Including keyboard, mouse, virtual, and any slots)
	/// </summary>
	public static readonly int TotalPossibleDeviceSlots = Enum.GetValues(typeof(SinputSystems.InputDeviceSlot)).Length;

	//overall mouse sensitivity
	/// <summary>
	/// Overall mouse sensitivity (effects all Controls bound to mouse movements)
	/// </summary>
	public static float mouseSensitivity = 1f;

	/// <summary>
	/// Device slots in the order they were last used, without repetitions, ordered from first used (0) to last used (length - 1)
	/// </summary>
	private static List<SinputSystems.InputDeviceSlot> lastUsedDeviceSlots = new List<SinputSystems.InputDeviceSlot>();

	/// <summary>
	/// Name of control scheme used when saving/loading custom control schemes
	/// <para>unless you're doing fancy stuff like switching between various control schemes, this is probably best left alone.</para>
	/// </summary>
	public static string controlSchemeName = "ControlScheme";

	private static Dictionary<string, SinputSystems.BaseControl> ControlsDict = new Dictionary<string, SinputSystems.BaseControl>();

	//the control scheme, set it with SetControlScheme()
	public static SinputSystems.Control[] controls { get; private set; }
	public static SinputSystems.SmartControl[] smartControls { get; private set; }

	/// <summary>
	/// Returns a copy of the current Sinput control list
	/// </summary>
	public static SinputSystems.Control[] GetControlsCopy() {
		//make a copy of the controls so we're definitely not returning something that will effect _controls
		var returnControlList = new SinputSystems.Control[controls.Length];
		for (int i = 0; i < controls.Length; i++) {
			returnControlList[i] = new SinputSystems.Control(controls[i].name);
			returnControlList[i].commonMappings.AddRange(controls[i].commonMappings);
			returnControlList[i].inputs.AddRange(controls[i].inputs);
		}

		return returnControlList;
	}

	/// <summary>
	/// Fill <paramref name="inputs"/> with the inputs that represent the specified control.
	/// The inputs are ordered by last input device used to first used (TO DO).
	/// Useful to get the input's name and prompt sprite.
	/// </summary>
	public static void FillInputsForControl(List<SinputSystems.DeviceInput> inputs, string controlName, SinputSystems.InputDeviceSlot playerSlot) {
		GetControlByName(controlName).FillInputs(inputs, playerSlot);
		inputs.Sort((a, b) => GetIndexLastUsedDeviceSlot(b.allowedSlot).CompareTo(GetIndexLastUsedDeviceSlot(a.allowedSlot)));
	}

	private static int GetIndexLastUsedDeviceSlot(SinputSystems.InputDeviceSlot inputSlot) {
		if (inputSlot == (SinputSystems.InputDeviceSlot) (-1)) return -1; // Should never happen
		// Search for the latest index (the latest is also the greater index inside lastUsedDeviceSlots)
		for (int i = lastUsedDeviceSlots.Count - 1; i >= 0; i--) {
			if (inputSlot == lastUsedDeviceSlots[i]) return i;
		}
		return -1;
	}

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
		ControlsDict.Clear();

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

			loadedControls.Add(newControl);
			if (ControlsDict.ContainsKey(newControl.name)) {
				Debug.LogError("A duplicated name was found in the control [" + newControl.name + "]. A Control scheme must not repeat control names");
			}
			ControlsDict.Add(newControl.name, newControl);
		}
		controls = loadedControls.ToArray();

		//Generate smartControls from controlScheme asset
		List<SinputSystems.SmartControl> loadedSmartControls = new List<SinputSystems.SmartControl>();
		for (int i = 0; i < scheme.smartControls.Count; i++) {
			SinputSystems.SmartControl newControl = new SinputSystems.SmartControl(scheme.smartControls[i].name);

			newControl.positiveControl = scheme.smartControls[i].positiveControl;
			newControl.negativeControl = scheme.smartControls[i].negativeControl;
			//newControl.scale = scheme.smartControls[i].scale;

			newControl.inversion = new bool[TotalPossibleDeviceSlots];
			newControl.scales = new float[TotalPossibleDeviceSlots];
			for (int k = 0; k < TotalPossibleDeviceSlots; k++) {
				newControl.inversion[k] = scheme.smartControls[i].invert;
				newControl.scales[k] = scheme.smartControls[i].scale;
			}

			loadedSmartControls.Add(newControl);
			if (ControlsDict.ContainsKey(newControl.name)) {
				Debug.LogError("A duplicated name was found in the control [" + newControl.name + "]. A Control scheme must not repeat control names");
			}
			ControlsDict.Add(newControl.name, newControl);
		}
		smartControls = loadedSmartControls.ToArray();
		for (int i = 0; i < smartControls.Length; i++) smartControls[i].Init();

		//now load any saved control scheme with custom rebound inputs
		if (loadCustomControls && SinputSystems.SinputFileIO.SaveDataExists(controlSchemeName)) {
			//Debug.Log("Found saved binding!");
			controls = SinputSystems.SinputFileIO.LoadControls(controls, controlSchemeName);
		}

		//make sure controls have any gamepad-relevant stuff set correctly
		RefreshGamepadControls();

		schemeLoaded = true;
		lastUpdateFrame = -99;
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Initialize() {
		Debug.Log("Sinput Initialize");
		// Create a gameobject that will call SinputUpdate every frame before any other script (-32000 script execution order)
		var goUpdater = new GameObject("Sinput updater");
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

		UnityEngine.Profiling.Profiler.BeginSample("Sinput.Update");
		try {
			if (!schemeLoaded) LoadControlScheme("MainControlScheme", true);

			//check if connected gamepads have changed
			CheckGamepads();

			//update controls
			if (null != controls) {
				for (int i = 0; i < controls.Length; i++) {
					controls[i].Update();//resetAxisButtonStates);
				}
			}

			//update our smart controls
			if (null != smartControls) {
				for (int i = 0; i < smartControls.Length; i++) {
					smartControls[i].Update();
				}
			}
		}
		finally {
			UnityEngine.Profiling.Profiler.EndSample();
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

			refreshGamepadsNow = false;
		}
	}

	private static void RefreshGamepadControls() {
		//if (null != _controls) {
		for (int i = 0; i < controls.Length; i++) {
			//reapply common bindings
			controls[i].ReapplyCommonBindings();

			//reset axis button states
			//_controls[i].ResetAxisButtonStates();

			//make sure inputs are linked to correct gamepad slots
			controls[i].SetAllowedInputSlots();
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

		if (ButtonCheck(controlWithName, SinputSystems.InputDeviceSlot.keyboardAndMouse, SinputSystems.ButtonAction.DOWN)) return SinputSystems.InputDeviceSlot.keyboardAndMouse;

		for (int i = (int) SinputSystems.InputDeviceSlot.gamepad1; i <= (int) SinputSystems.InputDeviceSlot.gamepad11; i++) {
			if (ButtonCheck(controlWithName, (SinputSystems.InputDeviceSlot) i, SinputSystems.ButtonAction.DOWN)) return (SinputSystems.InputDeviceSlot) i;
		}

		if (ButtonCheck(controlWithName, SinputSystems.InputDeviceSlot.virtual1, SinputSystems.ButtonAction.DOWN)) return SinputSystems.InputDeviceSlot.virtual1;

		return SinputSystems.InputDeviceSlot.any;
	}

	/// <summary>
	/// Get the last used player slot. Use it when using any to know what slot is being used, for example to know what visual prompts and input names to use.
	/// Will return <see cref="SinputSystems.InputDeviceSlot.any"/> if no input has been pressed since the start of the game.
	/// </summary>
	public static SinputSystems.InputDeviceSlot GetLastUsedDeviceSlot() {
		return lastUsedDeviceSlots.Count > 0 ? lastUsedDeviceSlots[lastUsedDeviceSlots.Count - 1] : SinputSystems.InputDeviceSlot.any;
	}

	public static void SetLastUsedDeviceSlot(SinputSystems.InputDeviceSlot deviceSlot) {
		lastUsedDeviceSlots.Remove(deviceSlot);
		lastUsedDeviceSlots.Add(deviceSlot);
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
	public static float GetAxisRaw(string controlName, SinputSystems.InputDeviceSlot slot) { return GetAxisRaw(controlName, slot, out var _); }

	/// <summary>
	/// Returns the raw value of a Sinput Control or Smart Control.
	/// </summary>
	/// <param name="prefersDeltaUse">If true, the value is a delta. If false, the value is an offset from a center, like a joystick, and must be multiplied by Time.deltaTime</param>
	public static float GetAxisRaw(string controlName, SinputSystems.InputDeviceSlot slot, out bool prefersDeltaUse) { return AxisCheck(GetControlByName(controlName), slot, out prefersDeltaUse); }

	internal static float AxisCheck(SinputSystems.BaseControl control, SinputSystems.InputDeviceSlot slot, out bool prefersDeltaUse) {
		prefersDeltaUse = true; // Defaults to true, but doesn't matter because when default, the value returned is 0

		if (null == control) return 0f;

		return control.GetAxisState(slot, out prefersDeltaUse);
	}

	//vector checks
	/// <summary>
	/// Returns a Vector2 made with GetAxis() values applied to x and y
	/// </summary>
	public static Vector2 GetVector(string controlNameA, string controlNameB, SinputSystems.InputDeviceSlot slot, bool normalClip = true) { return Vector2Check(GetControlByName(controlNameA), GetControlByName(controlNameB), slot, out var _, normalClip); }
	/// <summary>
	/// Returns a Vector2 made with GetAxis() values applied to x and y
	/// </summary>
	/// <param name="prefersDeltaUse">If true, the value is a delta. If false, the value is an offset from a center, like a joystick, and must be multiplied by Time.deltaTime</param>
	public static Vector2 GetVector(string controlNameA, string controlNameB, SinputSystems.InputDeviceSlot slot, out bool prefersDeltaUse, bool normalClip = true) { return Vector2Check(GetControlByName(controlNameA), GetControlByName(controlNameB), slot, out prefersDeltaUse, normalClip); }

	static Vector2 Vector2Check(SinputSystems.BaseControl controlA, SinputSystems.BaseControl controlB, SinputSystems.InputDeviceSlot slot, out bool prefersDeltaUse, bool normalClip) {
		Vector2 returnVec2;
		returnVec2.x = AxisCheck(controlA, slot, out prefersDeltaUse);
		returnVec2.y = AxisCheck(controlB, slot, out prefersDeltaUse);

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
	public static Vector3 GetVector(string controlNameA, string controlNameB, string controlNameC, SinputSystems.InputDeviceSlot slot, bool normalClip = true) { return Vector3Check(GetControlByName(controlNameA), GetControlByName(controlNameB), GetControlByName(controlNameC), slot, out var _, normalClip); }
	/// <summary>
	/// Returns a Vector3 made with GetAxis() values applied to x, y, and z
	/// </summary>
	/// <param name="prefersDeltaUse">If true, the value is a delta. If false, the value is an offset from a center, like a joystick, and must be multiplied by Time.deltaTime</param>
	public static Vector3 GetVector(string controlNameA, string controlNameB, string controlNameC, SinputSystems.InputDeviceSlot slot, out bool prefersDeltaUse, bool normalClip = true) { return Vector3Check(GetControlByName(controlNameA), GetControlByName(controlNameB), GetControlByName(controlNameC), slot, out prefersDeltaUse, normalClip); }

	static Vector3 Vector3Check(SinputSystems.BaseControl controlA, SinputSystems.BaseControl controlB, SinputSystems.BaseControl controlC, SinputSystems.InputDeviceSlot slot, out bool prefersDeltaUse, bool normalClip) {
		Vector3 returnVec3;
		returnVec3.x = AxisCheck(controlA, slot, out prefersDeltaUse);
		returnVec3.y = AxisCheck(controlB, slot, out prefersDeltaUse);
		returnVec3.z = AxisCheck(controlC, slot, out prefersDeltaUse);

		if (normalClip) {
			var magnitude = returnVec3.magnitude;
			if (magnitude > 1f) {
				returnVec3 = returnVec3 / magnitude; // Normalize reusing magnitude (optimization)
			}
		}

		return returnVec3;
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
			for (int k = 0; k < TotalPossibleDeviceSlots; k++) {
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
			for (int k = 0; k < TotalPossibleDeviceSlots; k++) {
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
