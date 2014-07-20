using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    InstanceMethod();
    {
      Contract.Requires(false);
    }
  }
  private void InstanceMethod() {}
}