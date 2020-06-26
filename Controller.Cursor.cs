using UnityEngine;

namespace KK_PovX
{
	public static partial class Controller
	{
		public static void Update_Cursor()
		{
			if (KK_PovX.ToggleCursorKey.Value.IsDown())
			{
				ToggleCursorLock(!IsCursorLocked());
				return;
			}

			if (Input.GetMouseButtonUp(0))
				ToggleCursorLock(false);
		}


		public static void ToggleCursorLock(bool flag)
		{
			if (!GameCursor.IsInstance())
			{
				Cursor.visible = !flag;
				Cursor.lockState = flag ? CursorLockMode.Locked : CursorLockMode.None;
			}
			else
				GameCursor.Instance.SetCursorLock(flag);
		}

		public static bool IsCursorLocked()
		{
			return GameCursor.IsInstance() ? GameCursor.isLock : Cursor.lockState == CursorLockMode.Locked;
		}
	}
}
