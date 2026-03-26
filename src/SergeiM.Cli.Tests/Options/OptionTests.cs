using SergeiM.Cli.Options;

namespace SergeiM.Cli.Tests.Options;

[TestClass]
public class OptionTests
{
    [TestMethod]
    public void Constructor_WithoutShortName_SetsAllProperties()
    {
        var option = new Option<string>("--name", "A name", isRequired: true);
        Assert.AreEqual("--name", option.Name);
        Assert.IsNull(option.ShortName);
        Assert.AreEqual("A name", option.Description);
        Assert.AreEqual(typeof(string), option.ValueType);
        Assert.IsTrue(option.IsRequired);
        Assert.IsNull(option.Default);
        Assert.IsNull(option.DefaultValue);
    }

    [TestMethod]
    public void Constructor_WithShortName_SetsShortName()
    {
        var option = new Option<string>("--name", "-n", "A name");
        Assert.AreEqual("--name", option.Name);
        Assert.AreEqual("-n", option.ShortName);
    }

    [TestMethod]
    public void Constructor_WithDefaultValue_ExposesTypedAndUntypedDefault()
    {
        var option = new Option<int>("--count", "Count", defaultValue: 42);
        Assert.AreEqual(42, option.Default);
        Assert.AreEqual(42, option.DefaultValue);
    }

    [TestMethod]
    public void Constructor_OptionalByDefault_IsRequiredIsFalse()
    {
        var option = new Option<string>("--name", "A name");
        Assert.IsFalse(option.IsRequired);
    }

    [TestMethod]
    public void ValueType_ReflectsTypeParameter()
    {
        Assert.AreEqual(typeof(int), new Option<int>("--n", "N").ValueType);
        Assert.AreEqual(typeof(bool), new Option<bool>("--v", "V").ValueType);
        Assert.AreEqual(typeof(double), new Option<double>("--d", "D").ValueType);
    }
}
