using System;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    internal enum ErrorType
    {
        CodeContractError,
        CodeContractWarning,
        CustomWarning,
        NoError
    }

    /// <summary>
    /// Represents result of validation process for a statements in the conract block.
    /// </summary>
    /// <remarks>
    /// This type is actually a discriminated union, with a set of potential combinations, like
    /// if ErrorType i CodeContractError then MalformedContractError property should be used.
    /// But for now I don't want more complicated implementations!
    /// </remarks>
    [ContractClass(typeof (ValidationResultContract))]
    internal abstract class ValidationResult
    {
        private readonly ICSharpStatement _statement;
        protected ValidationResult(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            _statement = statement;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Statement != null);
        }

        public T Match<T>(
            Func<NoErrorValidationResult, T> noErrorMatch,
            Func<CodeContractErrorValidationResult, T> errorMatch,
            Func<CodeContractWarningValidationResult, T> warningMatch)
        {
            Contract.Requires(noErrorMatch != null);
            Contract.Requires(errorMatch != null);
            Contract.Requires(warningMatch != null);

            var noErrorResult = this as NoErrorValidationResult;
            if (noErrorResult != null)
                return noErrorMatch(noErrorResult);

            var errorResult = this as CodeContractErrorValidationResult;
            if (errorResult != null)
                return errorMatch(errorResult);

            var warningResult = this as CodeContractWarningValidationResult;
            if (warningResult != null)
                return warningMatch(warningResult);

            Contract.Assert(false, "Unknown validation result type: " + GetType());
            throw new InvalidOperationException("Unknown validation result type: " + GetType());
        }

        public abstract ErrorType ErrorType { get; }

        public string GetErrorText()
        {
            return DoGetErrorText(GetEnclosingMethodName());
        }

        protected abstract string DoGetErrorText(string methodName);

        public ICSharpStatement Statement { get { return _statement; } }

        private string GetEnclosingMethodName()
        {
            return Statement.GetContainingTypeMemberDeclaration().DeclaredName;
        }

        public static ValidationResult CreateNoError(ICSharpStatement statement)
        {
            return new NoErrorValidationResult(statement);
        }

        public static ValidationResult CreateError(ICSharpStatement statement, MalformedContractError error)
        {
            return new CodeContractErrorValidationResult(statement, error);
        }

        public static ValidationResult CreateWarning(ICSharpStatement statement, MalformedContractWarning warning)
        {
            return new CodeContractWarningValidationResult(statement, warning);
        }
    }

    internal sealed class NoErrorValidationResult : ValidationResult
    {
        public NoErrorValidationResult(ICSharpStatement statement) : base(statement)
        { }

        public override ErrorType ErrorType { get { return ErrorType.NoError; } }

        protected override string DoGetErrorText(string methodName)
        {
            return string.Empty;
        }
    }

    internal sealed class CodeContractErrorValidationResult : ValidationResult
    {
        public CodeContractErrorValidationResult(ICSharpStatement statement, MalformedContractError error) : base(statement)
        {
            Error = error;
        }

        public MalformedContractError Error { get; private set; }

        public override ErrorType ErrorType { get { return ErrorType.CodeContractError; } }

        protected override string DoGetErrorText(string methodName)
        {
            return Error.GetErrorText(methodName);
        }
    }

    internal sealed class CodeContractWarningValidationResult : ValidationResult
    {
        public CodeContractWarningValidationResult(ICSharpStatement statement, MalformedContractWarning warning)
            : base(statement)
        {
            Warning = warning;
        }

        public override ErrorType ErrorType { get { return ErrorType.CodeContractWarning; } }

        public MalformedContractWarning Warning { get; private set; }

        protected override string DoGetErrorText(string methodName)
        {
            return Warning.GetErrorText(methodName);
        }
    }


    [ContractClassFor(typeof(ValidationResult))]
    abstract class ValidationResultContract : ValidationResult
    {
        protected ValidationResultContract(ICSharpStatement statement) : base(statement)
        { }

        protected override string DoGetErrorText(string methodName)
        {
            Contract.Requires(!string.IsNullOrEmpty(methodName));
            Contract.Ensures(Contract.Result<string>() != null);

            throw new NotImplementedException();
        }
    }
}