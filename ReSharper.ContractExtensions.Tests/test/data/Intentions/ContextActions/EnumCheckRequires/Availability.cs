using System;
using System.Diagnostics.Contracts;

enum Foo
{
  Value1,
}

abstract class A
{
  void EnableOnCustomEnum(Foo foo{on})
  {}

  void EnableOnDotNetEnum(System.Reflection.BindingFlags flags{on})
  {}

  void DisabledBecauseAlreadyChecked(System.Reflection.BindingFlags flags{off})
  {
    Contract.Requires(Enum.IsDefined(typeof(System.Reflection.BindingFlags), flags));
  }

  void DisabledBecauseAlreadyCheckedByIfTHrow(System.Reflection.BindingFlags flags{off})
  {
    if (!Enum.IsDefined(typeof(System.Reflection.BindingFlags), flags))
      throw new ArgumentException("flags")
  }

}