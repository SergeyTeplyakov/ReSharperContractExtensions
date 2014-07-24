using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
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