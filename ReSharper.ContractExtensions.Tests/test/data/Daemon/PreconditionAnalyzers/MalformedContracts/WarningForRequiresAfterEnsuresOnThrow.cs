using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    Contract.EnsuresOnThrow<Exception>(false);
    Contract.Requires(s != null);
  }
}