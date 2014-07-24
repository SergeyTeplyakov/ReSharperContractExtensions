using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    {
      try
      {
        Contract.Requires(false);
        {caret}Contract.Ensures(false);
        Contract.EndContractBlock();
      }
      finally {}
    }
  }
}