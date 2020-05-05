using Manager;
using System.Collections.Generic;
using UnityEngine;

namespace KK_PovX
{
	public static class Controller
	{
		public static bool toggled = false;
		public static bool inHScene = false;

		public static Quaternion bodyQuaternion = Quaternion.identity;
		public static float bodyAngle = 0f; // Actual body, not the camera.

		// Angle offsets are used for situations where the character can't move.
		// The offsets are added to the neck's current rotation.
		// This means that the values can be negative.
		public static float cameraAngleOffsetX = 0f;
		public static float cameraAngleOffsetY = 0f;
		public static float cameraAngleY = 0f;

		// 0 = Player; 1 = 1st Partner; 2 = 2nd Partner; 3 = ...
		public static int focus = 0;
		public static List<ChaControl> chaCtrls = null;
		public static ChaControl chaCtrl = null;

		public static Vector3 prevPosition = Vector3.zero;
		//public static Vector3 eyeOffset = Vector3.zero;
		public static float backupFov = 0f;
		public static Queue<Vector3> seqPositions = new Queue<Vector3>();

		public static bool inScene = false;
		public static bool didHideHead = false;

		public static void Update()
		{
			if (KK_PovX.PovKey.Value.IsDown())
				TogglePoV(!toggled);

			if (!toggled)
				return;

			if (inHScene && chaCtrls.Count > 0)
			{
				if (KK_PovX.CharaCycleKey.Value.IsDown())
				{
					focus = (focus + 1) % chaCtrls.Count;
					SetChaControl(FromFocus());
					return;
				}
			}
			else if (focus != 0)
			{
				focus = 0;
				SetChaControl(FromFocus());
			}

			float sensitivity = KK_PovX.Sensitivity.Value;
			bool didZoom = KK_PovX.ZoomKey.Value.IsPressed();

			if (didZoom)
				sensitivity *= KK_PovX.ZoomFov.Value / KK_PovX.Fov.Value;

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

			if (inHScene)
			{
				if (KK_PovX.ToggleCursorKey.Value.IsDown())
				{
					Cursor.visible = !Cursor.visible;
					Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
				}
				else if (!didZoom && !Cursor.visible && Input.anyKeyDown)
				{
					Cursor.visible = true;
					Cursor.lockState = CursorLockMode.None;
				}
			}
		}

		public static void TogglePoV(bool flag)
		{
			if (toggled == flag)
				return;

			toggled = flag;

			if (flag)
			{
				SetChaControl(FromFocus());

				if (chaCtrl == null)
				{
					toggled = false;
					return;
				}

				cameraAngleOffsetX = cameraAngleOffsetY = 0f;
				cameraAngleY = chaCtrl.neckLookCtrl.neckLookScript.aBones[0].neckBone.eulerAngles.y;
				backupFov = Camera.main.fieldOfView;

				if (!Game.IsInstance())
					return;

				Game game = Game.Instance;

				if (game.actScene == null || game.actScene.Player == null)
					return;

				bodyQuaternion = Game.Instance.actScene.Player.rotation;
				bodyAngle = bodyQuaternion.eulerAngles.y;

				if (inHScene)
					return;

				Game.Instance.Player.chaCtrl.visibleAll = true;
				Game.Instance.actScene.VisibleList.ForEach(v => v.visibleAll = true);
			}
			else
			{
				if (inHScene)
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}

				Camera.main.fieldOfView = backupFov;

				SetChaControl(null);
			}
		}

		public static void SetChaControl(ChaControl next)
		{
			seqPositions.Clear();

			if (chaCtrl != null && didHideHead)
			{
				didHideHead = false;
				chaCtrl.objHeadBone.SetActive(true);
			}

			chaCtrl = next;

			if (chaCtrl != null)
			{
				//eyeOffset = Tools.GetEyesOffset(chaCtrl);

				if (KK_PovX.HideHead.Value)
				{
					didHideHead = true;
					chaCtrl.objHeadBone.SetActive(false);
				}
			}
		}

		public static ChaControl FromFocus()
		{
			if (chaCtrls == null || focus >= chaCtrls.Count)
				return Game.Instance.Player.chaCtrl;

			int count = chaCtrls.Count;

			for (int i = 0; i < count; i++)
			{
				ChaControl target = chaCtrls[focus];

				if (target.visibleAll)
					return chaCtrls[focus];

				// Skip invisible characters.
				focus = (focus + 1) % count;
			}

			return null;
		}

