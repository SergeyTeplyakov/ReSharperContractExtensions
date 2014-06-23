class A
{
  public void EnabledOnAbstractMethod(string s)
  {
    {caret}if (s == null) throw new System.ArgumentNullException("s", "s should not be null");
  }
}