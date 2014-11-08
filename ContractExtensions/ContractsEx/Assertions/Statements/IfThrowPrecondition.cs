using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class IfThrowPrecondition : ContractStatement, IPrecondition
    {
        private readonly IIfStatement _ifStatement;
        private readonly IClrTypeName _exceptionTypeName;

        private IfThrowPrecondition(IIfStatement ifStatement, 
            PredicateExpression predicateExpression, Message message, IClrTypeName exceptionTypeName)
            : base(ifStatement, predicateExpression, message)
        {
            Contract.Requires(ifStatement != null);
            Contract.Requires(exceptionTypeName != null);

            _ifStatement = ifStatement;
            _exceptionTypeName = exceptionTypeName;
        }

        [CanBeNull]
        internal static IfThrowPrecondition TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            var ifStatement = statement as IIfStatement;
            if (ifStatement == null || ifStatement.Condition == null)
                return null;

            var predicateExpression = PredicateExpression.Create(ifStatement.Condition);

            IThrowStatement throwStatement = ParseThrowStatement(ifStatement);
            if (throwStatement == null)
                return null;

            var arguments = throwStatement.GetArguments().ToList();
            var exceptionType = throwStatement.GetExceptionType();

            if (exceptionType == null)
                return null;

            // We can deal with any exception derived from the ArgumentException
            if (!IsDerivedOrEqualFor(exceptionType, typeof(ArgumentException)) ||
                arguments.Count == 0)
            {
                return null;
            }

            var message = 
                arguments.Skip(1).FirstOrDefault()
                .Return(ExtractMessage, NoMessage.Instance); // message is optional and should be second argument

            return new IfThrowPrecondition(ifStatement, predicateExpression, message, exceptionType);
        }

        /// <summary>
        /// Note, this implementation support only built-in CLR types!!1
        /// </summary>
        private static bool IsDerivedOrEqualFor(IClrTypeName exceptionType, Type type)
        {
            Type realExceptionType = Type.GetType(exceptionType.FullName);
            return type.IsAssignableFrom(realExceptionType);
        }

        private static IThrowStatement ParseThrowStatement(IIfStatement ifStatement)
        {
            if (ifStatement.Then is IThrowStatement)
                return ifStatement.Then as IThrowStatement;

            return ifStatement.Then
                .With(x => x as IBlock)
                .With(x => x.Statements.FirstOrDefault(s => s is IThrowStatement))
                .Return(x => x as IThrowStatement);
        }

        public IClrTypeName ExceptionTypeName
        {
            get
            {
                Contract.Ensures(Contract.Result<IClrTypeName>() != null);
                return _exceptionTypeName;
            }
        }

        public IIfStatement IfStatement
        {
            get { return _ifStatement; }
        }

        public PreconditionType PreconditionType
        {
            get { return PreconditionType.IfThrowStatement; }
        }

        public bool ChecksForNotNull(string name)
        {
            // For if-throw precondition check should be inverted!
            // That's why IsNotNull assertion means check for null
            return Predicates.Any(p => p.ChecksForNull(name));
        }
    }

    static class ThrowStatementExtensions
    {
        [CanBeNull]
        public static IClrTypeName GetExceptionType(this IThrowStatement throwStatement)
        {
            Contract.Requires(throwStatement != null);
            var declaredElement =
                throwStatement
                .With(x => x.Exception)
                .With(x => x as IObjectCreationExpression)
                .With(x => x.TypeReference)
                .With(x => x.Resolve())
                .With(x => x.DeclaredElement)
                .With(x => x as IClrDeclaredElement);

            var clrName =
                declaredElement
                .With(x => x.GetContainingType())
                .Return(x => x.GetClrName());

            if (clrName != null)
            {
                return clrName;
            }

            if (declaredElement == null)
                return null;

            // Unfortunately in the following code GetContainingType always returns null
            // although this approach works perfectly for determining CallSiteType!

            // This is terrible hack, but I don't know how to solve this!
            // declaredElement.ToString() returns "Class:Fullname" so I'll remove first part!
            return new ClrTypeName(declaredElement.ToString().Replace("Class:", ""));
        }

        public static IEnumerable<ICSharpArgument> GetArguments(this IThrowStatement throwStatement)
        {
            Contract.Requires(throwStatement != null);
            Contract.Ensures(Contract.Result<IEnumerable<ICSharpArgument>>() != null);

            return
                throwStatement
                    .With(x => x.Exception)
                    .With(x => x as IObjectCreationExpression)
                    .Return(x => x.Arguments, Enumerable.Empty<ICSharpArgument>());
        }
    }
}