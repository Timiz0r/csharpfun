# Local functions

## Introduction
Local functions are an often underused C# feature (at least from what I've come across),
even though they are one of the best features introduced in recent memory.
And by recent, I mean it was released around 7 years ago from the date of writing this in 2024!

Implemented are four contrived examples of parsing key-value pairs from a string into a dictionary:
* Using static methods
* Using lambdas
* Using local functions
* Using local functions, passed to something else

All code has been supplied and is runnable with `dotnet test`.

## Static methods
As we all know, it's not uncommon to break apart large methods into smaller ones to improve readability and organization.
If these smaller methods used state or other methods from an instance of the class,
they would of course also be instance methods. Otherwise, they would typically be static methods.
Furthermore, if the "parent" method has locals needed by the factored out methods, these locals would need to be passed.

Aside from being a contrived example, the below code is perfectly reasonable and was a common way of doing things
before local functions were added to the langauge.

```
using System.Text;

namespace csharpfun.LocalFunctions;

public class StringMapParser_StaticMethod
{
    public static IReadOnlyDictionary<string, string> Parse(string raw)
    {
        Dictionary<string, string> result = new();
        int index = 0;

        ParseImpl(raw, result, ref index);

        return result;
    }

    private static void ParseImpl(string raw, Dictionary<string, string> result, ref int index)
    {
        while (index < raw.Length)
        {
            ParsePair(raw, result, ref index);
            ReadDelimiter(';', raw, ref index);
        }
    }

    private static void ParsePair(string raw, Dictionary<string, string> result, ref int index)
    {
        string key = ReadString(raw, ref index);
        ReadDelimiter('=', raw, ref index);
        string value = ReadString(raw, ref index);

        if (!result.TryAdd(key, value))
        {
            throw new ParseException("Key already present.");
        }
    }

    private static void ReadDelimiter(char c, string raw, ref int index)
    {
        if (index >= raw.Length) throw new ParseException("Unexpectedly reached end of string.");
        if (c != raw[index]) throw new ParseException("Unexpected character found.");
        index++;
    }

    private static string ReadString(string raw, ref int index)
    {
        StringBuilder key = new StringBuilder();
        for (; index < raw.Length && char.IsLetter(raw[index]); index++)
        {
            key.Append(raw[index]);
        }
        return key.ToString();
    }
}
```

## Lambdas
While no one would ever have used lambdas in this way, this example was written to help illustrate an important
difference between lambdas and local functions. Let's take this portion of the code as an example:

```
Dictionary<string, string> result = new();
int index = 0;

var parseImpl = () =>
{
    while (index < raw.Length)
    {
        parsePair();
        readDelimiter(';');
    }
};

parseImpl();
```

Let's also take a look at the relevant portion of the IL:

```
IL_0000: newobj instance void csharpfun.LocalFunctions.StringMapParser_Lambda/'<>c__DisplayClass0_0'::.ctor()
IL_0005: dup
// string raw = raw;
IL_0006: ldarg.0
IL_0007: stfld string csharpfun.LocalFunctions.StringMapParser_Lambda/'<>c__DisplayClass0_0'::raw
// (no C# code)
IL_000c: dup
// Dictionary<string, string> result = new Dictionary<string, string>();
IL_000d: newobj instance void class [System.Collections]System.Collections.Generic.Dictionary`2<string, string>::.ctor()
IL_0012: stfld class [System.Collections]System.Collections.Generic.Dictionary`2<string, string> csharpfun.LocalFunctions.StringMapParser_Lambda/'<>c__DisplayClass0_0'::result
// (no C# code)
IL_0017: dup
// int index = 0;
IL_0018: ldc.i4.0
IL_0019: stfld int32 csharpfun.LocalFunctions.StringMapParser_Lambda/'<>c__DisplayClass0_0'::index

...

IL_0055: ldftn instance void csharpfun.LocalFunctions.StringMapParser_Lambda/'<>c__DisplayClass0_0'::'<Parse>b__3'()
IL_005b: newobj instance void [System.Runtime]System.Action::.ctor(object, native int)
// (no C# code)
IL_0060: callvirt instance void [System.Runtime]System.Action::Invoke()
```

When lambdas *capture* local state within their instantiating function, they instantiate a *closure*. They also
instantiate instances of *delegates* when calling them.
`newobj instance ... '<>c__DisplayClass0_0'::.ctor()` is the instantiation of an instance of this closure -- a heap
allocation. `newobj instance void ... System.Action::.ctor(object, native int)` is the instantiation of a delegate
on the heap. All in all, this is four extra heap allocations, where the previous implementation has zero of these.

## Local functions
Let's first compare the local implementation to the static method implementation.

