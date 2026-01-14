using System;
using System.Collections.Generic;

namespace Erg.Core.Messages;

public class MessageBuffer
{
    private readonly List<string> _messages = new();
    private int _displayOffset = 0;
    private const int LineWidth = 80;
    private const int VisibleLines = 2;

    public void Add(string message)
    {
        _messages.Add(message);
    }

    public void Clear()
    {
        _messages.Clear();
        _displayOffset = 0;
    }

    public bool HasUnreadMessages => GetWrappedLines().Count > _displayOffset;
    public bool NeedsMorePrompt => GetWrappedLines().Count > _displayOffset + VisibleLines;

    public void ShowNext()
    {
        _displayOffset += VisibleLines;
    }

    public void SkipAll()
    {
        _displayOffset = GetWrappedLines().Count;
    }

    public (string line1, string line2, bool showMore) GetDisplayLines()
    {
        var lines = GetWrappedLines();
        string line1 = _displayOffset < lines.Count ? lines[_displayOffset] : "";
        string line2 = _displayOffset + 1 < lines.Count ? lines[_displayOffset + 1] : "";
        bool showMore = lines.Count > _displayOffset + VisibleLines;

        if (showMore && line2.Length > LineWidth - 7)
            line2 = line2[..(LineWidth - 7)];

        return (line1, line2, showMore);
    }

    private List<string> GetWrappedLines()
    {
        var result = new List<string>();
        var fullText = string.Join(" ", _messages);

        if (string.IsNullOrEmpty(fullText))
            return result;

        var words = fullText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var currentLine = "";

        foreach (var word in words)
        {
            if (currentLine.Length == 0)
            {
                currentLine = word;
            }
            else if (currentLine.Length + 1 + word.Length <= LineWidth)
            {
                currentLine += " " + word;
            }
            else
            {
                result.Add(currentLine);
                currentLine = word;
            }
        }

        if (currentLine.Length > 0)
            result.Add(currentLine);

        return result;
    }
}
