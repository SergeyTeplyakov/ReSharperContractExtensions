using System.Diagnostics.Contracts;

class A
{
  private static string Message = "msg";
  public void Foo(string s)
  {
    Contract.Requires(s != null, Message);
  }
}