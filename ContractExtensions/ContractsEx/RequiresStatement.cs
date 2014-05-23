using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;

namespace ReSharper.ContractExtensions.ContractsEx
{
    internal class RequiresStatement : ContractStatement
    {
        private RequiresStatement(ICSharpStatement statement)
            : base(statement)
        {}

        public static RequiresStatement TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            // TODO: is it necessary? If so, move to "monadic" implementaion!
            var invokedExpression = AsInvocationExpression(statement);
            if (invokedExpression == null)
                return null;

            if (invokedExpression.Reference == null)
                return null;
            var resolution = invokedExpression.Reference.Resolve();
            if (resolution.ResolveErrorType != ResolveErrorType.OK)
                return null;
            if (invokedExpression.InvokedExpression == null)
                return null;

            var clrDeclaredElement = resolution.DeclaredElement as IClrDeclaredElement;
            Contract.Assert(clrDeclaredElement != null);

            var text = invokedExpression.InvokedExpression.GetText();

            if (clrDeclaredElement.Module.DisplayName != "mscorlib" && text != "Contract.Requires")
                return null;
            
            var argumentNames = invokedExpression.Arguments.Select(a => a.ArgumentName).ToList();
            if (invokedExpression.Arguments.Count == 0)
                return null;

            var compareArgument = invokedExpression.Arguments[0].Expression as IEqualityExpression;
            if (compareArgument == null)
                return null;

            var left = compareArgument.LeftOperand as IReferenceExpression;
            if (left == null)
                return null;

            var name = left.NameIdentifier.Name;
            var right = compareArgument.RightOperand as ICSharpLiteralExpression;
            if (right == null)
                return null;

            if (right.Literal.GetText() != "null")
                return null;

            // TODO: add comparing to Contract.Requires(!(s = null));
            if (compareArgument.EqualityType != EqualityExpressionType.NE)
                return null;

            //compareArgument.OperatorSign.GetTokenType() == 
            Contract.Assert(argumentNames.Count == 1 || argumentNames.Count == 2, 
                "Contract.Requires could have only one or two arguments.");

            string requiresMessage = argumentNames.Skip(1).FirstOrDefault();            
            
            var result = new RequiresStatement(statement);
            result.ArgumentName = name;
            result.Message = requiresMessage;

            return result;
        }

        public string ArgumentName { get; private set; }
        public string Message { get; private set; }
    }
}