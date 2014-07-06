using System.Diagnostics.Contracts;

class A
{
  public void Foo([JetBrains.Annotations.CanBeNull]string s)
  {
    Contract.Requires(s != null);
  }
}