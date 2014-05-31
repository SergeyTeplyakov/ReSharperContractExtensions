abstract class A
{
  public string Foo {set{on} {}}
 
  public string AvailableOnTheDeclaration{on} {set{}}

  public string UnavailableOnAutoProperty{off} {get; private set;}
  public string UnavailableOnGetterOnly{off} {get {return "";}}
}