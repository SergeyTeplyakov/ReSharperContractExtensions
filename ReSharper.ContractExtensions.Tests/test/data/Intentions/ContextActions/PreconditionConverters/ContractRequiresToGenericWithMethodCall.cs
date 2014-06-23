using System.Diagnostics.Contracts;

class A
{
  public void EnabledOnAbstractMethod(string s)
  {
    Contract.Requires{caret}(!string.IsNullOrEmpty(s));
  }
}