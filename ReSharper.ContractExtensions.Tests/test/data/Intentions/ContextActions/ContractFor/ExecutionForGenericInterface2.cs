internal interface ISomeInterface{caret}<T, U> where T : class, new() where U : struct
{
  void MethodWithPrecondition(T t);
  U MethodWithPostcondition();
  T PropertyWithPostcondition { get; }
}
