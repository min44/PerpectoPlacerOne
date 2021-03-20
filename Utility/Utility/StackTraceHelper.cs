using System.Diagnostics;
using System.Linq;

namespace BimGen.PerpectoPlacerOne.Utility
{
    public static class StackTraceHelper
    {
        public static string GetClassName(this StackTrace stackTrace)
        {
            var currentMethod = stackTrace.GetFrame(0).GetMethod();
            var declaringType = currentMethod.DeclaringType;

            return $"{declaringType?.FullName ?? string.Empty}";
        }

        public static string GetMethodName(this StackTrace stackTrace)
        {
            var currentMethod = stackTrace.GetFrame(0).GetMethod();
            var declaringType = currentMethod.DeclaringType;

            return $"{declaringType?.FullName + "."}{currentMethod.Name}";
        }

        public static string GetAllFrames(this StackTrace stackTrace, int framesCount = 0)
        {
            var stackFrames = stackTrace.GetFrames()
                ?.Where(n => !n.GetMethod()?.DeclaringType?.FullName?.StartsWith("System.") ?? false).ToList();

            if (stackFrames == null)
                return "StackTrace is 'NULL'";

            if (framesCount > 0 && framesCount < stackFrames.Count)
                stackFrames = stackFrames.GetRange(0, framesCount).ToList();

            var methods = stackFrames.Select(n => n.GetMethod()).ToList();

            return $"{string.Join("\n\t", methods.Select(n => $"{n?.DeclaringType?.FullName + "."}{n?.Name}"))}";
        }
    }
}