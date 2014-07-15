using System.Diagnostics.Contracts;

public class A
{
  public void Foo(string s)
  {
    Contract.Requires(!s.IsNullOrEmpty());
  }
}

static class StringExtensions
{
  public static bool IsNullOrEmpty(this string s)
  {
    return string.IsNullOrEmpty(s);
  }
}