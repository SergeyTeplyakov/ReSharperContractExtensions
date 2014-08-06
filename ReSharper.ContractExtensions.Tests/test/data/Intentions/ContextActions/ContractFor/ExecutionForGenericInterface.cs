internal interface ISomeInterface{caret}<T>
{
  void MethodWithPrecondition(T t);
  T MethodWithPostcondition();
  T PropertyWithPostcondition { get; }
}