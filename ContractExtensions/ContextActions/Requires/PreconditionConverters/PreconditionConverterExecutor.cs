using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Resources.Shell;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;
using ReSharper.ContractExtensions.ContractsEx.Assertions;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    internal sealed class PreconditionConverterExecutor : ContextActionExecutorBase
    {
        private readonly IPrecondition _precondition;
        private readonly PreconditionType _sourcePreconditionType;

        private struct Key
        {
            public PreconditionType from;
            public PreconditionType to;

            public Key To(PreconditionType to)
            {
                this.to = to;
                return this;
            }

            public static Key From(PreconditionType from)
            {
                return new Key {from = from};
            }
        }

        private readonly Dictionary<Key, Action<IPrecondition>> _converters = new Dictionary<Key, Action<IPrecondition>>();

        //private readonly PreconditionConverterAvailability _availability;
        private readonly PreconditionType _destinationPreconditionType;

        public PreconditionConverterExecutor(ICSharpStatement preconditionStatement, IPrecondition precondition,
            PreconditionType sourcePreconditionType,
            PreconditionType destinationPreconditionType)
            : base(preconditionStatement)
        {
            Contract.Requires(precondition != null);

            Contract.Requires(sourcePreconditionType != destinationPreconditionType);

            _precondition = precondition;
            _sourcePreconditionType = sourcePreconditionType;
            _destinationPreconditionType = destinationPreconditionType;

            InitializeConverters();
        }


        public PreconditionConverterExecutor(PreconditionConverterAvailability availability,
            PreconditionType destinationPreconditionType)
            : this(availability.Requires.CSharpStatement, availability.Requires, availability.SourcePreconditionType, destinationPreconditionType)
        {
        }

        protected override void DoExecuteTransaction()
        {
            var converter = GetConverter(_sourcePreconditionType, _destinationPreconditionType);
            Contract.Assert(converter != null, 
                string.Format("Converter from {0} to {1} is unavailable", _sourcePreconditionType, _destinationPreconditionType));

            converter(_precondition);
        }

        [CanBeNull]
        public Action<IPrecondition> GetConverter(PreconditionType sourcePreconditionType, PreconditionType destinationPreconditionType)
        {
            var key = Key.From(sourcePreconditionType).To(destinationPreconditionType);
            Action<IPrecondition> result;
            _converters.TryGetValue(key, out result);
            return result;
        }

        private void InitializeConverters()
        {
            _converters[Key.From(PreconditionType.ContractRequires).To(PreconditionType.GenericContractRequires)] =
                FromRequiresToGenericRequires;

            _converters[Key.From(PreconditionType.GenericContractRequires).To(PreconditionType.ContractRequires)] =
                FromGenericRequiresToRequires;

            _converters[Key.From(PreconditionType.IfThrowStatement).To(PreconditionType.ContractRequires)] =
                a => FromIfThrowToRequires(a, isGeneric: false);
            _converters[Key.From(PreconditionType.IfThrowStatement).To(PreconditionType.GenericContractRequires)] =
                a => FromIfThrowToRequires(a, isGeneric: true);
        }

        private void FromIfThrowToRequires(IPrecondition assertion, bool isGeneric)
        {
            // Convertion from if-throw precondition to Contract.Requires
            // contains following steps:
            // 1. Negate condition from the if statement (because if (s == null) throw ANE means that Contract.Requires(s != null))
            // 2. Create Contract.Requires expression (with all optional generic argument and optional message
            // 3. Add required using statements if necessary (for Contract class and Exception type)
            // 4. Replace if-throw statement with newly created contract statement

            var ifThrowAssertion = (IfThrowPrecondition) assertion;

            ICSharpExpression negatedExpression = 
                CSharpExpressionUtil.CreateLogicallyNegatedExpression(ifThrowAssertion.IfStatement.Condition);
            
            Contract.Assert(negatedExpression != null);

            string predicateCheck = negatedExpression.GetText();

            ICSharpStatement newStatement = null;
            if (isGeneric)
            {
                newStatement = CreateGenericContractRequires(ifThrowAssertion.ExceptionTypeName, predicateCheck,
                    ifThrowAssertion.Message);
            }
            else
            {
                newStatement = CreateNonGenericContractRequires(predicateCheck, ifThrowAssertion.Message);
            }

            ReplaceStatements(ifThrowAssertion.CSharpStatement, newStatement);
        }

        private void FromGenericRequiresToRequires(IPrecondition assertion)
        {
            var requiresAssertion = (ContractRequires)assertion;
            Contract.Assert(requiresAssertion.IsGenericRequires);

            string predicateCheck = requiresAssertion.OriginalPredicateExpression.GetText();
            var newStatement = CreateNonGenericContractRequires(predicateCheck, requiresAssertion.Message);
            
            ReplaceStatements(requiresAssertion.CSharpStatement, newStatement);

            RemoveSystemNamespaceUsingIfPossible();
        }

        [System.Diagnostics.Contracts.Pure]
        private ICSharpStatement CreateGenericContractRequires(IClrTypeName exceptionType, string predicateExpression, 
            Message message)
        {
            Contract.Requires(message != null);
            
            var originalExpression = message.OriginalExpression;
            
            if (originalExpression != null)
            {
                var formatWithMessage = string.Format("$0.Requires<$1>({0}, $2);", predicateExpression);

                return _factory.CreateStatement(
                    formatWithMessage, ContractType, 
                    CreateDeclaredType(exceptionType), originalExpression);
            }

            var format = string.Format("$0.Requires<$1>({0});", predicateExpression);

            return _factory.CreateStatement(format, ContractType, CreateDeclaredType(exceptionType));
        }

        [System.Diagnostics.Contracts.Pure]
        public ICSharpStatement CreateNonGenericContractRequires(string predicateExpression, Message message)
        {
            Contract.Requires(message != null);

            var originalExpression = message.OriginalExpression;

            if (originalExpression != null)
            {
                var formatWithMessage = string.Format("$0.Requires({0}, $1);", predicateExpression);

                return _factory.CreateStatement(formatWithMessage, ContractType, originalExpression);

            }
            var stringStatement = string.Format("$0.Requires({0});", predicateExpression);

            return _factory.CreateStatement(stringStatement, ContractType);
        }

        private void FromRequiresToGenericRequires(IPrecondition assertion)
        {
            var requiresAssertion = (ContractRequires) assertion;
            Contract.Assert(!requiresAssertion.IsGenericRequires);

            var exceptionType = requiresAssertion.PotentialGenericVersionException();
            
            string predicate = requiresAssertion.OriginalPredicateExpression.GetText();
            var newStatement = CreateGenericContractRequires(exceptionType, predicate, requiresAssertion.Message);

            ReplaceStatements(requiresAssertion.CSharpStatement, newStatement);
        }

        private void ReplaceStatements(ICSharpStatement original, ICSharpStatement @new)
        {
            _currentFile.GetPsiServices().Transactions.Execute("Replace Requires",
                () =>
                {
                    WriteLockCookie.Execute(() => ModificationUtil.ReplaceChild(original, @new));
                });
        }

        private void RemoveSystemNamespaceUsingIfPossible()
        {
            var usingDirective = _factory.CreateUsingDirective("using $0",
                typeof(ArgumentNullException).Namespace);

            var realUsing = _currentFile.Imports.FirstOrDefault(
                ud => ud.ImportedSymbolName.QualifiedName == usingDirective.ImportedSymbolName.QualifiedName);

            if (realUsing == null)
                return;
            
            var usages = UsingUtil.GetUsingDirectiveUsage(realUsing);

            if (usages.Count == 0)
                UsingUtil.RemoveImport(realUsing);
        }
    }
}