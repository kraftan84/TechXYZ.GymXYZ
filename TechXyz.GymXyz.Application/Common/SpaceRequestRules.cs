namespace TechXyz.GymXyz.Application.Common;

/// <summary>
/// What the onboarding form refuses, in the words it refuses with.
/// <para>
/// Read by the validator <b>and</b> by the six steps, so a step's inline error and
/// the server's refusal are literally the same sentence. The prototype validated
/// nothing at all; these are the rules that had to be decided rather than ported.
/// </para>
/// </summary>
public static class SpaceRequestRules
{
    public const string NameRequired = "Indiquez le nom de votre structure.";

    public const string SoloNameRequired = "Indiquez le nom de votre activité.";

    public const string FirstNameRequired = "Indiquez votre prénom.";

    public const string LastNameRequired = "Indiquez votre nom.";

    public const string EmailRequired = "Indiquez votre e-mail professionnel.";

    public const string EmailInvalid = "Cette adresse e-mail n'est pas valide.";

    public const string SubdomainRequired = "Choisissez l'adresse de votre espace.";

    public const string SubdomainInvalid =
        "L'adresse ne prend que des minuscules, des chiffres et des tirets, entre 3 et 40 caractères.";

    public const string SubdomainReserved =
        "Cette adresse est réservée par GymXYZ. Choisissez-en une autre.";

    public const string SubdomainTaken = "Cette adresse est déjà prise.";

    public const string PlanUnknown = "Choisissez une formule.";

    public const string ConsentsRequired =
        "Les deux premiers consentements sont nécessaires pour envoyer la demande.";

    /// <summary>
    /// Shown when the send itself failed. The wording matters: the entry brief
    /// asked that a failure keep the typed answers and offer another go, because
    /// six steps of retyping is how a real applicant gives up.
    /// </summary>
    public const string SendFailed =
        "L'envoi n'a pas abouti. Vos réponses sont conservées : réessayez dans un instant.";

    /// <summary>Postcode is optional, but a wrong one silently picks a holiday zone.</summary>
    public const string ZipCodeInvalid = "Un code postal français fait cinq chiffres.";
}
