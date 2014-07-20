using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    Contract.Assert(false);
    Contract.Assume(false);
    Contract.Requires(s != null);
    Contract.Ensures(false);
  }
}