using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using TechXyz.GymXyz.WebApp.Services;

namespace TechXYZ.GymXYZ.WebApp.Tests.Services;

/// <summary>
/// What the service does with its configuration, and what it does when the
/// sources will not answer. Pinned through a handler that never leaves the
/// machine: the two government APIs are outside the application, and a test
/// that called them would fail on a train.
/// <para>
/// This is what moving the URLs into <see cref="ExternalApiOptions"/> bought —
/// before it, pointing the service somewhere else meant editing the service.
/// </para>
/// </summary>
public class SchoolCalendarServiceTests
{
    [Fact]
    public async Task GetAsync_ShouldCallTheConfiguredEndpoints()
    {
        // An empty object rather than an empty array: it deserialises into both
        // the holidays dictionary and the vacations envelope, so one stub body
        // serves both calls. An array would throw on the first and the second
        // would never be made.
        var handler = new RecordingHandler(HttpStatusCode.OK, "{}");

        var service = CreateService(handler, new ExternalApiOptions
        {
            PublicHolidaysUrl = "https://example.test/feries/{year}.json",
            SchoolVacationsUrl = "https://example.test/vacances"
        });

        await service.GetAsync("69003", new DateOnly(2026, 8, 3), new DateOnly(2026, 8, 9));

        // The year is substituted rather than appended, which is the whole point
        // of the placeholder.
        handler.Requests.ShouldContain(url => url == "https://example.test/feries/2026.json");
        handler.Requests.ShouldContain(url => url.StartsWith("https://example.test/vacances?where="));
    }

    /// <summary>
    /// Every default is in the options class, so an installation that configures
    /// nothing still reaches the real sources. Checked without a call: this is
    /// about the values, not the network.
    /// </summary>
    [Fact]
    public void Defaults_ShouldPointAtTheOpenDataSources()
    {
        var options = new ExternalApiOptions();

        options.PublicHolidaysUrl.ShouldContain("calendrier.api.gouv.fr");
        options.PublicHolidaysUrl.ShouldContain("{year}");
        options.SchoolVacationsUrl.ShouldContain("data.education.gouv.fr");
        options.SchoolVacationsUrl.ShouldContain("fr-en-calendrier-scolaire");
        options.TimeoutSeconds.ShouldBeGreaterThan(0);
        options.CacheHours.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// A source that is down gives an unavailable calendar, not an exception.
    /// The planning renders without its banner; it does not fail to render, and
    /// nothing reaches the log as an unhandled error.
    /// </summary>
    [Fact]
    public async Task GetAsync_ShouldReturnUnavailable_WhenTheSourceFails()
    {
        var service = CreateService(
            new RecordingHandler(HttpStatusCode.ServiceUnavailable, string.Empty),
            new ExternalApiOptions());

        var calendar = await service.GetAsync("69003", new DateOnly(2026, 8, 3), new DateOnly(2026, 8, 9));

        calendar.IsAvailable.ShouldBeFalse();
        calendar.Zone.ShouldBe("A");
        calendar.Holidays.ShouldBeEmpty();
        calendar.Vacations.ShouldBeEmpty();
    }

    private static SchoolCalendarService CreateService(HttpMessageHandler handler, ExternalApiOptions options) =>
        new(new StubHttpClientFactory(handler),
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<SchoolCalendarService>.Instance,
            Options.Create(options));

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class RecordingHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        public List<string> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!.ToString());

            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}
