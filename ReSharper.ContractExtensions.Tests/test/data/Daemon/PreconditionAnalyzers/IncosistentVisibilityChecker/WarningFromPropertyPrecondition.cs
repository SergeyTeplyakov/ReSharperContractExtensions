using System.Diagnostics.Contracts;

class A
{
  public string Foo
  {
    get 
    {
      Contract.Requires(IsValid());
      return "";
    }
  }
  internal bool IsValid() {return true;}
}