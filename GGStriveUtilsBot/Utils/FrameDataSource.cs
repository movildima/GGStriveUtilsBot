using System.Collections.Generic;
using DSharpPlus.SlashCommands;
public class MoveData
{
    public string chara { get; set; }
    public string input { get; set; }
    public string name { get; set; }
    public string[] images { get; set; }
    public string damage { get; set; }
    public string guard { get; set; }
    public string startup { get; set; }
    public string active { get; set; }
    public string recovery { get; set; }
    public string onBlock { get; set; }
    public string onHit { get; set; }
    public string invuln { get; set; }
    public string type { get; set; }
    public bool imgLoaded { get; set; } = false;
    public string imgFull { get; set; }
}

public class MoveListInternal
{
    public List<MoveData> moves { get; set; }
    public MoveDataResult result { get; set; }
}

public enum MoveDataResult
{
    Success,
    NoResult,
    ExtraResults,
    TooManyResults,
    SpecialBehemoth
}

public class IconData
{
    public string name { get; set; }
    public string icon { get; set; }
    public bool iconLoaded { get; set; }
    public string iconFull { get; set; }
}
public enum Character
{
    [ChoiceName("Sol Badguy")]
    Sol,
    [ChoiceName("Ky Kiske")]
    Ky,
    [ChoiceName("May")]
    May,
    [ChoiceName("Faust")]
    Faust,
    [ChoiceName("I-no")]
    Ino,
    [ChoiceName("Ramlethal Valentine")]
    Ram,
    [ChoiceName("Zato-1")]
    Zato,
    [ChoiceName("Nagoriyuki")]
    Nago,
    [ChoiceName("Potemkin")]
    Pot,
    [ChoiceName("Giovanna")]
    Gio,
    [ChoiceName("Millia Rage")]
    Millia,
    [ChoiceName("Leo Whitefang")]
    Leo,
    [ChoiceName("Chipp Zanuff")]
    Chipp,
    [ChoiceName("Anji Mito")]
    Anji,
    [ChoiceName("Axl Low")]
    Axl,
    [ChoiceName("Goldlewis Dickinson")]
    Goldlewis,
    [ChoiceName("Jack-O")]
    Jacko,
    [ChoiceName("Happy Chaos")]
    HappyChaos,
    [ChoiceName("Baiken")]
    Baiken,
    [ChoiceName("Testament")]
    Testament,
    [ChoiceName("Bridget")]
    Bridget,
    [ChoiceName("Sin Kiske")]
    Sin,
    [ChoiceName("Bedman")]
    Bedman,
}