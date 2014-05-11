using System.Diagnostics.Contracts;

class A
{
  string{caret} Foo(string s)
  {
    Contract.Requires(s != null);
  }
}