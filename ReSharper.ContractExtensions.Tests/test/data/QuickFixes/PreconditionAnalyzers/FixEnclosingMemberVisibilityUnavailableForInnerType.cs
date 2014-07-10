using System.Diagnostics.Contracts;

class A
{
  protected void Foo()
  {
    {caret}Contract.Requires(B.IsValid);
  }

  class B
  {
    internal static bool IsValid {get; private set;}
  }
}

