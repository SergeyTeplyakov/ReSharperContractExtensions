using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    Contract.Requires(IsValid());
  }
  internal bool IsValid() {return true;}
}