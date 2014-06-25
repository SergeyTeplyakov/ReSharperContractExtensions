namespace CustomNamespace 
{
  enum Foo
  {
    Value1,
  }

  abstract class A
  {
    void EnableOnCustomEnum(Foo foo{caret})
    {}
  }
}