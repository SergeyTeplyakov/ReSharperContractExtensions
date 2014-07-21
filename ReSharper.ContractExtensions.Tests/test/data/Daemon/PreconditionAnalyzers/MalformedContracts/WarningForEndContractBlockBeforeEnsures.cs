using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    Contract.EndContractBlock();
    Contract.Ensures(s != null);
  }
}