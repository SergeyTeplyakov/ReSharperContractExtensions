using System.Diagnostics.Contracts;

class A
{
  private static string _foo;
  public void Foo(string s)
  {
    _foo = s;
    Contract.Requires(s != null);
  }
}