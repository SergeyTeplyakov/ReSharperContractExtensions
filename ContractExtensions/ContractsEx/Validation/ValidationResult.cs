using System;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions.Statements;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    /// <summary>
    /// List of validation error types (simplifies "pattern-matching".
    /// </summary>
    public enum ErrorType
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
    public abstract class ValidationResult
    {
        private readonly ICSharpStatement _statement;
        private ProcessedStatement _processedStatement;

        protected ValidationResult(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);
            _statement = statement;
        }

        public T Match<T>(
            Func<CodeContractErrorValidationResult, T> errorMatch,
            Func<CodeContractWarningValidationResult, T> warningMatch,
            Func<ValidationResult, T> defaultMatch)
        {
            Contract.Requires(errorMatch != null);
            Contract.Requires(warningMatch != null);
            Contract.Requires(defaultMatch != null);

            var errorResult = this as CodeContractErrorValidationResult;
            if (errorResult != null)
                return errorMatch(errorResult);

            var warningResult = this as CodeContractWarningValidationResult;
            if (warningResult != null)
                return warningMatch(warningResult);

            return defaultMatch(this);
        }
        
        public T Match<T>(
            Func<CodeContractErrorValidationResult, T> errorMatch,
            Func<CodeContractWarningValidationResult, T> warningMatch,
            Func<CustomWarningValidationResult, T> customWarningMatch,
            Func<ValidationResult, T> defaultMatch)
        {
            Contract.Requires(errorMatch != null);
            Contract.Requires(warningMatch != null);
            Contract.Requires(customWarningMatch != null);
            Contract.Requires(defaultMatch != null);

            var errorResult = this as CodeContractErrorValidationResult;
            if (errorResult != null)
                return errorMatch(errorResult);

            var warningResult = this as CodeContractWarningValidationResult;
            if (warningResult != null)
                return warningMatch(warningResult);
            
            var customWarning = this as CustomWarningValidationResult;
            if (customWarning != null)
                return customWarningMatch(customWarning);

            return defaultMatch(this);
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

        public ICSharpStatement Statement
        {
            get
            {
                Contract.Ensures(Contract.Result<ICSharpStatement>() != null);
                return _statement;
            }
        }

        public ProcessedStatement ProcessedStatement
        {
            get
            {
                Contract.Ensures(Contract.Result<ProcessedStatement>() != null);
                return _processedStatement;
            }
        }

        internal void SetProcessedStatement(ProcessedStatement processedStatement)
        {
            Contract.Requires(processedStatement != null);
            Contract.Requires(processedStatement.CSharpStatement == Statement,
                "Processed statement should have the same CSharpStatement that you passed to the constructor!");

            _processedStatement = processedStatement;
        }

        public string GetEnclosingMethodName()
        {
            return Statement.GetContainingTypeMemberDeclaration().DeclaredName;
        }

        public static ValidationResult CreateNoError(ICSharpStatement statement)
        {
            return new NoErrorValidationResult(statement);
        }

        public static ValidationResult CreateError(ICSharpStatement statement, MalformedContractError error, string message = null)
        {
            return new CodeContractErrorValidationResult(statement, error, message);
        }

        public static ValidationResult CreateWarning(ICSharpStatement statement, MalformedContractWarning warning)
        {
            return new CodeContractWarningValidationResult(statement, warning);
        }

        public static ValidationResult CreateCustomWarning(ICSharpStatement statement,
            MalformedContractCustomWarning customWarning)
        {
            return new CustomWarningValidationResult(statement, customWarning);
        }
    }

    /// <summary>
    /// Represents successful validation.
    /// </summary>
    public sealed class NoErrorValidationResult : ValidationResult
    {
        public NoErrorValidationResult(ICSharpStatement statement) : base(statement)
        { }

        public override ErrorType ErrorType { get { return ErrorType.NoError; } }

        protected override string DoGetErrorText(string methodName)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Error based on Code Contract compiler rules.
    /// </summary>
    public sealed class CodeContractErrorValidationResult : ValidationResult
    {
        private readonly string _message;

        public CodeContractErrorValidationResult(ICSharpStatement statement, MalformedContractError error, string message) 
            : base(statement)
        {
            _message = message;
            Error = error;
        }

        public MalformedContractError Error { get; private set; }

        public override ErrorType ErrorType { get { return ErrorType.CodeContractError; } }

        protected override string DoGetErrorText(string methodName)
        {
            return _message ?? Error.GetErrorText(methodName);
        }
    }

    /// <summary>
    /// Warning based on Code Contract compiler rules.
    /// </summary>
    public sealed class CodeContractWarningValidationResult : ValidationResult
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

    public sealed class CustomWarningValidationResult : ValidationResult
    {
        private readonly MalformedContractCustomWarning _customWarning;

        public CustomWarningValidationResult(ICSharpStatement statement, 
            MalformedContractCustomWarning customWarning) 
            : base(statement)
        {
            _customWarning = customWarning;
        }

        public override ErrorType ErrorType
        {
            get { return ErrorType.CustomWarning; }
        }

        public MalformedContractCustomWarning Warning
        {
            get { return _customWarning; }
        }

        protected override string DoGetErrorText(string methodName)
        {
            return _customWarning.GetErrorText(methodName);
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