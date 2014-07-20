using System.Diagnostics.Contracts;

class Guard
{
  [ContractArgumentValidator]
  public void Validate() {}
}

class A
{
  public void Foo(string s)
  {
    Guard.Validate();
    Contract.Requires(s != null);
  }
}