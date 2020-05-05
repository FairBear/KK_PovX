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
			if (!Controller.toggled ||
				Controller.chaCtrl == null ||
				__instance != Controller.chaCtrl.neckLookCtrl)
				return true;

			if (Tools.HasPlayerMovement())
				Controller.FreeRoamPoV();
			else
				Controller.ScenePoV();

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(HSceneProc), "SetShortcutKey")]
		public static void Prefix_HSceneProc_SetShortcutKey(List<ChaControl> ___lstFemale, ChaControl ___male, ChaControl ___male1)
		{
			Controller.chaCtrls = new List<ChaControl>();

			if (___male)
				Controller.chaCtrls.Add(___male);

			if (___male1)
				Controller.chaCtrls.Add(___male1);

			foreach (ChaControl female in ___lstFemale)
				Controller.chaCtrls.Add(female);

			Controller.inHScene = true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(HSceneProc), "OnDestroy")]
		public static void Prefix_HSceneProc_OnDestroy()
		{
			Controller.inHScene = false;
			Controller.chaCtrls = null;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CameraStateDefinitionChange), "UpdateVisible")]
		public static bool Prefix_CameraStateDefinitionChange_UpdateVisible()
		{
			return !Controller.toggled;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CameraStateDefinitionChange), "UpdateVisibleNPC")]
		public static bool Prefix_CameraStateDefinitionChange_UpdateVisibleNPC()
		{
			return !Controller.toggled;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ChaControl), "UpdateVisible")]
		public static void Prefix_ChaControl_UpdateVisible(ChaControl __instance)
		{
			if (Controller.inHScene && Controller.toggled)
				__instance.fileStatus.visibleBodyAlways = true;
		}
	}
}
