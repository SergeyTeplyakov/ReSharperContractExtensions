#define CONTRACTS_FULL
using System.Collections.Generic;
using System.Diagnostics.Contracts;

class A
{
  private IEnumerable<int> FooWithArgCheck(string s)
  {
    if (s == null) throw new System.ArgumentNullException("s");
    yield break;
  }

  public IEnumerable<int> FooWithLegacyContract(string s)
  {
    if (s == null) throw new System.ArgumentNullException("s");
    Contract.EndContractBlock();
    yield break;
  }

  public IEnumerable<int> Foo(string s)
  {
    Contract.Requires(s != null);
    yield break;
  }
}