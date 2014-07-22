using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    {caret}Contract.Ensures(false);
    Contract.Requires(s != null);
    Contract.EnsuresOnThrow<Exception>(false);
  }
}