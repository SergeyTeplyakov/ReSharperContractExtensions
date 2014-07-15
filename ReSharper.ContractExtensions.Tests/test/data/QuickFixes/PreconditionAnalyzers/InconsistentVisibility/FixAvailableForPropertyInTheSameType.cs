using System.Diagnostics.Contracts;

class A
{
  protected void Foo()
  {
    Contract.Requires(IsValid);
  }

  private bool IsValid {get; private set;}
}
