#define CONTRACTS_FULL
using System.Diagnostics.Contracts;

class A
{
  public event System.EventHandler E;

  public void Foo()
  {
    Contract.Requires(E != null);
  }
}