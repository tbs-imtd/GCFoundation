using System.Globalization;
using System.Resources;

namespace GCFoundation.Web.Resources;

/// <summary>Localized content for the code component documentation.</summary>
internal static class CodeDemo
{
    private static readonly ResourceManager Manager = new("GCFoundation.Web.Resources.CodeDemo", typeof(CodeDemo).Assembly);

    private static string Get(string name) => Manager.GetString(name, CultureInfo.CurrentUICulture)!;

    /// <summary>Gets the localized component name.</summary>
    public static string Name => Get(nameof(Name));
    /// <summary>Gets the localized Overview text.</summary>
    public static string Overview => Get(nameof(Overview));
    /// <summary>Gets the localized IndexDescription text.</summary>
    public static string IndexDescription => Get(nameof(IndexDescription));
    /// <summary>Gets the localized Purpose text.</summary>
    public static string Purpose => Get(nameof(Purpose));
    /// <summary>Gets the localized WhenUse1 text.</summary>
    public static string WhenUse1 => Get(nameof(WhenUse1));
    /// <summary>Gets the localized WhenUse2 text.</summary>
    public static string WhenUse2 => Get(nameof(WhenUse2));
    /// <summary>Gets the localized WhenNot1 text.</summary>
    public static string WhenNot1 => Get(nameof(WhenNot1));
    /// <summary>Gets the localized WhenNot2 text.</summary>
    public static string WhenNot2 => Get(nameof(WhenNot2));
    /// <summary>Gets the localized Accessibility1 text.</summary>
    public static string Accessibility1 => Get(nameof(Accessibility1));
    /// <summary>Gets the localized Accessibility2 text.</summary>
    public static string Accessibility2 => Get(nameof(Accessibility2));
    /// <summary>Gets the localized Accessibility3 text.</summary>
    public static string Accessibility3 => Get(nameof(Accessibility3));
    /// <summary>Gets the localized Ux1 text.</summary>
    public static string Ux1 => Get(nameof(Ux1));
    /// <summary>Gets the localized Ux2 text.</summary>
    public static string Ux2 => Get(nameof(Ux2));
    /// <summary>Gets the localized NoteLanguages text.</summary>
    public static string NoteLanguages => Get(nameof(NoteLanguages));
    /// <summary>Gets the localized NoteEncoding text.</summary>
    public static string NoteEncoding => Get(nameof(NoteEncoding));
    /// <summary>Gets the localized NoteProgressive text.</summary>
    public static string NoteProgressive => Get(nameof(NoteProgressive));
    /// <summary>Gets the localized NoteLimit text.</summary>
    public static string NoteLimit => Get(nameof(NoteLimit));
    /// <summary>Gets the localized NoteDynamic text.</summary>
    public static string NoteDynamic => Get(nameof(NoteDynamic));
    /// <summary>Gets the localized BlockTitle text.</summary>
    public static string BlockTitle => Get(nameof(BlockTitle));
    /// <summary>Gets the localized BlockText text.</summary>
    public static string BlockText => Get(nameof(BlockText));
    /// <summary>Gets the localized ExampleTitle text.</summary>
    public static string ExampleTitle => Get(nameof(ExampleTitle));
    /// <summary>Gets the localized ExampleText text.</summary>
    public static string ExampleText => Get(nameof(ExampleText));
    /// <summary>Gets the localized FallbackTitle text.</summary>
    public static string FallbackTitle => Get(nameof(FallbackTitle));
    /// <summary>Gets the localized FallbackText text.</summary>
    public static string FallbackText => Get(nameof(FallbackText));
    /// <summary>Gets the localized RendererTitle text.</summary>
    public static string RendererTitle => Get(nameof(RendererTitle));
    /// <summary>Gets the localized RendererText text.</summary>
    public static string RendererText => Get(nameof(RendererText));
    /// <summary>Gets the localized SampleResponseTitle text.</summary>
    public static string SampleResponseTitle => Get(nameof(SampleResponseTitle));
    /// <summary>Gets the localized SampleRegistrationTitle text.</summary>
    public static string SampleRegistrationTitle => Get(nameof(SampleRegistrationTitle));
    /// <summary>Gets the localized SampleCreateTitle text.</summary>
    public static string SampleCreateTitle => Get(nameof(SampleCreateTitle));
    /// <summary>Gets the localized SampleReadTitle text.</summary>
    public static string SampleReadTitle => Get(nameof(SampleReadTitle));
    /// <summary>Gets the localized SampleLogTitle text.</summary>
    public static string SampleLogTitle => Get(nameof(SampleLogTitle));
    /// <summary>Gets the localized SampleRequestTitle text.</summary>
    public static string SampleRequestTitle => Get(nameof(SampleRequestTitle));
    /// <summary>Gets the localized PropCode text.</summary>
    public static string PropCode => Get(nameof(PropCode));
    /// <summary>Gets the localized PropCopyEnabled text.</summary>
    public static string PropCopyEnabled => Get(nameof(PropCopyEnabled));
    /// <summary>Gets the localized PropId text.</summary>
    public static string PropId => Get(nameof(PropId));
    /// <summary>Gets the localized PropLanguage text.</summary>
    public static string PropLanguage => Get(nameof(PropLanguage));
    /// <summary>Gets the localized PropLanguageLabel text.</summary>
    public static string PropLanguageLabel => Get(nameof(PropLanguageLabel));
    /// <summary>Gets the localized PropTitle text.</summary>
    public static string PropTitle => Get(nameof(PropTitle));
    /// <summary>Gets the localized PropWrap text.</summary>
    public static string PropWrap => Get(nameof(PropWrap));
    /// <summary>Gets the localized PropExamples text.</summary>
    public static string PropExamples => Get(nameof(PropExamples));
    /// <summary>Gets the localized PropSelectedLanguage text.</summary>
    public static string PropSelectedLanguage => Get(nameof(PropSelectedLanguage));
    /// <summary>Gets the localized PropSyncGroup text.</summary>
    public static string PropSyncGroup => Get(nameof(PropSyncGroup));
}
