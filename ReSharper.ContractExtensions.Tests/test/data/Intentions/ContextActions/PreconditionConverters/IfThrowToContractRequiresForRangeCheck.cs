class A
{
  public void EnabledOnAbstractMethod(int n)
  {
    {caret}if (n <= 0 || n >= 42) throw new System.ArgumentOutOfRangeException("n", "n should be from 1 to 41!");
  }
}