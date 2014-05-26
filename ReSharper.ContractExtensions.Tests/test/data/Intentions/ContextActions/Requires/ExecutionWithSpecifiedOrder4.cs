using System.Diagnostics.Contracts;

class Person
{
  public string Name {get;set;}
}

class A
{
  void Foo2(Person person, string s1{caret})
  {
    Contract.Requires<System.ArgumentNullException>(person != null);
    Contract.Requires<System.ArgumentNullException>(person.Name != null);
  }
}