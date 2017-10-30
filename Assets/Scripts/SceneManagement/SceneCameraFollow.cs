using UnityEngine;
using System.Collections;

public class SceneCameraFollow : MonoBehaviour {

	public Transform target; 

	public Vector3 offset = new Vector3(0,1,1);

	public float speed = 5; 

	private Vector3 eulerAngles;

	void Start(){
		eulerAngles = transform.rotation.eulerAngles;
	}
	 
	void LateUpdate () {
		if (target != null) {
			transform.position = Vector3.Lerp (transform.position, target.transform.position + offset, speed * Time.deltaTime);

			Quaternion lookAt = transform.rotation;
			if (offset.z < 0.01) {
				lookAt = target.transform.rotation;
			} else {
				lookAt = Quaternion.LookRotation (target.transform.position - transform.position);

			}
			eulerAngles.y = lookAt.eulerAngles.y;
			transform.eulerAngles = eulerAngles;

		}
	}
}
