using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    try
    {
      Contract.Requires(s != null);
    }
    finally {}
  }
}