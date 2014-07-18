using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    InstanceMethod();
    Contract.Ensures(false);
  }
  private static void StaticMethod() {}
}