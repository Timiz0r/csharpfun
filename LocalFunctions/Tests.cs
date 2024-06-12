namespace csharpfun.LocalFunctions;

public class Tests
{
    [Test]
    public void StaticMethodImpl()
    {
        string raw = "foo=bar;baz=something;";
        Dictionary<string, string> expected = new() {
            { "foo", "bar" },
            { "baz", "something" }
        };
        IReadOnlyDictionary<string, string> actual = StringMapParser_StaticMethod.Parse(raw);
        Assert.That(actual, Is.EquivalentTo(expected));

        raw = "foo=bar;foo=baz;";
        Assert.That(() => StringMapParser_StaticMethod.Parse(raw), Throws.Exception.TypeOf<ParseException>());
    }

    [Test]
    public void LambdaImpl()
    {
        string raw = "foo=bar;baz=something;";
        Dictionary<string, string> expected = new() {
            { "foo", "bar" },
            { "baz", "something" }
        };
        IReadOnlyDictionary<string, string> actual = StringMapParser_Lambda.Parse(raw);
        Assert.That(actual, Is.EquivalentTo(expected));

        raw = "foo=bar;foo=baz;";
        Assert.That(() => StringMapParser_Lambda.Parse(raw), Throws.Exception.TypeOf<ParseException>());
    }

    [Test]
    public void LocalFunctionImpl()
    {
        string raw = "foo=bar;baz=something;";
        Dictionary<string, string> expected = new() {
            { "foo", "bar" },
            { "baz", "something" }
        };
        IReadOnlyDictionary<string, string> actual = StringMapParser_LocalFunc.Parse(raw);
        Assert.That(actual, Is.EquivalentTo(expected));

        raw = "foo=bar;foo=baz;";
        Assert.That(() => StringMapParser_LocalFunc.Parse(raw), Throws.Exception.TypeOf<ParseException>());
    }

    [Test]
    public void LocalFunctionClosureOnHeapImpl()
    {
        string raw = "foo=bar;baz=something;";
        Dictionary<string, string> expected = new() {
            { "foo", "bar" },
            { "baz", "something" }
        };
        IReadOnlyDictionary<string, string> actual = StringMapParser_LocalFuncClosureOnHeap.Parse(raw);
        Assert.That(actual, Is.EquivalentTo(expected));

        raw = "foo=bar;foo=baz;";
        Assert.That(() => StringMapParser_LocalFuncClosureOnHeap.Parse(raw), Throws.Exception.TypeOf<ParseException>());
    }
}