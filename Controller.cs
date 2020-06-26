using System;
using UnityEngine;

namespace KK_PovX
{
	public static partial class Controller
	{
		public static bool backup = false;
		public static float backupFoV = 0f;
		public static Vector3 backupPosition = Vector3.zero;
		public static Quaternion backupRotation = Quaternion.identity;
		public static float currFoV = 0f;
		public static Vector3 currPosition = Vector3.zero;
		public static Quaternion currRotation = Quaternion.identity;

		public static bool cameraDidSet = false;

		public static void Update()
		{
			Update_PoV();
			Update_FreeRoam();
			Update_Cursor();

			if (cameraDidSet)
				cameraDidSet = false;
		}

		public static bool ThresholdFoV(float a, float b)
		{
			return Math.Abs(a - b) < 1f;
		}

		public static bool ThresholdPosition(Vector3 a, Vector3 b)
		{
			return (a - b).magnitude < 0.1f;
		}

		public static bool ThresholdRotation(Quaternion a, Quaternion b)
		{
			return Quaternion.Angle(a, b) < 1f;
		}

		public static void SetFoV()
		{
			Camera camera = Camera.main;

			if (camera == null)
				return;

			if (!ThresholdFoV(currFoV, camera.fieldOfView))
				backupFoV = camera.fieldOfView;

			camera.fieldOfView = currFoV =
				KK_PovX.ZoomKey.Value.IsPressed() ?
					KK_PovX.ZoomFoV.Value :
					KK_PovX.FoV.Value;
		}

		public static void SetPosition(Vector3 position)
		{
			Transform transform = Camera.main?.transform;

			if (transform == null)
				return;

			if (!ThresholdPosition(currPosition, transform.position))
				backupPosition = transform.position;

			transform.position = currPosition = position;
		}

		public static void SetRotation(Quaternion rotation)
		{
			Transform transform = Camera.main?.transform;

			if (transform == null)
				return;

			if (!ThresholdRotation(currRotation, transform.rotation))
				backupRotation = transform.rotation;

			transform.rotation = currRotation = rotation;
		}

		public static void SetRotation(Action act)
		{
			Transform transform = Camera.main?.transform;

			if (transform == null)
				return;

			if (!ThresholdRotation(currRotation, transform.rotation))
				backupRotation = transform.rotation;

			act();
			currRotation = transform.rotation;
		}

		public static void SetBackups()
		{
			Camera camera = Camera.main;

			if (camera == null)
				return;

			if (backup)
				RestoreBackups();

			backup = true;
			backupFoV = currFoV = camera.fieldOfView;
			backupPosition = currPosition = camera.transform.position;
			backupRotation = currRotation = camera.transform.rotation;
		}

		public static void RestoreBackups()
		{
			ToggleCursorLock(false);

			Camera camera = Camera.main;

			if (camera == null)
				return;

			backup = false;

			if (ThresholdFoV(currFoV, camera.fieldOfView))
				camera.fieldOfView = backupFoV;

			if (ThresholdPosition(currPosition, camera.transform.position))
				camera.transform.position = backupPosition;

			if (ThresholdRotation(currRotation, camera.transform.rotation))
				camera.transform.rotation = backupRotation;
		}
	}
}
