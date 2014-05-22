using System.Linq;
using JetBrains.ReSharper.Feature.Services.CSharp.Generate;
using JetBrains.ReSharper.Feature.Services.Generate;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Util;
using JetBrains.Util;

namespace ReSharper.ContractExtensions.ContextActions.ContractsFor.Generator
{
    [GeneratorElementProvider("ContractClass", typeof(CSharpLanguage))]
    internal class ContractClassProvider : GeneratorProviderBase<CSharpGeneratorContext>
    {
        public override double Priority
        {
            get { return 0; }
        }

        /// <summary>
        /// Populate context with input elements
        /// </summary>
        /// <param name="context"></param>
        public override void Populate(CSharpGeneratorContext context)
        {
            var typeElement = context.ClassDeclaration.DeclaredElement;
            if (typeElement == null)
                return;

            if (!(typeElement is IStruct) && !(typeElement is IClass))
                return;

            // provide only readable properies
            var stuff =
                context.ClassDeclaration.PropertyDeclarations.Select(
                    member => new { member, memberType = member.Type as IDeclaredType }).Where(
                        t => !t.member.IsStatic &&
                             !t.member.IsAbstract &&
                             t.member.DeclaredElement.IsReadable && // must be readable
                             !t.member.IsSynthetic() &&
                             t.memberType != null &&
                             CSharpTypeUtil.CanUseExplicitly(t.memberType, context.ClassDeclaration))
                    .Select(u => new GeneratorDeclaredElement<ITypeOwner>(u.member.DeclaredElement));

            context.ProvidedElements.AddRange(stuff);
        }
    }
}