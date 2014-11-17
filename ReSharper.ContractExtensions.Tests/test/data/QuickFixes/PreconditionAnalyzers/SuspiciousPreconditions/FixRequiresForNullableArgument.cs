using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s = null)
  {
    {caret}Contract.Requires(s != null);
  }
}