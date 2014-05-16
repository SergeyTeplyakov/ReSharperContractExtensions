
internal interface ISomeInterface{on}
{
  void MethodWithPrecondition(string s);
  string MethodWithPostcondition();
  string PropertyWithPostcondition { get; }
}