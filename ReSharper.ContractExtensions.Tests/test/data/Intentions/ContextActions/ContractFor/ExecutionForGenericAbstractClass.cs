internal abstract class SomeClass{caret}<T, U> where T : class, new() where U : struct
{
  public abstract void MethodWithPrecondition(T t);
  protected abstract U MethodWithPostcondition();
  protected abstract T PropertyWithPostcondition { get; }
}
