using Manager;

namespace KK_PovX
{
	public static class Tools
	{
		public static bool HasPlayerMovement()
		{
			return
				!Controller.inHScene &&
				Game.IsInstance() &&
				Game.Instance.actScene != null &&
				Game.Instance.actScene.Player != null &&
				!Game.Instance.actScene.Player.isActionNow;
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
