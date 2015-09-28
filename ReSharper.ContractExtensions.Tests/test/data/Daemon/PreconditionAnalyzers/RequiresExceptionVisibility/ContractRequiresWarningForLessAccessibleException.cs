using System.Diagnostics.Contracts;

public class A
{
  public void Foo(string s)
  {
    Contract.Requires<CustomException>(s != null);
  }
}

class CustomException : System.ArgumentException
{
  public CustomException(string message, string paramName) : base(message, paramName) {}
}