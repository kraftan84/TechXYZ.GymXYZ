using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Application.Models;

namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// Sends through Brevo's transactional API.
/// <para>
/// Brevo rather than a US provider because this product hosts in France and says
/// so on its own login screen: a member's name and address leaving the EU would
/// need transfer safeguards nobody has arranged. Its REST endpoint rather than
/// its SDK because the call is one POST — no package to carry, and swapping
/// provider is this file and a configuration block.
/// </para>
/// <para>
/// Never throws, by the contract of <see cref="IEmailSender"/>: the caller has
/// already committed a cancellation or a stamped relance, and an exception here
/// would undo it because somebody else's API was slow.
/// </para>
/// </summary>
public sealed class BrevoEmailSender : IEmailSender
{
    public const string HttpClientName = "brevo";

    /// <summary>A screen is waiting on this. Better a reported failure than a hung page.</summary>
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(8);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EmailOptions _options;
    private readonly ILogger<BrevoEmailSender> _logger;

    public BrevoEmailSender(
        IHttpClientFactory httpClientFactory,
        IOptions<EmailOptions> options,
        ILogger<BrevoEmailSender> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<EmailDeliveryResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(Timeout);

            var client = _httpClientFactory.CreateClient(HttpClientName);
            client.DefaultRequestHeaders.Remove("api-key");
            client.DefaultRequestHeaders.Add("api-key", _options.ApiKey);

            var payload = new BrevoPayload(
                new BrevoContact(_options.FromAddress, message.FromName ?? _options.FromName),
                [new BrevoContact(message.ToAddress, message.ToName)],
                message.Subject,
                message.TextBody,
                message.ReplyToAddress is null
                    ? null
                    : new BrevoContact(message.ReplyToAddress, message.ReplyToName));

            using var response = await client.PostAsJsonAsync(_options.ApiUrl, payload, timeout.Token);

            if (response.IsSuccessStatusCode)
            {
                return EmailDeliveryResult.Sent;
            }

            // The body carries Brevo's own explanation — a rejected address, a
            // key without permission. Logged, never shown: it is their wording,
            // in English, about our configuration.
            var body = await response.Content.ReadAsStringAsync(timeout.Token);
            _logger.LogWarning(
                "Brevo refused a message to {Recipient}: {Status} {Body}",
                message.ToAddress, (int)response.StatusCode, body);

            return EmailDeliveryResult.Failed($"HTTP {(int)response.StatusCode}");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Brevo timed out sending to {Recipient}.", message.ToAddress);
            return EmailDeliveryResult.Failed("timeout");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not send a message to {Recipient}.", message.ToAddress);
            return EmailDeliveryResult.Failed(exception.GetType().Name);
        }
    }

    private sealed record BrevoPayload(
        BrevoContact Sender,
        IReadOnlyList<BrevoContact> To,
        string Subject,
        string TextContent,
        BrevoContact? ReplyTo);

    private sealed record BrevoContact(string Email, string? Name);
}
