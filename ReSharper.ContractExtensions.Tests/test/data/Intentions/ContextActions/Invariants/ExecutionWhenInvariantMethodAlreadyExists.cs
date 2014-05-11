using System.Diagnostics.Contracts;

class A
{
  private string _shouldNotBeNull{caret} = "";
  public A()
  {
  }

  [ContractInvariantMethod]
  private void ObjectInvariant()
  {}

}