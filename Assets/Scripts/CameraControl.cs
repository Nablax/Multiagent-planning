using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
	private float mRotationX = 30, mRotationY = 30;
	public float mSensitivityX = 1f;
	public float mSensitivityY = 1f;
	public float mouseSensitivityMultiplier = 3f;
	public float mCameraSpeed = 25f;

	private bool lockRotation = false;
	private void Start() {
		Cursor.visible = false;
		mRotationX = GetComponent<Transform>().localEulerAngles.y;
		mRotationY = GetComponent<Transform>().localEulerAngles.x;
	}
	void Update () {
		if(Input.GetKey(KeyCode.W))
		{
			transform.Translate(Vector3.forward * mCameraSpeed * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.S))
		{
			transform.Translate(Vector3.back * mCameraSpeed * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.A))
		{
			transform.Translate(Vector3.left * mCameraSpeed * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.D))
		{
			transform.Translate(Vector3.right * mCameraSpeed * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.C))
        {
			transform.Translate(Vector3.down * mCameraSpeed * Time.deltaTime);
        }
		if (Input.GetKey(KeyCode.Space))
        {
			transform.Translate(Vector3.up * mCameraSpeed * Time.deltaTime);
        }
		
		// sprint function
		if (Input.GetKeyDown("left shift"))
        {
			mCameraSpeed *= 3f;
        }
		else if (Input.GetKeyUp("left shift"))
        {
			mCameraSpeed /= 3f;
        }

		// allow the user to toggle on the cursor while holding down alt, which also locks the camera rotation
		if (Input.GetKeyDown("left alt"))
        {
			Cursor.visible = true;
			lockRotation = true;
        }
		else if (Input.GetKeyUp("left alt"))
        {
			Cursor.visible = false;
			lockRotation = false;
        }

		if (lockRotation == false)
		{ 
			mRotationX += Input.GetAxis("Mouse X") * (mSensitivityX * mouseSensitivityMultiplier);
			mRotationY -= Input.GetAxis("Mouse Y") * (mSensitivityY * mouseSensitivityMultiplier);
			mRotationY = Mathf.Clamp(mRotationY, -ConstValues.kMaxPitch, ConstValues.kMaxPitch);
			transform.localEulerAngles = new Vector3(mRotationY, mRotationX, 0);
		}
	}

}
