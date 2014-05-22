abstract class SomeClass{caret}
{
  public abstract void MethodWithPrecondition(string s);
  public abstract string MethodWithPostcondition();
  public abstract string PropertyWithPostcondition { get; }
  protected abstract void ProtectedMethodWithPrecondition(string s);
}