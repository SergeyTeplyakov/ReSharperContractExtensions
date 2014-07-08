using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    Contract.Requires(IsValid(s));
  }
  protected bool IsValid(string s) {return true;}
}