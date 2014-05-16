internal interface ISomeInterface{caret}
{
  void MethodWithPrecondition(string s);
  string MethodWithPostcondition();
  string PropertyWithPostcondition { get; }
}