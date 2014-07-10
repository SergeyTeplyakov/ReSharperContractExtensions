using System.Diagnostics.Contracts;
using System;

abstract class A
{
  public void EnableOnRangeCheck(int n)
  {
    {on}if (n <= 0 && n >= 42) throw new System.ArgumentOutOfRangeException("n", "n should be from 1 to 41!");
  }
}