using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    {caret}Contract.Requires<CustomException>(s != null);
  }

  protected class CustomException : ArgumentException
  {
    public CustomException(string message, string paramName) : base(message, paramName) {}
  }
}