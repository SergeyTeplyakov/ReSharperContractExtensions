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
    Contract.Requires(IsValid);
  }
}
