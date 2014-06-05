using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Features.Browsing.Resources;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx
{
    public enum PreconditionType
    {
        Requires,
        Invariant,
    }

    internal interface IPreconditionExpression
    {
        string ArgumentName { get; }
        bool IsCheckForNull();
    }

    internal struct PreconditionMethodCallExpression : IPreconditionExpression
    {
        public IClrTypeName CallSiteType { get; private set; }
        public string MethodName { get; private set; }
        public string ArgumentName { get; private set; }

        public bool IsCheckForNull()
        {
            return (CallSiteType.FullName == typeof (string).FullName &&
                    (MethodName == "IsNullOrEmpty" || MethodName == "IsNullOrWhiteSpace"));
        }

        public static PreconditionMethodCallExpression? TryCreate(IUnaryOperatorExpression expression)
        {
            Contract.Requires(expression != null);

            // Looking for constructs like !string.IsNullOrEmpty or !string.IsNullOrWhitespace
            // TODO: add test case: (!(string.IsNullOrEmpty))

            if (expression.UnaryOperatorType != UnaryOperatorType.EXCL)
            {
                return null;
            }

            var invocationExpression = expression.Operand as IInvocationExpression;
            if (invocationExpression == null)
                return null;

            var callSiteType = invocationExpression.GetCallSiteType();
            var method = invocationExpression.GetCalledMethod();
            var argument = invocationExpression.Arguments.FirstOrDefault()
                .With(x => x.Value as IReferenceExpression)
                .With(x => x.NameIdentifier.Name);

            if (callSiteType == null || method == null || argument == null)
                return null;

            return new PreconditionMethodCallExpression
            {
                CallSiteType = callSiteType,
                MethodName = method,
                ArgumentName = argument
            };
        }

    }

    internal struct PreconditionEqualityExpression : IPreconditionExpression
    {
        public string ArgumentName { get; private set; }
        public EqualityExpressionType EqualityType { get; private set; }
        public ICSharpLiteralExpression RightHandSide { get; private set; }

        public bool IsCheckForNull()
        {
            return EqualityType == EqualityExpressionType.NE
                   && RightHandSide.Literal.GetText() == "null";
        }

        public static PreconditionEqualityExpression? TryCreate(IEqualityExpression expression)
        {
            Contract.Requires(expression != null);

            var left = expression.LeftOperand as IReferenceExpression;

            var right = expression.RightOperand as ICSharpLiteralExpression;

            if (left == null || right == null)
                return null;

            // The problem is, that for "person.Name != null" and
            // for "person != null" I should get "person"
            var qualifierReference = left.QualifierExpression
                .With(x => x as IReferenceExpression);

            string predicateArgument = (qualifierReference ?? left).NameIdentifier.Name;

            return new PreconditionEqualityExpression
            {
                ArgumentName = predicateArgument,
                EqualityType = expression.EqualityType,
                RightHandSide = right,
            };
        }

        public static IEnumerable<IPreconditionExpression> Process(IExpression expression)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<IEnumerable<IPreconditionExpression>>() != null);

            return TraverseEqualityExpressions(expression)
                .Select(TryCreate)
                .Where(e => e != null)
                .Select(e => e.Value as IPreconditionExpression)
                .Concat(
                    TraverseUnaryExpressions(expression)
                        .Select(PreconditionMethodCallExpression.TryCreate)
                        .Where(e => e != null)
                        .Select(e => e.Value as IPreconditionExpression));
        }

        private static IEnumerable<IEqualityExpression> TraverseEqualityExpressions(IExpression expression)
        {
            var processor = new Procesor<IEqualityExpression>();
            expression.ProcessThisAndDescendants(processor);
            return processor.ProcessedNodes.OfType<IEqualityExpression>();
        }

        private static IEnumerable<IUnaryOperatorExpression> TraverseUnaryExpressions(IExpression expression)
        {
            var processor = new Procesor<IUnaryOperatorExpression>();
            expression.ProcessThisAndDescendants(processor);
            return processor.ProcessedNodes.OfType<IUnaryOperatorExpression>();
        }

        private class Procesor<T> : IRecursiveElementProcessor where T : IExpression
        {
            public Procesor()
            {
                ProcessedNodes = new List<ITreeNode>();
            }

            public List<ITreeNode> ProcessedNodes { get; private set; } 
            public bool InteriorShouldBeProcessed(ITreeNode element)
            {
                if (element is T)
                    return false;

                return true;
            }

            public void ProcessBeforeInterior(ITreeNode element)
            {
                ProcessedNodes.Add(element);
            }

            public void ProcessAfterInterior(ITreeNode element)
            {
                
            }

            public bool ProcessingIsFinished { get; private set; }
        }
    }

    /// <summary>
    /// Represents "precondition check" such as Contract.Requires or Contract.Invariant.
    /// </summary>
    /// <remarks>
    /// Every valid precondtion contains following:
    /// Contract.Requires(arg != null);
    /// So the parsed version contains:
    /// Precondition type ("Requires
    /// " in this case)
    /// Predicate Left Hand Side ("arg" in this case)
    /// Predicate Equality Type ("!=" in this case) and
    /// Predicate Right Hand Side ("null" in this case)
    /// </remarks>
    internal struct PreconditionExpression
    {
        private bool _isValid;
        private PreconditionType? _preconditionType;
        //private string _predicateArgument;
        private string _message;

        private List<IPreconditionExpression> _preconditionExpressions;
        //private IReferenceExpression _predicateLeftSide;
        //private EqualityExpressionType _predicateEqualityType;
        //private ILiteralExpression _predicateRightSide;


        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsValid || _preconditionExpressions != null);
            Contract.Invariant(!IsValid || _preconditionExpressions.Count != 0);
        }

        public static PreconditionExpression Parse(IInvocationExpression invocationExpression)
        {
            // TODO: potential enhancement: simplify condition first and convert !(result == null)
            Contract.Requires(invocationExpression != null);

            PreconditionType? preconditionType = GetPreconditionType(invocationExpression);
            if (preconditionType == null)
                return CreateInvalid();

            Contract.Assert(invocationExpression.Arguments.Count != 0,
                "Precondition expression should have at least one argument!");

            IExpression originalExpression = invocationExpression.Arguments[0].Expression;

            var preconditions = PreconditionEqualityExpression
                .Process(originalExpression)
                .Where(p => p.IsCheckForNull()).ToList();

            if (preconditions.Count == 0)
                return CreateInvalid();

            string message = ExtractMessage(invocationExpression);

            var result = new PreconditionExpression()
            {
                _isValid = true,

                _preconditionType = preconditionType,
                _preconditionExpressions = preconditions,

                _message = message,
            };

            // small hack: trigering object invariant
            result.CheckObjectInvariant();

            return result;
        }

        private static PreconditionType? GetPreconditionType(IInvocationExpression invocationExpression)
        {
            var clrType = invocationExpression.GetCallSiteType();
            var method = invocationExpression.GetCalledMethod();

            if (clrType.Return(x => x.FullName) != typeof (Contract).FullName)
                return null;

            return ParsePreconditionType(method);
        }

        private static PreconditionType? ParsePreconditionType(string method)
        {
            PreconditionType result;
            if (Enum.TryParse(method, true, out result))
                return result;

            return null;
        }

        private static string ExtractMessage(IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);

            Contract.Assert(invocationExpression.Arguments.Count != 0);

            var message = invocationExpression.Arguments.Skip(1).FirstOrDefault()
                .With(x => x.Expression as ICSharpLiteralExpression)
                .With(x => x.Literal.GetText());
            return message;
        }

        public bool IsValid
        {
            get { return _isValid; }
        }

        public ReadOnlyCollection<IPreconditionExpression> PreconditionExpressions
        {
            get
            {
                return new ReadOnlyCollection<IPreconditionExpression>(_preconditionExpressions);
            }
        }

        public PreconditionType? PreconditionType
        {
            get { return _preconditionType; }
        }

        public string Message
        {
            get { return _message; }
        }

        private static PreconditionExpression CreateInvalid()
        {
            return new PreconditionExpression() { _isValid = false };
        }

        internal void CheckObjectInvariant()
        { }
    }
}