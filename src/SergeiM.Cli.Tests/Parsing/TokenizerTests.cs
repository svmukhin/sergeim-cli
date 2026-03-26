using SergeiM.Cli.Parsing;

namespace SergeiM.Cli.Tests.Parsing;

[TestClass]
public class TokenizerTests
{
    private static Token[] Tokenize(params string[] args) => new Tokenizer().Tokenize(args);

    [TestMethod]
    public void Tokenize_EmptyArray_ReturnsEmptyArray()
    {
        Assert.AreEqual(0, Tokenize().Length);
    }

    [TestMethod]
    public void Tokenize_PlainValues_AllBecomeValue()
    {
        var tokens = Tokenize("remote", "add", "origin");
        Assert.AreEqual(3, tokens.Length);
        Assert.IsTrue(tokens.All(t => t.Kind == TokenKind.Value));
    }

    [TestMethod]
    public void Tokenize_LongOption_BecomesLongOption()
    {
        var tokens = Tokenize("--name", "alice");
        Assert.AreEqual(TokenKind.LongOption, tokens[0].Kind);
        Assert.AreEqual("--name", tokens[0].Raw);
        Assert.AreEqual(TokenKind.Value, tokens[1].Kind);
    }

    [TestMethod]
    public void Tokenize_ShortOption_BecomesShortOption()
    {
        var tokens = Tokenize("-n", "alice");
        Assert.AreEqual(TokenKind.ShortOption, tokens[0].Kind);
        Assert.AreEqual("-n", tokens[0].Raw);
        Assert.AreEqual(TokenKind.Value, tokens[1].Kind);
    }

    [TestMethod]
    public void Tokenize_BoolFlag_NoFollowingValueRequired()
    {
        var tokens = Tokenize("--verbose");
        Assert.AreEqual(1, tokens.Length);
        Assert.AreEqual(TokenKind.LongOption, tokens[0].Kind);
    }

    [TestMethod]
    public void Tokenize_EndOfOptionsSeparator_BecomesEndOfOptions()
    {
        var tokens = Tokenize("--");
        Assert.AreEqual(1, tokens.Length);
        Assert.AreEqual(TokenKind.EndOfOptions, tokens[0].Kind);
    }

    [TestMethod]
    public void Tokenize_TokensAfterSeparator_AllBecomeValue()
    {
        var tokens = Tokenize("--output", "--", "--not-an-option", "-x", "file.txt");
        Assert.AreEqual(TokenKind.LongOption, tokens[0].Kind);
        Assert.AreEqual(TokenKind.EndOfOptions, tokens[1].Kind);
        Assert.AreEqual(TokenKind.Value, tokens[2].Kind);
        Assert.AreEqual(TokenKind.Value, tokens[3].Kind);
        Assert.AreEqual(TokenKind.Value, tokens[4].Kind);
    }

    [TestMethod]
    public void Tokenize_SingleDashWithMultipleChars_BecomesValue()
    {
        var tokens = Tokenize("-verbose");
        Assert.AreEqual(1, tokens.Length);
        Assert.AreEqual(TokenKind.Value, tokens[0].Kind);
    }

    [TestMethod]
    public void Tokenize_MixedTokens_PreservesOrder()
    {
        var tokens = Tokenize("cmd", "--name", "alice", "-v");
        Assert.AreEqual(TokenKind.Value, tokens[0].Kind);
        Assert.AreEqual(TokenKind.LongOption, tokens[1].Kind);
        Assert.AreEqual(TokenKind.Value, tokens[2].Kind);
        Assert.AreEqual(TokenKind.ShortOption, tokens[3].Kind);
    }
}
