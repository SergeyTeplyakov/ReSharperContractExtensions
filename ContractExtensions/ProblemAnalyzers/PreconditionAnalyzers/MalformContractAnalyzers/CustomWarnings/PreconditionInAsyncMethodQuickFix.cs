// Not implemented yet!!

//using System;
//using System.Diagnostics.Contracts;
//using JetBrains.Application.Progress;
//using JetBrains.ProjectModel;
//using JetBrains.ReSharper.Feature.Services.Bulbs;
//using JetBrains.ReSharper.Intentions.Extensibility;
//using JetBrains.ReSharper.Psi.CSharp.Tree;
//using JetBrains.ReSharper.Refactorings.CSharp.ExtractMethod2.FromStatements;
//using JetBrains.ReSharper.Refactorings.Workflow;
//using JetBrains.TextControl;
//using JetBrains.Util;

//namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
//{
//    [QuickFix]
//    public class PreconditionInAsyncMethodQuickFix : QuickFixBase
//    {
//        private readonly PreconditionInAsyncMethodHighlighting _highlighting;

//        public PreconditionInAsyncMethodQuickFix(PreconditionInAsyncMethodHighlighting highlighting)
//        {
//            Contract.Requires(highlighting != null);

//            _highlighting = highlighting;
            
//        }

//        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
//        {
//            var contractBlock = ((ICodeContractFixableIssue) _highlighting).ValidatedContractBlock;
//            var method = _highlighting.ValidationResult.Statement.GetContainingNode<ICSharpFunctionDeclaration>();

//            var flow = new CSharpExtractMethodFromStatementsWorkflow(null, "foo");
//            //flow.Execute()
//            throw new NotImplementedException();
//        }

//        public override string Text
//        {
//            get { return string.Format("Split precondition and async method (extract Do{0})", _highlighting.MethodName); }
//        }

//        public override bool IsAvailable(IUserDataHolder cache)
//        {
//            return true;
//        }
//    }
//}