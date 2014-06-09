using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    /// <summary>
    /// Represents contract precondition check in form if(arg != null) throw new ArgumentNullException();
    /// </summary>
    public sealed class IfThrowPreconditionAssertion : ContractPreconditionAssertion
    {
        private readonly IList<IPredicateCheck> _predicates;

        private IfThrowPreconditionAssertion(ICSharpStatement statement, 
            IList<IPredicateCheck> predicates) 
            : base(statement)
        {
            Contract.Requires(predicates != null);

            _predicates = predicates;
        }

        public override bool IsCodeContractBasedPrecondition
        {
            get { return false; }
        }

        public override bool ChecksForNull(string name)
        {
            return _predicates.Any(p => p.ChecksForNull(name));
        }

        [CanBeNull]
        new internal static IfThrowPreconditionAssertion TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            var ifStatement = statement as IIfStatement;
            if (ifStatement == null)
                return null;

            var preconditionChecks = PredicateCheckFactory.Create(ifStatement.Condition).ToList();

            IThrowStatement throwStatement = ParseThrowStatement(ifStatement);

            var arguments = throwStatement.GetArguments().ToList();

            // TODO: is there any other exception types except ArgumentNullException?
            if (preconditionChecks.Count == 0 ||
                !throwStatement.Throws(typeof(ArgumentNullException)) ||
                arguments.Count == 0)
            {
                return null;
            }

            return new IfThrowPreconditionAssertion(statement, preconditionChecks)
            {
                Message = arguments.Skip(1).FirstOrDefault(), // message is optional and should be second argument
            };
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
    }

    static class ThrowStatementExtensions
    {
        public static bool Throws(this IThrowStatement throwStatement, Type exceptionType)
        {
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
                return clrName.FullName == exceptionType.FullName;
            }

            // Unfortunately in the following code GetContainingType always returns null
            // although this approach works perfectly for determining CallSiteType!

            return declaredElement.ToString().Contains(exceptionType.FullName);
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