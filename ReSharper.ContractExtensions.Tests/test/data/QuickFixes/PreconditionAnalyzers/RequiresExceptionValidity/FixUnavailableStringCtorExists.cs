using System.Diagnostics.Contracts;
using System;

public class A
{
  public void Foo(string s)
  {
    Contract.Requires<CustomException>(s != null);
  }
}

public class CustomExceptionBase : Exception
{
  private CustomException(string msg) {}
  public CustomException() {}
}

public class CustomException : CustomExceptionBase
{
}