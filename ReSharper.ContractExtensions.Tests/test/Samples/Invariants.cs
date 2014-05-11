public class Invariants
{
    private readonly string _someString;

    public string StringPropWithGetter { get { return "42"; } }

    public string StringPropWithoutGetter { set { } }

    private void MethodThatUsesField()
    {
        System.Console.WriteLine(_someString);
    }

    private void MethodThatUsesProperty()
    {
        System.Console.WriteLine(StringPropWithGetter);
    }
}
