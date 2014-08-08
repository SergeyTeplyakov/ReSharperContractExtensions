class A
{
  private string GetMessage() { return "foo"; }
  public void EnabledOnAbstractMethod(string s)
  {
    {caret}if (s == null) throw new System.ArgumentNullException("s", GetMessage());
  }
}