using UnityEngine;

namespace SinputSystems {
	public class SInputUpdater : MonoBehaviour {
		private void Awake() {
			DontDestroyOnLoad(gameObject);
		}

		private void Update() {
			Sinput.SinputUpdate();
		}
	}
}
