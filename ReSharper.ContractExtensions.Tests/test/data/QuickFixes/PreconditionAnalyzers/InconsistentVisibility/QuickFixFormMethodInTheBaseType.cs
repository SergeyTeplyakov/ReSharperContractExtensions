using System.Diagnostics.Contracts;

class B
{
  internal bool IsValid() {return true;}
}

class C : B {}

class A : C
{
  protected void Foo()
  {
    {caret}Contract.Requires(IsValid);
  }
}
