using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Chaldea.Fate.RhoAias.Dashboard;

internal class DashboardSpaBuilder : ISpaBuilder
{
    public IApplicationBuilder ApplicationBuilder { get; set; }
    public SpaOptions Options { get; set; }
}

internal sealed class SpaDefaultPageMiddleware
{
    public static void Attach(ISpaBuilder spaBuilder)
    {
        ArgumentNullException.ThrowIfNull(spaBuilder);

        var app = spaBuilder.ApplicationBuilder;
        var options = spaBuilder.Options;
        var serverOptions = app.ApplicationServices.GetService<IOptions<RhoAiasServerOptions>>().Value;

        // Rewrite all requests to the default page
        app.Use((context, next) =>
        {
            // If we have an Endpoint, then this is a deferred match - just noop.
            if (context.GetEndpoint() != null)
            {
                return next(context);
            }

            if (context.Connection.LocalPort != serverOptions.Bridge)
            {
                return next(context);
            }

            context.Request.Path = options.DefaultPage;
            return next(context);
        });

        // Serve it as a static file
        // Developers who need to host more than one SPA with distinct default pages can
        // override the file provider
        app.UseSpaStaticFilesInternal(
            options.DefaultPageStaticFileOptions ?? new StaticFileOptions(),
            allowFallbackOnServingWebRootFiles: true);

        // If the default file didn't get served as a static file (usually because it was not
        // present on disk), the SPA is definitely not going to work.
        app.Use((context, next) =>
        {
            // If we have an Endpoint, then this is a deferred match - just noop.
            if (context.GetEndpoint() != null)
            {
                return next(context);
            }

            if (context.Connection.LocalPort != serverOptions.Bridge)
            {
                return next(context);
            }

            var message = "The SPA default page middleware could not return the default page " +
                          $"'{options.DefaultPage}' because it was not found, and no other middleware " +
                          "handled the request.\n";

            // Try to clarify the common scenario where someone runs an application in
            // Production environment without first publishing the whole application
            // or at least building the SPA.
            var hostEnvironment = (IWebHostEnvironment?)context.RequestServices.GetService(typeof(IWebHostEnvironment));
            if (hostEnvironment != null && hostEnvironment.IsProduction())
            {
                message += "Your application is running in Production mode, so make sure it has " +
                           "been published, or that you have built your SPA manually. Alternatively you " +
                           "may wish to switch to the Development environment.\n";
            }

            throw new InvalidOperationException(message);
        });
    }
}

internal static class DashboardSpaExtensions
{
    public static void UseDashboardSpa(this IApplicationBuilder app, Action<ISpaBuilder> configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var spaBuilder = new DashboardSpaBuilder
        {
            Options = new SpaOptions(),
            ApplicationBuilder = app
        };

        SpaDefaultPageMiddleware.Attach(spaBuilder);
    }

    internal static void UseSpaStaticFilesInternal(
        this IApplicationBuilder app,
        StaticFileOptions staticFileOptions,
        bool allowFallbackOnServingWebRootFiles)
    {
        ArgumentNullException.ThrowIfNull(staticFileOptions);

        // If the file provider was explicitly supplied, that takes precedence over any other
        // configured file provider. This is most useful if the application hosts multiple SPAs
        // (via multiple calls to UseSpa()), so each needs to serve its own separate static files
        // instead of using AddSpaStaticFiles/UseSpaStaticFiles.
        // But if no file provider was specified, try to get one from the DI config.
        if (staticFileOptions.FileProvider == null)
        {
            var shouldServeStaticFiles = ShouldServeStaticFiles(
                app,
                allowFallbackOnServingWebRootFiles,
                out var fileProviderOrDefault);
            if (shouldServeStaticFiles)
            {
                staticFileOptions.FileProvider = fileProviderOrDefault;
            }
            else
            {
                // The registered ISpaStaticFileProvider says we shouldn't
                // serve static files
                return;
            }
        }

        app.UseStaticFiles(staticFileOptions);
    }

    private static bool ShouldServeStaticFiles(
        IApplicationBuilder app,
        bool allowFallbackOnServingWebRootFiles,
        out IFileProvider? fileProviderOrDefault)
    {
        var spaStaticFilesService = app.ApplicationServices.GetService<ISpaStaticFileProvider>();
        if (spaStaticFilesService != null)
        {
            // If an ISpaStaticFileProvider was configured but it says no IFileProvider is available
            // (i.e., it supplies 'null'), this implies we should not serve any static files. This
            // is typically the case in development when SPA static files are being served from a
            // SPA development server (e.g., Angular CLI or create-react-app), in which case no
            // directory of prebuilt files will exist on disk.
            fileProviderOrDefault = spaStaticFilesService.FileProvider;
            return fileProviderOrDefault != null;
        }
        else if (!allowFallbackOnServingWebRootFiles)
        {
            throw new InvalidOperationException($"To use UseSpaStaticFiles, you must " +
                                                $"first register an {nameof(ISpaStaticFileProvider)} in the service provider, typically " +
                                                $"by calling services.AddSpaStaticFiles.");
        }
        else
        {
            // Fall back on serving wwwroot
            fileProviderOrDefault = null;
            return true;
        }
    }
}