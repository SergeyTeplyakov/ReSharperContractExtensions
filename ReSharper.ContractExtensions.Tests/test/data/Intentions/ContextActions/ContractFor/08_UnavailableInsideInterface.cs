
internal interface ISomeInterface
{
  void MethodWithPrecondition{off}(string s);
  string MethodWithPostcondition();
  string PropertyWithPostcondition { get; }
}