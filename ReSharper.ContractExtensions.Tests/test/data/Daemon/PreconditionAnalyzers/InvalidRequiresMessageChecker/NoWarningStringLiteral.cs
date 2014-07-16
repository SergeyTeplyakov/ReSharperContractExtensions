using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    Contract.Requires(s != null, "message");
  }
}