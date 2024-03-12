namespace AlternativeCameraMod;

internal static class Utils
{
   public static bool EqualsFloat(float a, float b)
   {
      return (b > a - 1E-12) && (b < a + 1E-12);
   }
}
