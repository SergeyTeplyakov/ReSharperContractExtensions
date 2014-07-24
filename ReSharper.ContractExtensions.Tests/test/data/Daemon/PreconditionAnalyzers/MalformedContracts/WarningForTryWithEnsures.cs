using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    try
    {
      Contract.Ensures(s != null);
    }
    finally {}
  }
}