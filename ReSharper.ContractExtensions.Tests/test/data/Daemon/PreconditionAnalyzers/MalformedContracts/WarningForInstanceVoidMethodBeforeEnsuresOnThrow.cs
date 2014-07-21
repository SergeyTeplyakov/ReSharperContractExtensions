using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    InstanceMethod();
    Contract.EnsuresOnThrow<Exception>(false);
  }
  private void InstanceMethod() {}
}