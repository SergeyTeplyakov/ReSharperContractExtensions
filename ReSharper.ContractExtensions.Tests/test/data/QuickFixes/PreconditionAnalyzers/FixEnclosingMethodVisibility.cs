using System.Diagnostics.Contracts;

class A
{
  protected void Foo()
  {
    {caret}Contract.Requires(IsValid());
  }

  private bool IsValid() {return false;}
}