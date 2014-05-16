using System.Diagnostics.Contracts;

class A
{
  private string _anotherString = "";

  public string SomeProperty{caret} {get; set;}
  public A()
  {
  }

  [ContractInvariantMethod]
  private void ObjectInvariant()
  {
    Contract.Invariant(_anotherString != null);
  }
}