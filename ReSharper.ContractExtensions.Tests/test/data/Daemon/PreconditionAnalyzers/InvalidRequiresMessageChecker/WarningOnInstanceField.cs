using System.Diagnostics.Contracts;

class A
{
  string Message = "message";
  public void Foo(string s)
  {
    Contract.Requires(s != null, Message);
  }
}