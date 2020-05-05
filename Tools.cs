using Manager;
using UnityEngine;

namespace KK_PovX
{
	public static class Tools
	{
		public static bool IsInGame()
		{
			// In H scene. Definitely in-game.
			if (Controller.inHScene)
				return true;

			if (!Game.IsInstance())
				return false;

			Game game = Game.Instance;

			return
				game.actScene != null &&
				game.actScene.Map != null &&
				!game.actScene.Map.isMapLoading &&
				game.Player.chaCtrl != null;
		}

		public static bool HasPlayerMovement()
		{
			return !Controller.inHScene && !Game.Instance.actScene.Player.isActionNow;
		}

		// Return the offset of the eyes in the neck's object space.
		public static Vector3 GetEyesOffset(ChaControl chaCtrl)
		{
			Transform neck = chaCtrl.neckLookCtrl.neckLookScript.aBones[0].neckBone;
			EyeObject[] eyes = chaCtrl.eyeLookCtrl.eyeLookScript.eyeObjs;

			return Vector3.Lerp(
				GetEyesOffset_Internal(neck, eyes[0].eyeTransform),
				GetEyesOffset_Internal(neck, eyes[1].eyeTransform),
				0.5f
			);
		}

		public static Vector3 GetEyesOffset_Internal(Transform neck, Transform eye)
		{
			Vector3 offset = Vector3.zero;

			for (int i = 0; i < 50; i++)
			{
				if (eye == null || eye == neck)
					break;

				offset += eye.localPosition;
				eye = eye.parent;
			}

			return offset;
		}

		// Find smallest degrees to rotate in order to get to the next angle.
		public static float GetClosestAngle(float from, float to, out bool clockwise)
		{
			float angle = to - from;
			clockwise = (angle >= 0f && angle <= 180f) || angle <= -180f;

			if (angle < 0)
				angle += 360f;

			return clockwise ? angle : 360f - angle;
		}

		// Modulo without negative.
		public static float Mod2(float value, float mod)
		{
			if (value < 0)
				value = mod + (value % mod);

			return value % mod;
		}

		// Restrict angle where origin is at 0 angle.
		public static float AngleClamp(float value, float min, float max)
		{
			if (value > min && value < 360f - max)
				return min;
			else if (value < 360f - max && value > min)
				return 360f - max;

			return value;
		}
	}
}
