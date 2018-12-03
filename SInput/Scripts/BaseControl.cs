using System.Collections.Generic;
using UnityEngine;

namespace SinputSystems {
	public abstract class BaseControl {
		public abstract bool GetButtonState(ButtonAction bAction, InputDeviceSlot slot, bool getRaw);
		public abstract float GetAxisState(InputDeviceSlot slot, out bool prefersDeltaUse);
		public abstract void FillSprites(List<Sprite> sprites, InputDeviceSlot slot);
	}
}
