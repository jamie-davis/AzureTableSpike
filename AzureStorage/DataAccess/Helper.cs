using System;
using System.Collections.Generic;

namespace AzureStorage.DataAccess
{
    internal static class Helper
    {
        public static List<Type> AllowedTypes;

        static Helper()
        {
            if (AllowedTypes == null || AllowedTypes.Count == 0)
            {
                AllowedTypes = new List<Type>();
                AllowedTypes.AddRange(new Type[]
                {
                    typeof(string),
                    typeof(byte[]),
                    typeof(Int32), typeof(Int32?),
                    typeof(Int64), typeof(Int64?),
                    typeof(DateTime), typeof(DateTime?),
                    typeof(Guid), typeof(Guid?),
                    typeof(bool), typeof(bool?),
                    typeof(double), typeof(double?)
                });
            }
        }
    }
}