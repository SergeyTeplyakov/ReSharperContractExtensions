using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    try
    {
      Contract.Requires(s != null);
      Contract.Ensures(s != null);
      Contract.EnsuresOnThrow<System.Exception>(false);
      Contract.EndContractBlock();
      Contract.Assert(false);
      Contract.Assume(false);
    }
    catch {}
  }
}