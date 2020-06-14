using ActionGame;
using HarmonyLib;
using System.Collections.Generic;

namespace KK_PovX
{
	public partial class KK_PovX
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(NeckLookControllerVer2), "LateUpdate")]
		public static bool Prefix_NeckLookControllerVer2_LateUpdate(NeckLookControllerVer2 __instance)
		{
			if (!Controller.Toggled)
				return true;

			bool flag = __instance != Controller.chaCtrl.neckLookCtrl;

			if (Controller.cameraDidSet)
				return flag;

			Controller.cameraDidSet = true;

			if (Tools.HasPlayerMovement())
				Controller.FreeRoamPoV();
			else
				Controller.ScenePoV();

			return flag;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CameraControl_Ver2), "LateUpdate")]
		public static bool Prefix_CameraControl_Ver2_LateUpdate()
		{
			return !Controller.Toggled;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(HSceneProc), "SetShortcutKey")]
		public static void Prefix_HSceneProc_SetShortcutKey(HSceneProc __instance, List<ChaControl> ___lstFemale, ChaControl ___male)
		{
			Controller.inHScene = true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(HSceneProc), "OnDestroy")]
		public static void Prefix_HSceneProc_OnDestroy()
		{
			Controller.inHScene = false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CameraStateDefinitionChange), "UpdateVisible")]
		public static bool Prefix_CameraStateDefinitionChange_UpdateVisible()
		{
			return !Controller.Toggled;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CameraStateDefinitionChange), "UpdateVisibleNPC")]
		public static bool Prefix_CameraStateDefinitionChange_UpdateVisibleNPC()
		{
			return !Controller.Toggled;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ChaControl), "UpdateVisible")]
		public static void Prefix_ChaControl_UpdateVisible(ChaControl __instance)
		{
			if (Controller.inHScene && Controller.Toggled)
				__instance.fileStatus.visibleBodyAlways = true;
		}
	}
}
