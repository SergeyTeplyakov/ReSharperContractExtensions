using System.Diagnostics.Contracts;

class A
{
  public string Message {get; private set;}
  public void Foo(string s)
  {
    Contract.Requires(s != null, Message);
  }
}