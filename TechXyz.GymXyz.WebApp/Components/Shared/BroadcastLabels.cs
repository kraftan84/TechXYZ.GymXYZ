namespace TechXyz.GymXyz.WebApp.Components.Shared;

/// <summary>
/// What the three « Diffuser le planning » buttons say — Accueil desktop,
/// Accueil mobile and the head of the Planning — plus the two lines around them.
/// <para>
/// In one place because the three buttons are one feature and have to switch on
/// together: while they are off they must give the same reason, and a screen
/// that keeps its own wording is the one still promising something after the
/// other two have stopped.
/// </para>
/// <para>
/// Every line here is written against a decision taken on 2026-08-07: the button
/// produces an <b>image to publish yourself</b>. There is no send, no
/// notification and no recipient list — so no wording may hint at one. The
/// subtitle is included for that reason: it survives the switch-on, and
/// « diffusez-la à vos membres » would describe the feature falsely forever.
/// </para>
/// </summary>
public static class BroadcastLabels
{
    /// <summary>
    /// Subtitle of the Accueil, on both shells. Says what the week is for
    /// without naming an audience the product never reaches.
    /// </summary>
    public const string AccueilSubtitle =
        "Préparez la semaine et publiez-la en image.";

    /// <summary>
    /// Why the three buttons cannot be pressed yet. Carries the whole truth
    /// rather than half of it: the head of the Planning has no footnote under it
    /// to finish the sentence.
    /// </summary>
    public const string Unavailable =
        "L'affiche du planning à publier n'est pas encore disponible. Aucun envoi aux membres n'est prévu.";

    /// <summary>
    /// Why « Aperçu » is still off once the image itself works. The screen that
    /// would hold a preview at scale is not drawn yet.
    /// </summary>
    public const string PreviewUnavailable =
        "L'aperçu à l'échelle arrivera avec l'écran de diffusion.";

    /// <summary>
    /// The foot of the week card. The prototype writes « Dernière diffusion :
    /// dimanche dernier · vos membres notifiés », which describes a product that
    /// was not built; this states what the feature actually is.
    /// </summary>
    public const string Footnote =
        "Le planning se publie en image, à partager vous-même. Aucun envoi n'est fait aux membres.";
}
