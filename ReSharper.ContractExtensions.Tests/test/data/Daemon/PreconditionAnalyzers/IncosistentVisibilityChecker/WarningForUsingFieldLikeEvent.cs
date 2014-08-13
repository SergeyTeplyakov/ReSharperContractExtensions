using System.Diagnostics.Contracts;

class A
{
  public event EventHandler E;

  public void Foo()
  {
    Contract.Requires(E != null);
  }
}