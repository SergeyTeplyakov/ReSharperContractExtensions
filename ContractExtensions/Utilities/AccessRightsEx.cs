using System;
using JetBrains.ReSharper.Psi;

namespace ReSharper.ContractExtensions.Utilities
{
    internal static class AccessRightsEx
    {
        public static string ToCSharpString(this AccessRights accessRights)
        {
            switch (accessRights)
            {
                case AccessRights.PUBLIC:
                    return "public";
                case AccessRights.INTERNAL:
                    return "internal";
                case AccessRights.PROTECTED:
                    return "protected";
                case AccessRights.PROTECTED_OR_INTERNAL:
                    return "protected";
                case AccessRights.PRIVATE:
                    return "private";
                default:
                    return "";
            }
        }
    }
}