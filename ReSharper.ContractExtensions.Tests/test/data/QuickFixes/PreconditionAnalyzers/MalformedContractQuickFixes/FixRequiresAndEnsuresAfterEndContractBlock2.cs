using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    Contract.EndContractBlock();
    Contract.Requires(false);
    Contract.EndContractBlock();
    {caret}Contract.Requires(s != null);
    Contract.Ensures(false);
  }
}