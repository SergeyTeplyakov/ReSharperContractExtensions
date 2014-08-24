using System;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    internal static class MessageExtensions
    {
        public static T Match<T>(this Message message,
            Func<LiteralMessage, T> stringFunc,
            Func<ReferenceMessage, T> referenceFunc,
            Func<InvocationMessage, T> invocationFunc,
            Func<T> noMessageFunc)
        {
            Contract.Requires(message != null);
            Contract.Requires(stringFunc != null);
            Contract.Requires(referenceFunc != null);
            Contract.Requires(invocationFunc != null);
            Contract.Requires(noMessageFunc != null);

            var stringMessage = message as LiteralMessage;
            if (stringMessage != null)
                return stringFunc(stringMessage);

            var referenceMessage = message as ReferenceMessage;
            if (referenceMessage != null)
                return referenceFunc(referenceMessage);

            var invocationReference = message as InvocationMessage;
            if (invocationReference != null)
                return invocationFunc(invocationReference);

            if (message is NoMessage)
                return noMessageFunc();

            Contract.Assert(false, "Unknown message type: " + message.GetType());
            return noMessageFunc();
        }

        public static bool IsValidForRequires(this Message message)
        {
            Contract.Requires(message != null);
                        
            return Match(message, _ => true, // string literals are valid
                r => IsValidReference(r.Reference), // references should be checked separately
                _ => false, // message calls are invalid
                () => true); // "NoMessage" is valid

        }

        /// <summary>
        /// Returns true if <paramref name="referenceExpression"/> points to the static field or property
        /// with at least internal access.
        /// </summary>
        private static bool IsValidReference(IReferenceExpression referenceExpression)
        {
            Contract.Requires(referenceExpression != null);

            var modifiersOwner =
                referenceExpression
                    .With(x => x.Reference)
                    .With(x => x.Resolve())
                    .With(x => x.DeclaredElement as IModifiersOwner);

            if (modifiersOwner == null)
                return false;
            
            // Supported only fields or properties
            if (!(modifiersOwner is IField) && !(modifiersOwner is IProperty))
                return false;

            // Checking access rights and is this stuff is static
            var accessRights = modifiersOwner.GetAccessRights();
            if (accessRights == AccessRights.PUBLIC ||
                accessRights == AccessRights.INTERNAL ||
                accessRights == AccessRights.PROTECTED_OR_INTERNAL)
            {
                if (modifiersOwner.IsStatic)
                    return true;
            }

            return false;
        }
    }
}