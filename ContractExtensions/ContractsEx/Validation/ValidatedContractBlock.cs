using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using ReSharper.ContractExtensions.ContractsEx.Assertions.Statements;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    /// <summary>
    /// Represents validated block of contract statements.
    /// </summary>
    internal sealed class ValidatedContractBlock
    {
        private readonly IList<ValidatedStatement> _validatedContractBlock;
        private readonly IList<ProcessedStatement> _contractBlock;
        private readonly IList<ValidationResult> _validationResults;

        public ValidatedContractBlock(IList<ValidatedStatement> validatedContractBlock)
        {
            Contract.Requires(validatedContractBlock != null);

            _validatedContractBlock = validatedContractBlock;

            _contractBlock = validatedContractBlock.Select(x => x.ProcessedStatement).ToList();
            _validationResults = validatedContractBlock.SelectMany(x => x.ValidationResults).ToList();
        }

        public ReadOnlyCollection<ValidatedStatement> ValidatedBlock
        {
            get { return new ReadOnlyCollection<ValidatedStatement>(_validatedContractBlock); }
        } 

        public ReadOnlyCollection<ProcessedStatement> ContractBlock
        {
            get { return new ReadOnlyCollection<ProcessedStatement>(_contractBlock); }
        }

        public ReadOnlyCollection<ValidationResult> ValidationResults
        {
            get { return new ReadOnlyCollection<ValidationResult>(_validationResults); }
        }
    }

    /// <summary>
    /// Represents validated statement with a list of validated errors.
    /// </summary>
    internal sealed class ValidatedStatement
    {
        private readonly ProcessedStatement _processedStatement;
        private readonly IList<ValidationResult> _validationResults;

        public ValidatedStatement(ProcessedStatement processedStatement, IList<ValidationResult> validationResults)
        {
            Contract.Requires(processedStatement != null);
            Contract.Requires(validationResults != null);

            _processedStatement = processedStatement;
            _validationResults = validationResults.Count == 0 ? new[] { ValidationResult.CreateNoError(processedStatement.CSharpStatement) } : validationResults;
        }

        public ProcessedStatement ProcessedStatement
        {
            get
            {
                Contract.Ensures(Contract.Result<ProcessedStatement>() != null);
                return _processedStatement;
            }
        }

        public IList<ValidationResult> ValidationResults
        {
            get
            {
                Contract.Ensures(Contract.Result<IList<ValidationResult>>() != null);
                Contract.Ensures(Contract.Result<IList<ValidationResult>>().Count != 0);
                return _validationResults;
            }
        }
    }
}