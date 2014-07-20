using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    InstanceMethod();
    StaticMethod();
    Contract.Requires(s != null);
    Contract.Ensures(false);
  }
  private void InstanceMethod() {}
  private void StaticMethod() {}
}