class A
{
  public void EnabledOnAbstractMethod(string s)
  {
    {caret}if (s == null) throw new System.ArgumentException("s", "s should not be null");
  }
}