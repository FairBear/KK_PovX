using UnityEngine;

namespace KK_PovX
{
	public static partial class Controller
	{
		public static void SetRotationScenePoV()
		{
			Transform head = chaCtrl.objHeadBone.transform;
			Transform camTransform = Camera.main.transform;

			if (!ChaControlPredicate(chaCtrlLockOn))
				chaCtrlLockOn = GetChaControlLockOn();

			if (cameraRotation != Camera.main.transform.rotation)
				backupRotation = Camera.main.transform.rotation;

			if (chaCtrlLockOn != null)
			{
				Vector3 position = camTransform.position;
				camTransform.position = GetDesiredPosition(chaCtrl);
				{
					camTransform.LookAt(GetDesiredPosition(chaCtrlLockOn), Vector3.up);
				}
				camTransform.position = position;
			}
			else if (KK_PovX.CameraNormalize.Value)
				camTransform.rotation = Quaternion.Euler(head.rotation.eulerAngles.x, head.rotation.eulerAngles.y, 0f);
			else
				camTransform.rotation = head.rotation;

			Camera.main.transform.Rotate(cameraAngleOffsetX, cameraAngleOffsetY, 0f);

			if (KK_PovX.CameraHeadRotate.Value)
			{
				NeckObjectVer2[] bones = chaCtrl.neckLookCtrl.neckLookScript.aBones;
				bones[0].neckBone.rotation = Camera.main.transform.rotation;
			}

			cameraRotation = Camera.main.transform.rotation;
		}

		public static void ScenePoV()
		{
			if (!inScene)
			{
				inScene = true;
				cameraAngleOffsetX = cameraAngleOffsetY = 0;
			}

			SetRotationScenePoV();
			SetPosition();
			SetFoV();
		}
	}
}
