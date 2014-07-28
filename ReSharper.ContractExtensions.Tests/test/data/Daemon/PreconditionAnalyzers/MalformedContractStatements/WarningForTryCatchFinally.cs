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

  public void InTryCatch(string s)
  {
    try
    {
      Contract.Requires(s != null);
      Contract.Ensures(s != null);
      Contract.EnsuresOnThrow<System.Exception>(false);
      Contract.EndContractBlock();
      Contract.Assert(false);
      Contract.Assume(false);
    }
    catch {}
  }

  public void EnsureInFinally(string s)
  {
    try
    {
      Contract.Ensures(s != null);
    }
    finally {}
  }

  public void WithDoubleTry(string s)
  {
    {
      try
      {
        try
        {
            Contract.Requires(s != null);
        }
        catch
        { }
      }
      finally
      {
      }
    }
  }


}