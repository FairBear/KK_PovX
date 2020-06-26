using UnityEngine;

namespace KK_PovX
{
	public static partial class Controller
	{
		public static bool _freeRoam = false;
		public static Vector3 freeRoamPosition = Vector3.zero;
		public static Quaternion freeRoamRotation = Quaternion.identity;

		public static bool FreeRoamToggled
		{
			get => _freeRoam;

			set
			{
				if (_freeRoam == value)
					return;

				if (value)
				{
					if (PoVToggled)
						PoVToggled = false;

					SetBackups();
					freeRoamPosition = currPosition;
					freeRoamRotation = currRotation;
				}
				else
					RestoreBackups();

				_freeRoam = value;
			}
		}

		public static void Update_FreeRoam()
		{
			if (KK_PovX.FreeRoamKey.Value.IsDown())
				FreeRoamToggled = !FreeRoamToggled;

			if (!FreeRoamToggled)
				return;

			if (Camera.main == null)
			{
				FreeRoamToggled = false;
				return;
			}


			// Translation

			int tx = 0;
			int ty = 0;
			int tz = 0;

			if (Input.GetKey(KK_PovX.FreeRoamLeftKey.Value.MainKey))
				tx++;

			if (Input.GetKey(KK_PovX.FreeRoamRightKey.Value.MainKey))
				tx--;

			if (Input.GetKey(KK_PovX.FreeRoamAscendKey.Value.MainKey))
				ty++;

			if (Input.GetKey(KK_PovX.FreeRoamDescendKey.Value.MainKey))
				ty--;

			if (Input.GetKey(KK_PovX.FreeRoamUpKey.Value.MainKey))
				tz++;

			if (Input.GetKey(KK_PovX.FreeRoamDownKey.Value.MainKey))
				tz--;

			if (tx != 0 || ty != 0 || tz != 0)
			{
				float speed = KK_PovX.Speed.Value * Time.unscaledDeltaTime;

				if (Input.GetKey(KK_PovX.FreeRoamSlowKey.Value.MainKey))
					speed *= 0.1f;

				freeRoamPosition +=
					freeRoamRotation * Vector3.forward * tz * speed +
					freeRoamRotation * Vector3.up * ty * speed +
					freeRoamRotation * Vector3.left * tx * speed;
			}


			// Rotation

			float sensitivity = KK_PovX.Sensitivity.Value;

			if (KK_PovX.ZoomKey.Value.IsPressed())
				sensitivity *= KK_PovX.ZoomFoV.Value / KK_PovX.FoV.Value;

			float rx = Input.GetAxis("Mouse Y") * sensitivity;
			float ry = Input.GetAxis("Mouse X") * sensitivity;

			if ((rx != 0 || ry != 0) &&
				(IsCursorLocked() || KK_PovX.CameraDragKey.Value.IsPressed()))
			{
				Vector3 euler = freeRoamRotation.eulerAngles;
				freeRoamRotation = Quaternion.Euler(
					euler.x - rx,
					euler.y + ry,
					0f
				);
			}


			// There are some cases that the `NeckLookControllerVer2` is disabled.
			if (!cameraDidSet)
			{
				cameraDidSet = true;
				CameraFreeRoam();
			}
		}

		public static void CameraFreeRoam()
		{
			SetFoV();
			SetRotation(freeRoamRotation);
			SetPosition(freeRoamPosition);
		}
	}
}
