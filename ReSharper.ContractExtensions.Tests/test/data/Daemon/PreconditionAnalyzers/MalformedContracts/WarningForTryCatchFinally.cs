using System.Diagnostics.Contracts;

class A
{
  public void InTry(string s)
  {
    try
    {
      {
        Contract.Requires(s != null);
      }
    }
    catch(System.Exception e)
    {
      {
        Contract.Requires(false);
      }
    }
    finally 
    {
      {
        Contract.Requires(false);
      }
    }
  }
}