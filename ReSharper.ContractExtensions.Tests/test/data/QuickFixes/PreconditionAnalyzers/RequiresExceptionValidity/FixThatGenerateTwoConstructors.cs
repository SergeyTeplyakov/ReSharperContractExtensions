using System.Diagnostics.Contracts;
using System;

public class A
{
  public void Foo(string s)
  {
    {caret}Contract.Requires<CustomException>(s != null);
  }
}

public class CustomException : ArgumentException
{}