using System.Diagnostics.Contracts;

public class A
{
  public void Foo()
  {
    {caret}Contract.Requires(B.IsValid);
  }
}

class B
{
  internal static bool IsValid {get; private set;}
}