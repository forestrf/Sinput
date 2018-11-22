using UnityEngine;
using UnityEngine.Profiling;

namespace SinputSystems.Examples{
	public class ShootyPlayer : MonoBehaviour {

		//which input device controls this player
		public SinputSystems.InputDeviceSlot playerSlot = SinputSystems.InputDeviceSlot.any;

		//lets display which input slot we are using, just for kicks
		public TextMesh playerSlotDisplay;

		//stuff we need for our platforming code
		private CharacterController characterController;
		private float yMotion = 0f;

		public Renderer[] playerRenderers;

		private Vector3 lookDirection = Vector3.forward;

		public GameObject bulletPrefab;
		private float bulletCooldown = 0f;
		public Transform gunTransform;

		// Use this for initialization
		void Start () {
			characterController = transform.GetComponent<CharacterController>();
			//set the player a random colour
			Color playerColor = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 1f);
			for (int i=0; i<playerRenderers.Length; i++) {
				playerRenderers[i].material.color = playerColor;
			}
			playerSlotDisplay.text = "Input:\n" + playerSlot.ToString();
		}
		
		// Update is called once per frame
		void Update () {
			
			//get player input for motion
			Profiler.BeginSample("ShootyPlayer moving GetVector");
			Vector3 motionInput = Sinput.GetVector("Horizontal", "", "Vertical", playerSlot);
			Profiler.EndSample();

			Profiler.BeginSample("ShootyPlayer characterController.Move");
			//we want to move like, three times as much as this
			motionInput *= 3f;

			//gravity
			yMotion -= Time.deltaTime * 10f;
			motionInput.y = yMotion;

			//move our character controller now
			characterController.Move(motionInput * Time.deltaTime);
			Profiler.EndSample();

			//landing/jumping
			Profiler.BeginSample("ShootyPlayer jumping");
			if (characterController.isGrounded){
				yMotion = -0.05f;

				if (Sinput.GetButtonDown("Jump", playerSlot)) {
					//we pressed jump while on the ground, so we jump!
					yMotion = 5f;
				}
			}
			Profiler.EndSample();

			//aiming
			Profiler.BeginSample("ShootyPlayer aiming");
			Vector3 aimDir = Vector3.zero;
			aimDir.x = Sinput.GetAxisRaw("Look Horizontal", playerSlot);
			aimDir.z = Sinput.GetAxisRaw("Look Vertical", playerSlot);
			if (aimDir.magnitude > 0.4f) {
				//inputs are strong enough, lets look in the aim direction
				lookDirection = aimDir.normalized;
				Quaternion fromRotation = transform.rotation;
				transform.LookAt(transform.position + lookDirection);
				transform.rotation = Quaternion.Slerp(fromRotation, transform.rotation, Time.deltaTime * 10f);
			}
			//make sure our display text always faces the same way
			playerSlotDisplay.transform.eulerAngles = Vector3.zero;
			Profiler.EndSample();

			//shooting
			Profiler.BeginSample("ShootyPlayer shooting");
			bulletCooldown -= Time.deltaTime;
			if (bulletCooldown <= 0f) {
				if (Sinput.GetButton("Fire1", playerSlot)) {
					bulletCooldown = 0.2f;
					GameObject newBullet = (GameObject) GameObject.Instantiate(bulletPrefab);
					newBullet.transform.position = gunTransform.position;
					newBullet.transform.rotation = gunTransform.rotation;
					newBullet.GetComponent<BulletScript>().moveDir = gunTransform.forward;
				}
				else if (Sinput.GetButton("Fire2", playerSlot)) {
					bulletCooldown = 0.05f;
					GameObject newBullet = (GameObject) GameObject.Instantiate(bulletPrefab);
					newBullet.transform.position = gunTransform.position;
					newBullet.transform.rotation = gunTransform.rotation;
					newBullet.transform.localScale = Vector3.one * 0.3f;
					newBullet.GetComponent<BulletScript>().moveDir = gunTransform.forward;
					newBullet.GetComponent<BulletScript>().moveSpeed = 30f;
					newBullet.GetComponent<BulletScript>().life = 0.33f;
				}
			}
			Profiler.EndSample();
		}
	}

	

}