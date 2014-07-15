using System.Diagnostics.Contracts;

public class A
{
  public void Foo(string s)
  {
    Contract.Requires(!Inner.IsValid(s));
  }

  private class Inner
  {
    public static bool IsValid(string s) {return s != null;}
  }
}
