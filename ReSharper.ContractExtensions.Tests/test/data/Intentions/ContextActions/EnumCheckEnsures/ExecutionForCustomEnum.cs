namespace CustomNamespace 
{
  enum Foo
  {
    Value1,
  }

  abstract class A
  {
    Foo{caret} EnableOnCustomEnum()
    {
      throw new System.NotImplementedException();
    }
  }
}