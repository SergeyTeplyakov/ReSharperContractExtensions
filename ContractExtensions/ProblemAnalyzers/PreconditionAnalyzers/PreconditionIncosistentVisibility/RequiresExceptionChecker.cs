using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    [ElementProblemAnalyzer(new[] { typeof(IInvocationExpression) },
        HighlightingTypes = new[] {
            typeof(RequiresExceptionInconsistentVisibiityHighlighting),
            typeof(RequiresExceptionValidityHighlighting),
            typeof(InvalidRequiresMessageHighlighting),
        })]
    public sealed class RequiresExceptionInconsistentVisibilityChecker : PreconditionProblemAnalyzer
    {
        protected override IEnumerable<IHighlighting> DoRun(IInvocationExpression invocationExpression, 
            ContractRequiresExpression contractAssertion,
            MemberWithAccess preconditionContainer)
        {
            var preconditionException = 
                contractAssertion.GenericArgumentDeclaredType
                .With(x => x.Resolve())
                .With(x => x.DeclaredElement as IClass);

            if (preconditionException != null)
            {
                if (ExceptionIsLessVisible(preconditionContainer, preconditionException))
                {
                    yield return
                        new RequiresExceptionInconsistentVisibiityHighlighting(preconditionException,
                            preconditionContainer);
                }

                if (DoesntHaveAppropriateConstructor(preconditionException))
                {
                    yield return new RequiresExceptionValidityHighlighting(preconditionException, preconditionContainer);
                }
            }

            if (!MessageIsAppropriateForContractRequires(contractAssertion.Message))
            {
                yield return new InvalidRequiresMessageHighlighting(contractAssertion.Message);
            }

        }

        /// <summary>
        /// Returns true if exception type in the Contract.Requires&lt;E&gt; doesn't have
        /// appropriate constructors: ctor(string) or ctor(string, string).
        /// </summary>
        private bool DoesntHaveAppropriateConstructor(IClass exceptionDeclaration)
        {
            Contract.Requires(exceptionDeclaration != null);

            return exceptionDeclaration.Constructors.All(c => !c.IsSutableForGenericContractRequires());
        }

        private bool ExceptionIsLessVisible(MemberWithAccess preconditionContainer, IClass exception)
        {
            Contract.Requires(preconditionContainer != null);
            Contract.Requires(exception != null);

            return !AccessVisibilityChecker.MemberWith(exception.GetAccessRights())
                .IsAccessibleFrom(preconditionContainer.GetCombinedAccessRights());
        }

        private bool MessageIsAppropriateForContractRequires(Message message)
        {
            return message.IsValidForRequires();
        }
    }
}