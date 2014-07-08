using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    Contract.Requires(IsValid);
  }
  private bool IsValid {get; private set;}
}