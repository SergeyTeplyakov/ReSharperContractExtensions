using System;

static class A
{
  public static string{on} EnabledOnReferenceTypeResult(this IConvertible c) { return c.ToString(); }
  public static int?{on} EnabledOnNullableReferenceTypeResult(this IConvertible c) { return 42; }
}