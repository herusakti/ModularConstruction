using UnityEngine;

public class CameraNavigation : MonoBehaviour
{
	public float movespeed;
	public float zoomSpeed;
	public float mouseSensitivity;
	public float clampAngle;

	float rotationY = 0.0f;
	float rotationX = 0.0f;

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButton(1))
		{
			float mouseX = Input.GetAxis("Mouse X");
			float mouseY = -Input.GetAxis("Mouse Y");

			rotationY += mouseX * mouseSensitivity * Time.deltaTime;
			rotationX += mouseY * mouseSensitivity * Time.deltaTime;

			rotationX = Mathf.Clamp(rotationX, -clampAngle, clampAngle);

			transform.rotation = Quaternion.Euler(rotationX, rotationY, 0.0f);
		}
		if (Input.GetMouseButton(2))
		{
			float mouseX = Input.GetAxis("Mouse X");
			float mouseY = -Input.GetAxis("Mouse Y");

			if (mouseX > 0)
			{
				transform.Translate(Vector3.right * Time.deltaTime * -movespeed);
			}
			if (mouseX < 0)
			{
				transform.Translate(Vector3.right * Time.deltaTime * movespeed);
			}

			if (mouseY > 0)
			{
				transform.Translate(Vector3.up * Time.deltaTime * movespeed);
			}
			if (mouseY < 0)
			{
				transform.Translate(Vector3.up * Time.deltaTime * -movespeed);
			}
		}

		transform.Translate(new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel")) * Time.deltaTime * zoomSpeed);
	}
	void OnEnable()
	{
		Vector3 rot = transform.localRotation.eulerAngles;
		rotationY = rot.y;
		rotationX = rot.x;
	}
}
