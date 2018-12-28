using System;
using System.Activities;
using System.Activities.Tracking;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RehostedWorkflowDesigner
{


    public static class ReflectionUtils
    {

        static PropertyInfo instancePi;
        public static Type internalStateType;
        static PropertyInfo parentPi;


        static ReflectionUtils()
        {
            //activity instance
            var activityInfoType = Type.GetType("System.Activities.Tracking.ActivityInfo, System.Activities");

            instancePi = activityInfoType.GetPropertyWithName("Instance");

            internalStateType = Type.GetType("System.Activities.Statements.InternalState, System.Activities");
            var activityInstanceType = Type.GetType("System.Activities.ActivityInstance, System.Activities");

            parentPi = activityInstanceType.GetPropertyWithName("Parent");
        }

        static FieldInfo GetFieldWithName(this Type t, string name)
        {
            return t.GetRuntimeFields().Where(i => i.Name == name).First();
        }

        static PropertyInfo GetPropertyWithName(this Type t, string name)
        {
            return t.GetRuntimeProperties().Where(i => i.Name == name).First();
        }

        static MethodInfo GetmethodWithName(this Type t, string name)
        {
            return t.GetRuntimeMethods().Where(i => i.Name == name).First();
        }

        static T InvokeMethod<T>(this MethodInfo mi, object instance, params object[] parameters)
        {
            return (T)mi.Invoke(instance, parameters);
        }

        static void InvokeMethod(this MethodInfo mi, object instance, params object[] parameters)
        {
            mi.Invoke(instance, parameters);
        }

        public static bool TryGetInstance(this ActivityInfo activityInfo, out ActivityInstance activityInstance)
        {
            //get internal instance property
            activityInstance = instancePi.GetValue(activityInfo) as ActivityInstance;
            return activityInstance != null;
        }

        public static bool TryGetParent(this ActivityInstance activityInfo, out ActivityInstance activityInstance)
        {
            //get internal parent property
            activityInstance = parentPi.GetValue(activityInfo) as ActivityInstance;
            return activityInstance != null;
        }
    }
}
