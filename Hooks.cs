using ActionGame;
using HarmonyLib;

namespace KK_PovX
{
	public partial class KK_PovX
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(NeckLookControllerVer2), "LateUpdate")]
		public static bool Prefix_NeckLookControllerVer2_LateUpdate(NeckLookControllerVer2 __instance)
		{
			if (Controller.PoVToggled)
			{
				bool flag = __instance != Controller.chaCtrl.neckLookCtrl;

				if (!Controller.cameraDidSet)
				{
					Controller.cameraDidSet = true;

					if (Tools.HasPlayerMovement())
						Controller.CameraPoVFreeRoam();
					else
						Controller.CameraPoVScene();
				}

				return flag;
			}
			else if (Controller.FreeRoamToggled && !Controller.cameraDidSet)
			{
				Controller.cameraDidSet = true;
				Controller.CameraFreeRoam();
			}

			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CameraControl_Ver2), "LateUpdate")]
		public static bool Prefix_CameraControl_Ver2_LateUpdate()
		{
			return !Controller.PoVToggled;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CameraStateDefinitionChange), "UpdateVisible")]
		public static bool Prefix_CameraStateDefinitionChange_UpdateVisible()
		{
			return !Controller.PoVToggled;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CameraStateDefinitionChange), "UpdateVisibleNPC")]
		public static bool Prefix_CameraStateDefinitionChange_UpdateVisibleNPC()
		{
			return !Controller.PoVToggled;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ChaControl), "UpdateVisible")]
		public static void Prefix_ChaControl_UpdateVisible(ChaControl __instance)
		{
			if (Controller.PoVToggled)
				__instance.fileStatus.visibleBodyAlways = true;
		}
	}
}
