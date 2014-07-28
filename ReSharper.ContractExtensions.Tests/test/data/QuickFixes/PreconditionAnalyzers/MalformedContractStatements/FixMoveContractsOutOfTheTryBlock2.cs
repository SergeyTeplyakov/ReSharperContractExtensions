using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    {
      Contract.Requires(s != null);
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