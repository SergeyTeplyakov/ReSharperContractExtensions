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
    public sealed class IfThrowPreconditionStatement : ContractPreconditionStatementBase
    {
        internal IfThrowPreconditionStatement(ICSharpStatement statement, IfThrowPreconditionExpression expression) 
            : base(statement, expression)
        {
            ExceptionType = expression.ExceptionTypeName;
            IfStatement = expression.IfStatement;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(IfStatement != null);
            Contract.Invariant(ExceptionType != null);
        }

        /// <summary>
        /// Returns true if current precondition is based on Code Contracts library, false otherwise.
        /// </summary>
        public override bool IsCodeContractBasedPrecondition
        {
            get { return false; }
        }

        /// <summary>
        /// Returns true if current Assertion checks for null something with specified <paramref name="name"/>.
        /// </summary>
        public override bool AssertsArgumentIsNotNull(string name)
        {
            // For if-throw precondition check should be inverted!
            // That's why IsNotNull assertion means check for null
            return _expression.Predicates.Any(p => p.ChecksForNull(name));
        }

        public override bool AssertsArgumentIsNull(string name)
        {
            // For if-throw precondition check should be inverted!
            return _expression.Predicates.Any(p => p.ChecksForNotNull(name));
        }

        public override PreconditionType PreconditionType
        {
            get { return PreconditionType.IfThrowStatement; }
        }

        public IIfStatement IfStatement { get; private set; }
        public IClrTypeName ExceptionType { get; private set; }

        //[CanBeNull]
        //new internal static IfThrowPreconditionStatement TryCreate(ICSharpStatement statement)
        //{
        //    Contract.Requires(statement != null);
        //    var expression = IfThrowPreconditionExpression.FromStatement(statement);
        //    return expression.With(x => new IfThrowPreconditionStatement(statement, expression));
        //}
    }
}