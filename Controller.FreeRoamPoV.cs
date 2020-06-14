using Manager;
using UnityEngine;

namespace KK_PovX
{
	public static partial class Controller
	{
		public static void SetRotationFreeRoamPoV()
		{
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
			Game.Instance.actScene.Player.rotation = bodyQuaternion;
			neck.rotation = Quaternion.Euler(
				Tools.AngleClamp(
					Tools.Mod2(neck_euler.x + cameraAngleOffsetX, 360f),
					Tools.Mod2(neck_euler.x + KK_PovX.NeckMin.Value, 360f),
					Tools.Mod2(neck_euler.x + KK_PovX.NeckMax.Value, 360f)
				),
				cameraAngleY,
				neck_euler.z
			);
		}

		// PoV exclusively for the player.
		public static void FreeRoamPoV()
		{
			if (inScene)
			{
				inScene = false;

				if (chaCtrl != Game.Instance.Player.chaCtrl)
				{
					SetChaControl(GetChaControl());

					if (!Toggled)
						return;
				}
			}

			SetFoV();
			SetRotationFreeRoamPoV();
			SetPosition();
		}
	}
}
