using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    Contract.EndContractBlock();
    {caret}Contract.Requires(s != null);
    Contract.Requires(false);
    Contract.Ensures(false);
  }
}