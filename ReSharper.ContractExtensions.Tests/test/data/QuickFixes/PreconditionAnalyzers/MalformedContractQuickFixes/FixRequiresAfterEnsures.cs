using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    Contract.Ensures(false);
    {caret}Contract.Requires(s != null);
    Contract.Requires(false);
  }
}