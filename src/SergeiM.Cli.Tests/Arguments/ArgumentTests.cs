using SergeiM.Cli.Arguments;

namespace SergeiM.Cli.Tests.Arguments;

[TestClass]
public class ArgumentTests
{
    [TestMethod]
    public void Constructor_RequiredByDefault_SetsAllProperties()
    {
        var arg = new Argument<string>("<file>", "Input file");
        Assert.AreEqual("<file>", arg.Name);
        Assert.AreEqual("Input file", arg.Description);
        Assert.AreEqual(typeof(string), arg.ValueType);
        Assert.IsTrue(arg.IsRequired);
        Assert.IsNull(arg.Default);
        Assert.IsNull(arg.DefaultValue);
    }

    [TestMethod]
    public void Constructor_Optional_IsRequiredIsFalse()
    {
        var arg = new Argument<string>("<file>", "Optional file", isRequired: false);
        Assert.IsFalse(arg.IsRequired);
    }

    [TestMethod]
    public void Constructor_WithDefaultValue_ExposesTypedAndUntypedDefault()
    {
        var arg = new Argument<int>("<count>", "Count", defaultValue: 1);
        Assert.AreEqual(1, arg.Default);
        Assert.AreEqual(1, arg.DefaultValue);
    }

    [TestMethod]
    public void ValueType_ReflectsTypeParameter()
    {
        Assert.AreEqual(typeof(int), new Argument<int>("<n>", "N").ValueType);
        Assert.AreEqual(typeof(bool), new Argument<bool>("<flag>", "Flag").ValueType);
    }
}