		public static void SetCamera(Transform neck)
		{
			Camera.main.fieldOfView =
				KK_PovX.ZoomKey.Value.IsPressed() ?
					KK_PovX.ZoomFov.Value :
					KK_PovX.Fov.Value;

			EyeObject[] eyes = chaCtrl.eyeLookCtrl.eyeLookScript.eyeObjs;
			Vector3 pos = Vector3.Lerp(
				eyes[0].eyeTransform.position,
				eyes[1].eyeTransform.position,
				0.5f
			);

			Vector3 next =
				pos +
				KK_PovX.OffsetX.Value * neck.right +
				KK_PovX.OffsetY.Value * neck.up +
				KK_PovX.OffsetZ.Value * neck.forward;

			if (inHScene && KK_PovX.CameraStabilize.Value)
			{
				seqPositions.Enqueue(next);

				if (seqPositions.Count > 10)
					seqPositions.Dequeue();

				next = Vector3.zero;

				foreach (Vector3 prev in seqPositions)
					next += prev;

				next /= seqPositions.Count;
			}

			Camera.main.transform.position = next;

			/*Camera.main.transform.position =
				neck.position +
				(KK_PovX.OffsetX.Value + eyeOffset.x) * neck.right +
				(KK_PovX.OffsetY.Value + eyeOffset.y) * neck.up +
				(KK_PovX.OffsetZ.Value + eyeOffset.z) * neck.forward;*/
		}

		// Used for scenes where the focused character cannot be controlled.
		public static void ScenePoV()
		{
			if (!inScene)
			{
				inScene = true;
				// Reset rotation to prevent disorientation.
				cameraAngleOffsetX = cameraAngleOffsetY = 0;
			}

			NeckObjectVer2[] bones = chaCtrl.neckLookCtrl.neckLookScript.aBones;
			Transform neck = bones[0].neckBone;

			if (KK_PovX.CameraHeadRotate.Value)
			{
				neck.Rotate(cameraAngleOffsetX, cameraAngleOffsetY, 0f);
				Camera.main.transform.rotation = neck.rotation;
			}
			else
			{
				// Preserve current neck rotation.
				Camera.main.transform.rotation = neck.rotation;
				Camera.main.transform.Rotate(cameraAngleOffsetX, cameraAngleOffsetY, 0f);
			}

			SetCamera(neck);
		}

		// PoV exclusively for the player.
		public static void FreeRoamPoV()
		{
			var player = Game.Instance.actScene.Player;

			if (Tools.HasPlayerMovement())
			{
				if (inScene)
					inScene = false;

				Vector3 nextPosition = Game.Instance.actScene.Player.position;

				if (!KK_PovX.RotateHeadFirst.Value || nextPosition != prevPosition)
				{
					// Move entire body when moving.
					bodyAngle = cameraAngleY;
					bodyQuaternion = Quaternion.Euler(0f, bodyAngle, 0f);
				}
				else
				{
					// Rotate head first. If head rotation is at the limit, rotate body.
					float angle = Tools.GetClosestAngle(bodyAngle, cameraAngleY, out bool clockwise);
					float max = KK_PovX.HeadMax.Value;

					if (angle > max)
					{
						if (clockwise)
							bodyAngle = Tools.Mod2(bodyAngle + angle - max, 360f);
						else
							bodyAngle = Tools.Mod2(bodyAngle - angle + max, 360f);

						bodyQuaternion = Quaternion.Euler(0f, bodyAngle, 0f);
					}
				}

				prevPosition = nextPosition;

				Transform neck = chaCtrl.neckLookCtrl.neckLookScript.aBones[0].neckBone;
				Vector3 neck_euler = neck.eulerAngles;

				Camera.main.transform.rotation = Quaternion.Euler(cameraAngleOffsetX, cameraAngleY, 0f);
				player.rotation = bodyQuaternion;
				neck.rotation = Quaternion.Euler(
					Tools.AngleClamp(
						Tools.Mod2(neck_euler.x + cameraAngleOffsetX, 360f),
						Tools.Mod2(neck_euler.x + KK_PovX.NeckMin.Value, 360f),
						Tools.Mod2(neck_euler.x + KK_PovX.NeckMax.Value, 360f)
					),
					cameraAngleY,
					neck_euler.z
				);

				SetCamera(neck);
			}
			else
				// When the player is unable to move, treat it as a scene.
				ScenePoV();
		}
	}
}
