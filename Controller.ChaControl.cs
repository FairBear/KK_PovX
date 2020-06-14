using Manager;
using System.Linq;
using UnityEngine;

namespace KK_PovX
{
	public static partial class Controller
	{
		public static bool ChaControlPredicate(ChaControl chaCtrl) =>
			chaCtrl != null && chaCtrl.rendBody.isVisible;

		public static void SetChaControl(ChaControl next)
		{
			if (chaCtrl != null)
			{
				RestoreBackups();

				if (didHideHead)
				{
					didHideHead = false;
					chaCtrl.objHeadBone.SetActive(true);
				}
			}

			chaCtrl = next;

			if (chaCtrl != null)
			{
				SetBackups();

				prevPosition = GetDesiredPosition(chaCtrl);
				cameraAngleOffsetX = cameraAngleOffsetY = 0f;
				cameraAngleY = chaCtrl.neckLookCtrl.neckLookScript.aBones[0].neckBone.eulerAngles.y;

				if (KK_PovX.HideHead.Value)
				{
					didHideHead = true;
					chaCtrl.objHeadBone.SetActive(false);
				}
			}
			else
			{
				focusLockOn = -1;
				chaCtrlLockOn = null;
			}
		}

		public static ChaControl GetChaControl()
		{
			ChaControl[] chaCtrls = GetChaControls();

			if (chaCtrls.Length == 0)
				return null;

			int length = chaCtrls.Length;

			if (focus >= length)
				focus %= length;

			for (int i = 0; i < length; i++)
			{
				ChaControl target = chaCtrls[focus];

				if (ChaControlPredicate(target))
					return target;

				focus = (focus + 1) % length;
			}

			return null;
		}

		public static void RefreshChaControl()
		{
			if (Toggled)
				SetChaControl(GetChaControl());
		}

		public static ChaControl GetChaControlLockOn()
		{
			if (focusLockOn == -1)
				return null;

			ChaControl[] chaCtrls = GetChaControls();

			if (chaCtrls.Length < 2)
				goto EMPTY;

			int length = chaCtrls.Length;

			if (focusLockOn >= length)
				goto EMPTY;

			for (; focusLockOn < length; focusLockOn++)
			{
				ChaControl target = chaCtrls[focusLockOn];

				if (ChaControlPredicate(target) && target != chaCtrl)
					return target;
			}

		EMPTY:
			focusLockOn = -1;
			return null;
		}

		public static ChaControl[] GetChaControls()
		{
			if (Tools.HasPlayerMovement())
				return new ChaControl[] { Game.Instance.Player.chaCtrl };

			return Object.FindObjectsOfType<ChaControl>().Where(ChaControlPredicate).ToArray();
		}

		public static void SetBackups()
		{
			Camera camera = Camera.main;
			backupFoV = cameraFoV = camera.fieldOfView;
			backupPosition = cameraPosition = camera.transform.position;
			backupRotation = cameraRotation = camera.transform.rotation;
		}

		public static void RestoreBackups()
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;

			Camera camera = Camera.main;

			if (camera.fieldOfView == cameraFoV)
				camera.fieldOfView = backupFoV;

			if (camera.transform.position == cameraPosition)
				camera.transform.position = backupPosition;

			if (camera.transform.rotation == cameraRotation)
				camera.transform.rotation = backupRotation;
		}
	}
}
