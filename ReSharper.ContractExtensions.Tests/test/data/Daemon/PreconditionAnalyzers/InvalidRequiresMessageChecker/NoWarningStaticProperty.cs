using System.Diagnostics.Contracts;

class A
{
  internal string Message {get {return "message";}}
  public void Foo(string s)
  {
    Contract.Requires(s != null, Message);
  }
}