using System;
using System.Diagnostics.Contracts;

class Person
{
  public string Name {get; set;}
}


abstract class A
{  
  void EnabledOnStringArgumentWithNotNullDefault(string s{off} = null) {}
}