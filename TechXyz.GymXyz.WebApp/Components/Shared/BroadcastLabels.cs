namespace TechXyz.GymXyz.WebApp.Components.Shared;

/// <summary>
/// What the three « Diffuser le planning » buttons say — Accueil desktop,
/// Accueil mobile and the head of the Planning — plus the two lines around them.
/// <para>
/// In one place because the three buttons are one feature and switch on
/// together: a screen keeping its own wording is the one still describing the
/// old behaviour after the other two have moved on.
/// </para>
/// <para>
/// Every line here is written against the decision of 2026-08-07: the button
/// produces an <b>image the manager publishes themselves</b>. There is no send,
/// no notification and no recipient list, so no wording may hint at one. The
/// subtitle is here for that reason — it outlives the switch-on, and
/// « diffusez-la à vos membres » would have described the feature falsely
/// forever.
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
    /// The same line for somebody who cannot publish. A coach has no button, so
    /// the subtitle above would describe an action they cannot take and the
    /// footnote would explain a feature that is not theirs — the screen would
    /// talk about the manager's job while showing the coach's.
    /// </summary>
    public const string AccueilSubtitleWithoutBroadcast =
        "Votre semaine, et vos cours du jour.";

    /// <summary>What the button does, before it is pressed.</summary>
    public const string ButtonHint =
        "Produit l'affiche de la semaine, à publier vous-même.";

    /// <summary>
    /// The same, from the Planning — which has filter chips above it. The image
    /// carries the whole week whatever they are set to, and a poster of one
    /// coach's classes published as the club's planning is the trap this
    /// sentence exists to close.
    /// </summary>
    public const string PlanningButtonHint =
        "Produit l'affiche de la semaine, à publier vous-même. Elle reprend toute la semaine, sans les filtres.";

    /// <summary>On the button while the image is being drawn.</summary>
    public const string Working = "Génération…";

    /// <summary>
    /// Why « Aperçu » is still off although the image itself works. The screen
    /// that would show it at scale before producing it is not designed yet.
    /// </summary>
    public const string PreviewUnavailable =
        "L'aperçu à l'échelle arrivera avec l'écran de diffusion.";

    /// <summary>
    /// The foot of the week card. The prototype writes « Dernière diffusion :
    /// dimanche dernier · vos membres notifiés », which describes a product that
    /// was never built; this says what the feature actually is.
    /// </summary>
    public const string Footnote =
        "Le planning se publie en image, à partager vous-même. Aucun envoi n'est fait aux membres.";

    /// <summary>Said once the file has reached the browser.</summary>
    public const string Done = "Affiche générée. Elle est dans vos téléchargements.";

    /// <summary>Named in the toast when a generation fails, and in the log.</summary>
    public const string Action = "la génération de l'affiche";
}
