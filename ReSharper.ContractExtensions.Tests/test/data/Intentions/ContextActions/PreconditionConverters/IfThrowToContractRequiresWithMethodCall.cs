class A
{
  public void EnabledOnAbstractMethod(string s)
  {
    {caret}if (string.IsNullOrEmpty(s)) throw new System.ArgumentNullException("s");
  }
}