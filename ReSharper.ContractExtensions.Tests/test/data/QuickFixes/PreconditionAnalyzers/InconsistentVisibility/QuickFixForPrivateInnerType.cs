using System.Diagnostics.Contracts;

public class A
{
  public void Foo(string s)
  {
    {caret}Contract.Requires(!Inner.IsValid(s));
  }

  private class Inner
  {
    internal static bool IsValid(string s) {return s != null;}
  }
}
