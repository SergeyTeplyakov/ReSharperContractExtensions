using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    StaticMethod();
    Contract.Ensures(false);
  }
  private static void StaticMethod() {}
}