#define CONTRACTS_FULL
using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    Contract.EndContractBlock();
    Contract.Ensures(false);
    Contract.Requires(s != null);
    Contract.EnsuresOnThrow<System.Exception>(false);
  }
}
