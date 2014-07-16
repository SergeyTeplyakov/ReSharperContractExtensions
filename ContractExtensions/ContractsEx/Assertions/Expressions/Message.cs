using System.Diagnostics.Contracts;
using JetBrains.Annotations;

namespace ReSharper.ContractExtensions.ContractsEx
{
    public abstract class Message
    {
         
    }

    internal static class MessageExtensions
    {
        [CanBeNull]
        public static string GetStringLiteral(this Message message)
        {
            Contract.Requires(message != null);

            var stringMessage = message as StringMessage;
            return stringMessage != null ? stringMessage.Literal : null;
        }
    }

    public sealed class NoMessage : Message
    {
        private NoMessage()
        {}

        public static readonly NoMessage Instance = new NoMessage();
    }

    public sealed class StringMessage : Message
    {
        private readonly string _literal;

        public StringMessage(string literal)
        {
            Contract.Requires(!string.IsNullOrEmpty(literal));
            _literal = literal;
        }

        public string Literal
        {
            get { return _literal; }
        }
    }

    public sealed class ReferenceMessage : Message
    {
        public ReferenceMessage()
        {
        }
    }
}