using System.Collections.Generic;

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
    TooManyResults
}

public class IconData
{
    public string name { get; set; }
    public string icon { get; set; }
    public bool iconLoaded { get; set; }
    public string iconFull { get; set; }
}
