using System.Diagnostics.Contracts;

class A
{
  private string _anotherString = "";
  private string _shouldNotBeNull{caret} = "";

  public A()
  {
  }

  [ContractInvariantMethod]
  private void ObjectInvariant()
  {
    Contract.Invariant(_anotherString != null);
  }
}