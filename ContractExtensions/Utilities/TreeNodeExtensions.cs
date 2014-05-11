using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.ContractExtensions.Utilities
{
    public static class TreeNodeExtensions
    {
        public static bool Is<T>(this ITreeNode treeNode) where T : class
        {
            return ((treeNode as T) != null);
        }
    }
}