using System;
using System.Diagnostics.Contracts;

class Person
{
  public string Name {get; set;}
}


abstract class A
{  
  public void DisabledBecauseAlreadyCheckedInFirstComplex(string s1, string s2{off}) 
  {
    Contract.Requires((s1 != null || s1.Length == 0) && s2 != null);
  }

}