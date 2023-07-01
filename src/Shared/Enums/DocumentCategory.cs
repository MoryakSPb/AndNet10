namespace AndNet.Manager.Shared.Enums;

[Flags]
public enum DocumentCategory : byte
{
    Directive = 1,
    Report = 2,
    Protocol = 4,
    Decision = 8,

    All = byte.MaxValue
}