using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    Contract.Ensures(false);
    Contract.Requires(s != null);
  }
}