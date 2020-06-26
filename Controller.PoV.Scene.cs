using UnityEngine;

namespace KK_PovX
{
	public static partial class Controller
	{
		public static void SetRotationPoVScene()
		{
			SetRotation(SetRotationPoVScene_Internal);
		}

		public static void SetRotationPoVScene_Internal()
		{
			Quaternion headRotation = chaCtrl.objHeadBone.transform.rotation;
			Transform camTransform = Camera.main.transform;

			if (!ChaControlPredicate(chaCtrlLockOn))
				chaCtrlLockOn = GetChaControlLockOn();

			if (chaCtrlLockOn != null)
			{
				Vector3 position = camTransform.position;
				camTransform.position = GetDesiredPositionPoV(chaCtrl);
				{
					camTransform.LookAt(GetDesiredPositionPoV(chaCtrlLockOn), Vector3.up);
				}
				camTransform.position = position;
			}
			else if (KK_PovX.CameraNormalize.Value)
				camTransform.rotation = Quaternion.Euler(headRotation.eulerAngles.x, headRotation.eulerAngles.y, 0f);
			else
				camTransform.rotation = headRotation;

			camTransform.Rotate(cameraAngleOffsetX, cameraAngleOffsetY, 0f);

			if (KK_PovX.CameraHeadRotate.Value)
			{
				NeckObjectVer2[] bones = chaCtrl.neckLookCtrl.neckLookScript.aBones;
				bones[0].neckBone.rotation = camTransform.rotation;
			}
		}

		public static void CameraPoVScene()
		{
			if (!inScenePoV)
			{
				inScenePoV = true;
				cameraAngleOffsetX = cameraAngleOffsetY = 0;
			}

			SetFoV();
			SetRotationPoVScene();
			SetPositionPoV();
		}
	}
}
