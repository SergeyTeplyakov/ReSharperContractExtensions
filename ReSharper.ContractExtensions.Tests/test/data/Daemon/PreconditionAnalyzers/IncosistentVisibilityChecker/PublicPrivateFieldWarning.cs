using System.Diagnostics.Contracts;

class A
{
  private bool _isValid;
  public void Foo()
  {
    Contract.Requires(_isValid);
  }
}