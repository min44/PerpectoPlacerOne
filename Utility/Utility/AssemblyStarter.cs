using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BimGen.PerpectoPlacerOne.Utility
{
    public static class AssemblyStarter
    {

        public static void RunStatic(string assemblyName, string className, string methodName)
        {
            var objType = GetObjectTypeByName(assemblyName, className);
            var method = objType.GetMethod(methodName);
            method.Invoke(null, null);
        }

        public static void RunAssembly(string assemblyName, string className, string methodName, ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Type objType = GetObjectTypeByName(assemblyName, className);

            object ibaseObject = Activator.CreateInstance(objType);

            object[] arguments = new object[] { commandData, message, elements };

            objType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, ibaseObject, arguments);
        }

        public static void RunAssembly(string assemblyName, string className, string methodName, UIApplication uIApplication)
        {
            Type objType = GetObjectTypeByName(assemblyName, className);

            object ibaseObject = Activator.CreateInstance(objType);

            object[] arguments = new object[] { uIApplication };

            objType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, ibaseObject, arguments);
        }

        public static Type GetObjectTypeByName(string assemblyName, string className)
        {
            String assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            String assemblyLocation = Path.Combine(assemblyDirectory, assemblyName);

            byte[] assemblyBytes = File.ReadAllBytes(assemblyLocation);

            Assembly objAssembly = Assembly.Load(assemblyBytes);

            IEnumerable<Type> myIEnumerableType = GetTypesSafely(objAssembly);

            return myIEnumerableType.FirstOrDefault(x => x.IsClass && x.Name.ToLower() == className.ToLower());
        }

        public static IEnumerable<Type> GetTypesSafely(Assembly assembly)
        {
            try
            { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException ex)
            { return ex.Types.Where(x => x != null); }
        }
    }
}
