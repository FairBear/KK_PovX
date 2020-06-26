using Manager;
using UnityEngine;

namespace KK_PovX
{
	public static partial class Controller
	{
		public static Quaternion bodyQuaternion = Quaternion.identity;
		public static float bodyAngle = 0f; // Actual body, not the camera.

		// Angle offsets are used for situations where the character can't move.
		// The offsets are added to the neck's current rotation.
		// This means that the values can be negative.
		public static float cameraAngleOffsetX = 0f;
		public static float cameraAngleOffsetY = 0f;
		public static float cameraAngleY = 0f;
		public static float cameraSmoothness = 0f;
		public static Vector3 prevPosition = Vector3.zero;

		public static int focus = 0;
		public static int focusLockOn = -1;
		public static ChaControl chaCtrl = null;
		public static ChaControl chaCtrlLockOn = null;

		public static bool inScenePoV = false;

		public static bool PoVToggled
		{
			get => chaCtrl != null;

			set
			{
				if (PoVToggled == value)
					return;

				if (value)
				{
					if (FreeRoamToggled)
						FreeRoamToggled = false;

					focus = 0;
					ChaControl[] chaCtrls = GetChaControls();

					if (chaCtrls.Length == 0)
						return;

					SetBackups();
					SetChaControl(GetChaControl());

					if (Tools.HasPlayerMovement())
					{
						bodyQuaternion = Game.Instance.actScene.Player.rotation;
						bodyAngle = bodyQuaternion.eulerAngles.y;
					}
				}
				else
				{
					SetChaControl(null);
					RestoreBackups();
					focusLockOn = -1;
					chaCtrlLockOn = null;
				}
			}
		}

		public static void Update_PoV()
		{
			if (KK_PovX.PovKey.Value.IsDown())
				PoVToggled = !PoVToggled;

			if (!PoVToggled)
				return;

			if (!ChaControlPredicate(chaCtrl))
			{
				PoVToggled = false;
				return;
			}

			if (KK_PovX.CharaCycleKey.Value.IsDown())
			{
				int prev = focus;
				focus++;
				SetChaControl(GetChaControl());

				// Swap lock-on.
				if (focusLockOn == focus)
				{
					focusLockOn = prev;
					chaCtrlLockOn = GetChaControlLockOn();
					cameraAngleOffsetX = cameraAngleOffsetY = 0f;
				}
				return;
			}

			if (KK_PovX.LockOnKey.Value.IsDown())
			{
				focusLockOn = (focusLockOn + 2) % (GetChaControls().Length + 1) - 1;
				chaCtrlLockOn = GetChaControlLockOn();
				cameraAngleOffsetX = cameraAngleOffsetY = 0f;
				return;
			}

			MoveCameraPoV();
		}

		public static void MoveCameraPoV()
		{
			float sensitivity = KK_PovX.Sensitivity.Value;

			if (KK_PovX.ZoomKey.Value.IsPressed())
				sensitivity *= KK_PovX.ZoomFoV.Value / KK_PovX.FoV.Value;

			float x = Input.GetAxis("Mouse Y") * sensitivity;
			float y = Input.GetAxis("Mouse X") * sensitivity;

			if ((x != 0 || y != 0) &&
				(IsCursorLocked() || KK_PovX.CameraDragKey.Value.IsPressed()))
			{
				float max = KK_PovX.CameraMaxX.Value;
				float min = KK_PovX.CameraMinX.Value;
				float span = KK_PovX.CameraSpanY.Value;

				cameraAngleOffsetX = Mathf.Clamp(cameraAngleOffsetX - x, -max, min);
				cameraAngleOffsetY = Mathf.Clamp(cameraAngleOffsetY + y, -span, span);
				cameraAngleY = Tools.Mod2(cameraAngleY + y, 360f);
			}
		}

		public static Vector3 GetDesiredPositionPoV(ChaControl chaCtrl)
		{
			Transform head = chaCtrl.objHeadBone.transform;
			EyeObject[] eyes = chaCtrl.eyeLookCtrl.eyeLookScript.eyeObjs;
			Vector3 pos = Vector3.Lerp(
				eyes[0].eyeTransform.position,
				eyes[1].eyeTransform.position,
				0.5f
			);

			return pos +
				KK_PovX.OffsetX.Value * head.right +
				KK_PovX.OffsetY.Value * head.up +
				KK_PovX.OffsetZ.Value * head.forward;
		}

		public static void SetPositionPoV()
		{
			Vector3 next = GetDesiredPositionPoV(chaCtrl);

			if (cameraSmoothness > 0f)
				next = prevPosition = Vector3.Lerp(next, prevPosition, cameraSmoothness);

			SetPosition(next);
		}
	}
}
