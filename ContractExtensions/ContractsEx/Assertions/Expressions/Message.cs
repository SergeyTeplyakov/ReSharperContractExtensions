using System;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx
{
    /// <summary>
    /// Represents message in the contract expression.
    /// </summary>
    /// <remarks>
    /// This hierarchy is an OO implementation of the descriminated union with a list of following cases:
    /// <see cref="NoMessage"/>, <see cref="LiteralMessage"/>, <see cref="ReferenceMessage"/> and 
    /// <see cref="InvocationMessage"/>.
    /// </remarks>
    public abstract class Message
    {
        private readonly IExpression _originalExpression;

        protected Message(IExpression originalExpression)
        {
            _originalExpression = originalExpression;
        }

        public static Message Create(IExpression expression)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<Message>() != null);

            var literal = expression as ICSharpLiteralExpression;
            if (literal != null)
                return new LiteralMessage(expression, literal.Literal.GetText());

            var reference = expression as IReferenceExpression;
            if (reference != null)
                return new ReferenceMessage(expression, reference);

            var invocationExpression = expression as IInvocationExpression;
            if (invocationExpression != null)
                return new InvocationMessage(expression, invocationExpression);

            return NoMessage.Instance;
        }

        public IExpression OriginalExpression { get { return _originalExpression; } }
    }

    public sealed class NoMessage : Message
    {
        private NoMessage()
            : base(null)
        {}

        public static readonly NoMessage Instance = new NoMessage();
    }

    public sealed class LiteralMessage : Message
    {
        private readonly string _literal;

        public LiteralMessage(IExpression originalExpression, string literal)
            : base(originalExpression)
        {
            Contract.Requires(!string.IsNullOrEmpty(literal));
            _literal = literal;
        }

        public string Literal
        {
            get { return _literal; }
        }
    }

    public sealed class InvocationMessage : Message
    {
        private readonly IInvocationExpression _invocationExpression;

        public InvocationMessage(IExpression originalExpression, IInvocationExpression invocationExpression)
            : base(originalExpression)
        {
            Contract.Requires(invocationExpression != null);

            _invocationExpression = invocationExpression;
        }

        public IInvocationExpression InvocationExpression
        {
            get { return _invocationExpression; }
        }
    }

    public sealed class ReferenceMessage : Message
    {
        private readonly IReferenceExpression _reference;

        public ReferenceMessage(IExpression originalExpression, IReferenceExpression reference)
            : base(originalExpression)
        {
            Contract.Requires(reference != null);
            _reference = reference;
        }

        public IReferenceExpression Reference
        {
            get
            {
                Contract.Ensures(Contract.Result<IReferenceExpression>() != null);
                return _reference;
            }
        }
    }

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

        //[CanBeNull]
        //public static string GetStringLiteral(this Message message)
        //{
        //    Contract.Requires(message != null);
        //    var literal = message as LiteralMessage;
        //    return literal != null ? literal.Literal : null;
        //}

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