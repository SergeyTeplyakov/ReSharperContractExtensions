using System.Diagnostics.Contracts;

class A
{
  public event EventHandler E;
  protected void Foo()
  {
    {caret}Contract.Requires(E != null);
  }
}