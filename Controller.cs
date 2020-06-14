using Manager;
using UnityEngine;

namespace KK_PovX
{
	public static partial class Controller
	{
		public static bool inHScene = false;

		public static Quaternion bodyQuaternion = Quaternion.identity;
		public static float bodyAngle = 0f; // Actual body, not the camera.

		// Angle offsets are used for situations where the character can't move.
		// The offsets are added to the neck's current rotation.
		// This means that the values can be negative.
		public static float cameraAngleOffsetX = 0f;
		public static float cameraAngleOffsetY = 0f;
		public static float cameraAngleY = 0f;
		public static float cameraFoV = 0f;
		public static Vector3 cameraPosition = Vector3.zero;
		public static Quaternion cameraRotation = Quaternion.identity;
		public static bool cameraDidSet = false;
		public static float cameraSmoothness = 0f;

		public static int focus = 0;
		public static int focusLockOn = -1;
		public static ChaControl chaCtrl = null;
		public static ChaControl chaCtrlLockOn = null;

		public static Vector3 prevPosition = Vector3.zero;
		public static float backupFoV = 0f;
		public static Vector3 backupPosition = Vector3.zero;
		public static Quaternion backupRotation = Quaternion.identity;

		public static bool inScene = false;
		public static bool didHideHead = false;

		public static bool Toggled
		{
			get => chaCtrl != null;

			set
			{
				if (Toggled == value)
					return;

				if (value)
				{
					focus = 0;
					ChaControl[] chaCtrls = GetChaControls();

					if (chaCtrls.Length == 0)
						return;

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
					focusLockOn = -1;
					chaCtrlLockOn = null;
				}
			}
		}

		public static void Update()
		{
			if (cameraDidSet)
				cameraDidSet = false;

			if (KK_PovX.PovKey.Value.IsDown())
				Toggled = !Toggled;

			if (!Toggled)
				return;

			if (!ChaControlPredicate(chaCtrl))
			{
				SetChaControl(GetChaControl());

				if (chaCtrl == null)
				{
					Toggled = false;
					return;
				}
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

			if (KK_PovX.ToggleCursorKey.Value.IsDown())
			{
				Cursor.visible = !Cursor.visible;
				Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
				return;
			}

			if (Input.anyKeyDown && !Cursor.visible && !KK_PovX.ZoomKey.Value.IsPressed())
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
				return;
			}

			MoveCamera();
		}

		public static void MoveCamera()
		{
			float sensitivity = KK_PovX.Sensitivity.Value;

			if (KK_PovX.ZoomKey.Value.IsPressed())
				sensitivity *= KK_PovX.ZoomFoV.Value / KK_PovX.FoV.Value;

			float x = Input.GetAxis("Mouse Y") * sensitivity;
			float y = Input.GetAxis("Mouse X") * sensitivity;

			if (Cursor.lockState != CursorLockMode.None || KK_PovX.CameraDragKey.Value.IsPressed())
			{
				float max = KK_PovX.CameraMaxX.Value;
				float min = KK_PovX.CameraMinX.Value;
				float span = KK_PovX.CameraSpanY.Value;

				cameraAngleOffsetX = Mathf.Clamp(cameraAngleOffsetX - x, -max, min);
				cameraAngleOffsetY = Mathf.Clamp(cameraAngleOffsetY + y, -span, span);
				cameraAngleY = Tools.Mod2(cameraAngleY + y, 360f);
			}
		}

		public static Vector3 GetDesiredPosition(ChaControl chaCtrl)
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

		public static void SetFoV()
		{
			if (cameraFoV != Camera.main.fieldOfView)
				backupFoV = Camera.main.fieldOfView;

			Camera.main.fieldOfView = cameraFoV =
				KK_PovX.ZoomKey.Value.IsPressed() ?
					KK_PovX.ZoomFoV.Value :
					KK_PovX.FoV.Value;
		}

		public static void SetPosition()
		{
			if (cameraPosition != Camera.main.transform.position)
				backupPosition = Camera.main.transform.position;

			Vector3 next = GetDesiredPosition(chaCtrl);

			if (cameraSmoothness > 0f)
				next = prevPosition = Vector3.Lerp(next, prevPosition, cameraSmoothness);

			Camera.main.transform.position = cameraPosition = next;
		}
	}
}
