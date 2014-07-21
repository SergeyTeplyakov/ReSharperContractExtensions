using System.Diagnostics.Contracts;

class A
{
  private static string _foo;
  public void Foo(string s)
  {
    {caret}_foo = s;
    StaticMethod();
    Contract.Assert(false);
    Contract.Requires(s != null);
    Contract.Assume(false);
    Contract.Ensures(false);
  }
  private static bool StaticMethod() {return false;}
}