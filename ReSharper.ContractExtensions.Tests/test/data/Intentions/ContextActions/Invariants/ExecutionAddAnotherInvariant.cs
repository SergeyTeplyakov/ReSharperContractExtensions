using System.Diagnostics.Contracts;

class A
{
  private string _shouldNotBeNull{caret} = "";
  private string _anotherString = "";
  public A()
  {
  }

  [ContractInvariantMethod]
  private void ObjectInvariant()
  {
    Contract.Invariant(_anotherString != null);
  }
}