```
public static IReadOnlyDictionary<string, string> Parse(string raw)
{
    Dictionary<string, string> result = new();
    int index = 0;

    ParseImpl();

    return result;

    void ParseImpl()
    {
        while (index < raw.Length)
        {
            ParsePair();
            ReadDelimiter(';');
        }
    }

    void ParsePair()
    {
        string key = ReadString();
        ReadDelimiter('=');
        string value = ReadString();

        if (!result.TryAdd(key, value))
        {
            throw new ParseException("Key already present.");
        }
    }

    void ReadDelimiter(char c)
    {
        if (index >= raw.Length) throw new ParseException("Unexpectedly reached end of string.");
        if (c != raw[index]) throw new ParseException("Unexpected character found.");
        index++;
    }

    string ReadString()
    {
        StringBuilder key = new StringBuilder();
        for (; index < raw.Length && char.IsLetter(raw[index]); index++)
        {
            key.Append(raw[index]);
        }
        return key.ToString();
    }
}
```

It's not a huge deal in our contrived example, but note how we don't need to pass `result` and `index`,
because local functions can also *capture* local state. For more complicated methods with more locals,
this can become very complicated, and the costs start to add up when adding additional locals months down the line,
which can necessitate modifying multiple method signatures that get longer and longer.
Here, with local functions, there is no need to redeclare these. This implementation is the cleanest of the three.

Let's now compare the local function implementation to the lambda implementation and the IL of both.
Though, again, no sane person would have ever used lambdas in this way. Both implementations take advantage of *capturing*.
However, the lambda functions are ordered in a particular way. Since they are stored in variables,
`parsePair` must come before the `parseImpl` that calls it. Local functions have no such restriction,
and the functions can be placed anywhere most convenient -- sometimes at the bottom of the method in natural order,
and sometimes near to where they are used.

Now, let's look at some of the IL:

```
.locals init (
    [0] valuetype csharpfun.LocalFunctions.StringMapParser_LocalFunc/'<>c__DisplayClass0_0' 'CS$<>8__locals0'
)

IL_0000: ldloca.s 0
// string raw = raw;
IL_0002: ldarg.0
IL_0003: stfld string csharpfun.LocalFunctions.StringMapParser_LocalFunc/'<>c__DisplayClass0_0'::raw
// Dictionary<string, string> result = new Dictionary<string, string>();
IL_0008: ldloca.s 0
IL_000a: newobj instance void class [System.Collections]System.Collections.Generic.Dictionary`2<string, string>::.ctor()
IL_000f: stfld class [System.Collections]System.Collections.Generic.Dictionary`2<string, string> csharpfun.LocalFunctions.StringMapParser_LocalFunc/'<>c__DisplayClass0_0'::result
// int index = 0;
IL_0014: ldloca.s 0
IL_0016: ldc.i4.0
IL_0017: stfld int32 csharpfun.LocalFunctions.StringMapParser_LocalFunc/'<>c__DisplayClass0_0'::index
// ParseImpl();
IL_001c: ldloca.s 0
IL_001e: call void csharpfun.LocalFunctions.StringMapParser_LocalFunc::'<Parse>g__ParseImpl|0_0'(valuetype csharpfun.LocalFunctions.StringMapParser_LocalFunc/'<>c__DisplayClass0_0'&)
// (no C# code)
IL_0023: ldloc.0
// return result;
IL_0024: ldfld class [System.Collections]System.Collections.Generic.Dictionary`2<string, string> csharpfun.LocalFunctions.StringMapParser_LocalFunc/'<>c__DisplayClass0_0'::result
IL_0029: ret
```

Local functions also create closures, but they are located cheaply on the stack (usually). No extra allocations, with
all the benefits of capturing!

## Passing local functions
It *is* possible for the compiler to allocate closures on the heap, though.

```
private static void Invoke(Action action) => action();

public static IReadOnlyDictionary<string, string> Parse(string raw)
{
    Dictionary<string, string> result = new();
    int index = 0;

    Invoke(ParseImpl);

    return result;

    void ParseImpl()
    {
        ...
```

```
IL_0000: newobj instance void csharpfun.LocalFunctions.StringMapParser_LocalFuncClosureOnHeap/'<>c__DisplayClass1_0'::.ctor()
...
IL_001f: ldftn instance void csharpfun.LocalFunctions.StringMapParser_LocalFuncClosureOnHeap/'<>c__DisplayClass1_0'::'<Parse>g__ParseImpl|0'()
IL_0025: newobj instance void [System.Runtime]System.Action::.ctor(object, native int)
```

Here, we're passing the local function to another non-local method. This requires allocating a delegate for the
`ParseImpl` call on the heap.
It is, of course, generally unsafe for heap-allocated data to reference stack-allocated data, since the heap allocations
can outlive the stack allocation. As such, the closure must also be allocated on the heap.

Luckily, in this example, this is only two extra heap allocations, versus the four from the lambda implementation.
One is for the closure, and one is for the initial call to `ParseImpl`. The rest of the calls are members of the closure
object, so no additional allocations are needed.

## Conclusion
Use local functions more! Obviously, I wouldn't say local functions should always be preferred over static methods
and lambdas. However, local functions often result in cleaner code and can sometimes result in more efficient code.

Ah! I forgot to mention that static local functions are also a thing...
```
void Foo()
{
    Bar();

    static Bar() {}
}
```

Static local functions cannot reference other locals (but can reference members of the class where appropriate).
As such, since they can't capture any locals, they don't necessitate the usage of closures, neither stack-allocated
nor heap-allocated. Not that stack-allocated closures usually matter.

For more information on local functions:
https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/local-functions