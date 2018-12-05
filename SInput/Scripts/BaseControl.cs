using System.Collections.Generic;

namespace SinputSystems {
	public abstract class BaseControl {
		public abstract bool GetButtonState(ButtonAction bAction, InputDeviceSlot slot, bool getRaw);
		public abstract float GetAxisState(InputDeviceSlot slot, out bool prefersDeltaUse);
		public abstract void FillInputs(List<KeyValuePair<InputDeviceSlot, DeviceInput>> inputs, InputDeviceSlot slot);
	}
}
