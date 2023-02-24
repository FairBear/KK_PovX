using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace KK_PovX
{
	[BepInProcess("Koikatu")]
	[BepInProcess("Koikatsu Party")]
	[BepInProcess("CharaStudio")]
	[BepInPlugin(GUID, PluginName, Version)]
	public partial class KK_PovX : BaseUnityPlugin
	{
		public const string GUID = "com.2155x.fairbair.kk_povx";
		public const string PluginName = "KK_PovX";
		public const string Version = "1.2.1";


		const string SECTION_GENERAL = "General";
		const string SECTION_SCENE_POV = "Scene PoV";
		const string SECTION_FREE_ROAM_POV = "Free-Roam PoV";
		const string SECTION_HOTKEYS = "Hotkeys";
		const string SECTION_HOTKEYS_FREE_ROAM = "Hotkeys - Free-Roam";

		const string DESCRIPTION_HIDE_HEAD =
			"Should the head be invisible when in PoV mode?";
		const string DESCRIPTION_OFFSET_X =
			"Sideway offset from the character's eyes.";
		const string DESCRIPTION_OFFSET_Y =
			"Vertical offset from the character's eyes.";
		const string DESCRIPTION_OFFSET_Z =
			"Forward offset from the character's eyes.";

		const string DESCRIPTION_CAMERA_MIN_X =
			"Highest downward and leftward angle the camera can rotate.";
		const string DESCRIPTION_CAMERA_MAX_X =
			"Highest upward and rightware angle the camera can rotate.";
		const string DESCRIPTION_CAMERA_SPAN_Y =
			"How far can the camera be rotated horizontally? " +
			"Only applies in scene PoV.";
		const string DESCRIPTION_CAMERA_NORMALIZE =
			"Stops the camera from tilting. " +
			"Always enabled when locked-on.";
		const string DESCRIPTION_CAMERA_SMOOTHNESS =
			"The higher the number, the smoother the camera becomes. " +
			"Does not apply in free-roam PoV.";
		const string DESCRIPTION_CAMERA_HEAD_ROTATE =
			"When enabled, the head rotates along with the camera. " +
			"This may ruin some animations when selecting the girls, " +
			"such as blowjobs. " +
			"Only applies in scene PoV.";

		const string DESCRIPTION_ROTATE_HEAD_FIRST =
			"Should the head rotate first before turning the whole body? " +
			"Only applies in free-roam PoV.";
		const string DESCRIPTION_NECK_MIN =
			"Highest downward angle the head can rotate.";
		const string DESCRIPTION_NECK_MAX =
			"Highest upward angle the head can rotate.";
		const string DESCRIPTION_HEAD_MAX =
			"The farthest the head can rotate until the body would rotate. " +
			"Only applies in free-roam PoV.";

		const string DESCRIPTION_CHARA_CYCLE_KEY =
			"Switch between characters during PoV mode. " +
			"Only applies in scene PoV.";
		const string DESCRIPTION_LOCK_ON_KEY =
			"Lock-on to any of the other characters during PoV mode. " +
			"Press again to cycle between characters or exit lock-on mode.";
		const string DESCRIPTION_CAMERA_DRAG_KEY =
			"During PoV mode, holding down this key will move the camera if the mouse isn't locked.";
		const string DESCRIPTION_TOGGLE_CURSOR_KEY =
			"Pressing this key will toggle the cursor. " +
			"Releasing the left mouse button will reveal the cursor.";

		const string DESCRIPTION_FREE_ROAM_SLOW_KEY =
			"Holding this key slows down the camera move speed. " +
			"Only applies in free-roam mode";

		public static ConfigEntry<bool> HideHead { get; set; }
		public static ConfigEntry<float> Sensitivity { get; set; }
		public static ConfigEntry<float> Speed { get; set; }
		public static ConfigEntry<float> FoV { get; set; }
		public static ConfigEntry<float> ZoomFoV { get; set; }
		public static ConfigEntry<float> OffsetX { get; set; }
		public static ConfigEntry<float> OffsetY { get; set; }
		public static ConfigEntry<float> OffsetZ { get; set; }

		public static ConfigEntry<float> CameraMinX { get; set; }
		public static ConfigEntry<float> CameraMaxX { get; set; }
		public static ConfigEntry<float> CameraSpanY { get; set; }
		public static ConfigEntry<bool> CameraNormalize { get; set; }
		public static ConfigEntry<int> CameraSmoothness { get; set; }
		public static ConfigEntry<bool> CameraHeadRotate { get; set; }

		public static ConfigEntry<bool> RotateHeadFirst { get; set; }
		public static ConfigEntry<float> NeckMin { get; set; }
		public static ConfigEntry<float> NeckMax { get; set; }
		public static ConfigEntry<float> HeadMax { get; set; }

		public static ConfigEntry<KeyboardShortcut> PovKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> FreeRoamKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> CharaCycleKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> LockOnKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> CameraDragKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> ToggleCursorKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> ZoomKey { get; set; }

		public static ConfigEntry<KeyboardShortcut> FreeRoamSlowKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> FreeRoamAscendKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> FreeRoamDescendKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> FreeRoamUpKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> FreeRoamDownKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> FreeRoamLeftKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> FreeRoamRightKey { get; set; }

		public void Awake()
		{
			HideHead = Config.Bind(SECTION_GENERAL, "Hide Head", true, DESCRIPTION_HIDE_HEAD);
			Sensitivity = Config.Bind(SECTION_GENERAL, "Camera Sensitivity", 2f);
			Speed = Config.Bind(SECTION_GENERAL, "Camera Move Speed", 5f);
			FoV = Config.Bind(SECTION_GENERAL, "Field of View", 70f);
			ZoomFoV = Config.Bind(SECTION_GENERAL, "Zoom Field of View", 5f);
			OffsetX = Config.Bind(SECTION_GENERAL, "Offset X", 0f, DESCRIPTION_OFFSET_X);
			OffsetY = Config.Bind(SECTION_GENERAL, "Offset Y", 0f, DESCRIPTION_OFFSET_Y);
			OffsetZ = Config.Bind(SECTION_GENERAL, "Offset Z", 0.03f, DESCRIPTION_OFFSET_Z);

			CameraMinX = Config.Bind(SECTION_SCENE_POV, "Min Camera Angle X", 80f, DESCRIPTION_CAMERA_MIN_X);
			CameraMaxX = Config.Bind(SECTION_SCENE_POV, "Max Camera Angle X", 80f, DESCRIPTION_CAMERA_MAX_X);
			CameraSpanY = Config.Bind(SECTION_SCENE_POV, "Camera Angle Span Y", 70f, DESCRIPTION_CAMERA_SPAN_Y);
			CameraNormalize = Config.Bind(SECTION_SCENE_POV, "Normalize Camera Z-Axis", false, DESCRIPTION_CAMERA_NORMALIZE);
			CameraSmoothness = Config.Bind(SECTION_SCENE_POV, "Camera Smoothness", 0, new ConfigDescription(DESCRIPTION_CAMERA_SMOOTHNESS, new AcceptableValueRange<int>(0, 90)));
			CameraHeadRotate = Config.Bind(SECTION_SCENE_POV, "Rotate Head to Camera", false, DESCRIPTION_CAMERA_HEAD_ROTATE);

			RotateHeadFirst = Config.Bind(SECTION_FREE_ROAM_POV, "Rotate Head First", true, DESCRIPTION_ROTATE_HEAD_FIRST);
			NeckMin = Config.Bind(SECTION_FREE_ROAM_POV, "Min Neck Angle X", 0f, DESCRIPTION_NECK_MIN);
			NeckMax = Config.Bind(SECTION_FREE_ROAM_POV, "Max Neck Angle X", 90f, DESCRIPTION_NECK_MAX);
			HeadMax = Config.Bind(SECTION_FREE_ROAM_POV, "Max Head Angle Y", 60f, DESCRIPTION_HEAD_MAX);

			PovKey = Config.Bind(SECTION_HOTKEYS, "PoV Toggle Key", new KeyboardShortcut(KeyCode.Comma));
			FreeRoamKey = Config.Bind(SECTION_HOTKEYS, "Free-Roam Toggle Key", new KeyboardShortcut(KeyCode.Slash));
			CharaCycleKey = Config.Bind(SECTION_HOTKEYS, "Character Cycle Key", new KeyboardShortcut(KeyCode.Period), DESCRIPTION_CHARA_CYCLE_KEY);
			LockOnKey = Config.Bind(SECTION_HOTKEYS, "Lock-On Key", new KeyboardShortcut(KeyCode.Semicolon), DESCRIPTION_LOCK_ON_KEY);
			CameraDragKey = Config.Bind(SECTION_HOTKEYS, "Camera Drag Key", new KeyboardShortcut(KeyCode.Mouse0), DESCRIPTION_CAMERA_DRAG_KEY);
			ToggleCursorKey = Config.Bind(SECTION_HOTKEYS, "Toggle Cursor Key", new KeyboardShortcut(KeyCode.BackQuote), DESCRIPTION_TOGGLE_CURSOR_KEY);
			ZoomKey = Config.Bind(SECTION_HOTKEYS, "Zoom Key", new KeyboardShortcut(KeyCode.X));

			FreeRoamSlowKey = Config.Bind(SECTION_HOTKEYS_FREE_ROAM, "Free-Roam Slowdown Key", new KeyboardShortcut(KeyCode.LeftShift));
			FreeRoamAscendKey = Config.Bind(SECTION_HOTKEYS_FREE_ROAM, "Free-Roam Ascend Key", new KeyboardShortcut(KeyCode.Q));
			FreeRoamDescendKey = Config.Bind(SECTION_HOTKEYS_FREE_ROAM, "Free-Roam Descend Key", new KeyboardShortcut(KeyCode.E));
			FreeRoamUpKey = Config.Bind(SECTION_HOTKEYS_FREE_ROAM, "Free-Roam Up Key", new KeyboardShortcut(KeyCode.W));
			FreeRoamDownKey = Config.Bind(SECTION_HOTKEYS_FREE_ROAM, "Free-Roam Down Key", new KeyboardShortcut(KeyCode.S));
			FreeRoamLeftKey = Config.Bind(SECTION_HOTKEYS_FREE_ROAM, "Free-Roam Left Key", new KeyboardShortcut(KeyCode.A));
			FreeRoamRightKey = Config.Bind(SECTION_HOTKEYS_FREE_ROAM, "Free-Roam Right Key", new KeyboardShortcut(KeyCode.D));

			HideHead.SettingChanged += (sender, args) => Controller.RefreshChaControl();

			Harmony.CreateAndPatchAll(typeof(KK_PovX));
		}

		public void Update()
		{
			Controller.Update();
		}
	}
}
