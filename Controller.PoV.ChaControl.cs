using Manager;
using System.Linq;
using UnityEngine;

namespace KK_PovX
{
	public static partial class Controller
	{
		public static bool didHideHead = false;

		public static bool ChaControlPredicate(ChaControl chaCtrl) =>
			chaCtrl != null && chaCtrl.rendBody.isVisible;

		public static void SetChaControl(ChaControl next)
		{
			if (chaCtrl != null && didHideHead)
			{
				didHideHead = false;
				chaCtrl.objHeadBone.SetActive(true);
			}

			chaCtrl = next;

			if (chaCtrl != null)
			{
				prevPosition = GetDesiredPositionPoV(chaCtrl);
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
			if (PoVToggled)
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
	}
}
