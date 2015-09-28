using System.Diagnostics.Contracts;

public class A
{
  public void Foo(string s)
  {
    Contract.Requires<System.ArgumentNullException>(s != null);
  }
}