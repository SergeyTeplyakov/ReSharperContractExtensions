using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx
{
    public sealed class IfThrowPreconditionExpression : ContractAssertionExpressionBase
    {
        private readonly IIfStatement _ifStatement;
        private readonly IClrTypeName _exceptionTypeName;
        //private readonly IExpression _predicateExpression;

        private IfThrowPreconditionExpression(List<PredicateCheck> predicates, 
            string message, IIfStatement ifStatement, IClrTypeName exceptionTypeName)
            : base(AssertionType.Precondition, predicates, message)
        {
            Contract.Requires(ifStatement != null);
            Contract.Requires(exceptionTypeName != null);
            _ifStatement = ifStatement;
            _exceptionTypeName = exceptionTypeName;
        }

        [CanBeNull]
        public static IfThrowPreconditionExpression FromStatement(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            Contract.Requires(statement != null);

            var ifStatement = statement as IIfStatement;
            if (ifStatement == null || ifStatement.Condition == null)
                return null;

            var preconditionChecks = PredicateCheckFactory.Create(ifStatement.Condition).ToList();

            IThrowStatement throwStatement = ParseThrowStatement(ifStatement);
            if (throwStatement == null)
                return null;

            var arguments = throwStatement.GetArguments().ToList();
            var exceptionType = throwStatement.GetExceptionType();

            // We can deal with any exception derived from the ArgumentException
            if (preconditionChecks.Count == 0 ||
                !IsDerivedOrEqualFor(exceptionType, typeof(ArgumentException)) ||
                arguments.Count == 0)
            {
                return null;
            }

            string message = arguments.Skip(1).FirstOrDefault(); // message is optional and should be second argument

            return new IfThrowPreconditionExpression(preconditionChecks, message, ifStatement, exceptionType);
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

        //public IExpression PredicateExpression
        //{
        //    get
        //    {
        //        Contract.Ensures(Contract.Result<IExpression>() != null);
        //        return _predicateExpression;
        //    }
        //}

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

        public static bool Throws(this IThrowStatement throwStatement, Type exceptionType)
        {
            Contract.Requires(throwStatement != null);
            Contract.Requires(exceptionType != null);

            var clrExceptionType = throwStatement.GetExceptionType();
            if (clrExceptionType == null)
                return false;

            return clrExceptionType.FullName == exceptionType.FullName;
        }

        public static IEnumerable<string> GetArguments(this IThrowStatement throwStatement)
        {
            Contract.Requires(throwStatement != null);
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            var objectCreationExpression =
                throwStatement
                    .With(x => x.Exception)
                    .With(x => x as IObjectCreationExpression);

            if (objectCreationExpression == null)
                return Enumerable.Empty<string>();

            return objectCreationExpression.Arguments
                .Select(a => a.Value as ICSharpLiteralExpression)
                .Where(x => x != null)
                .Select(x => x.Literal.GetText());
        }
    }

}