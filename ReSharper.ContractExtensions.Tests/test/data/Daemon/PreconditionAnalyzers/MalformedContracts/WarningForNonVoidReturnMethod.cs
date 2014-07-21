using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    InstanceMethod();
    Contract.Requires(s != null);
    Contract.Ensures(false);
  }
  private bool InstanceMethod() { return true; }
}