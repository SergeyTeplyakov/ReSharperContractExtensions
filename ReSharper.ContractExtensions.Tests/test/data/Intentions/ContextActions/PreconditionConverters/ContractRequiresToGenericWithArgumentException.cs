using System.Diagnostics.Contracts;

class A
{
  public void EnabledOnAbstractMethod(int n)
  {
    Contract.Requires{caret}(n > 0);
  }
}