﻿using System;
using System.Reflection;
using System.Web;

namespace IMETPO
{
    /// <summary>
    /// This class is used to figure out the MIME type of a file from its extension.
    /// </summary>
    public static class MimeHelper
    {
        private static readonly object Locker = new object();
        private static object mimeMapping;
        private static readonly MethodInfo GetMimeMappingMethodInfo;
        static MimeHelper()
        {
            Type mimeMappingType = Assembly.GetAssembly(typeof(HttpRuntime)).GetType("System.Web.MimeMapping");
            if (mimeMappingType == null)
                throw new SystemException("Couldnt find MimeMapping type");
            GetMimeMappingMethodInfo = mimeMappingType.GetMethod("GetMimeMapping",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (GetMimeMappingMethodInfo == null)
                throw new SystemException("Couldnt find GetMimeMapping method");
            if (GetMimeMappingMethodInfo.ReturnType != typeof(string))
                throw new SystemException("GetMimeMapping method has invalid return type");
            if (GetMimeMappingMethodInfo.GetParameters().Length != 1
                && GetMimeMappingMethodInfo.GetParameters()[0].ParameterType != typeof(string))
                throw new SystemException("GetMimeMapping method has invalid parameters");
        }

        public static string GetMimeType(string filename)
        {
            lock (Locker)
                return (string)GetMimeMappingMethodInfo.Invoke(mimeMapping,
                    new object[] { filename });
        }

    }
}