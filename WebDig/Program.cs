namespace WebDig
{


    
    using System.Collections.Generic;
    using System.Net;
    using System.IO;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System.Text.Json;

    // Assuming Heijden.DNS is already ported as mentioned
    using Heijden.DNS;



    public class Program
    {
        public static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Add services
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });


            // Add custom StringWriter to capture console output
            builder.Services.AddSingleton<ConsoleOutputCapture>();


            // Add services to the container.
            builder.Services.AddRazorPages();









            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Optional: Customizing default file behavior
            app.UseDefaultFiles(new DefaultFilesOptions() { DefaultFileNames = new List<string> { "index.htm" } });


            app.UseStaticFiles();
            app.UseCors();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();


            // Serve the HTML file as the default page
            app.MapGet("/", async (HttpContext context) =>
            {
                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync("wwwroot/index.html");
            });



            // API endpoints
            app.MapGet("/api/qtypes", () =>
            {
                var values = System.Enum.GetValues(typeof(QType));
                var options = new List<object>();

                foreach (var value in values)
                {
                    options.Add(new { value = value.ToString(), text = value.ToString() });
                }

                return options;
            });




            app.MapGet("/api/qclasses", () =>
            {
                var values = System.Enum.GetValues(typeof(QClass));
                var options = new List<object>();

                foreach (var value in values)
                {
                    options.Add(new { value = value.ToString(), text = value.ToString() });
                }

                return options;
            });


            app.MapGet("/api/default-dns", () =>
            {
                // Get system DNS server
                var dnsServers = Resolver.GetDnsServers();
                string serverAddress = dnsServers.Length > 0
                    ? dnsServers[0].Address.ToString()
                    : "8.8.8.8"; // Default to Google DNS if no system DNS found

                return new { server = serverAddress };
            });



            app.MapPost("/api/dig", async (HttpContext context, ConsoleOutputCapture console) =>
            {
                try
                {
                    using var reader = new StreamReader(context.Request.Body);
                    var requestBody = await reader.ReadToEndAsync();
                    var queryRequest = JsonSerializer.Deserialize<DnsQueryRequest>(requestBody);

                    if (queryRequest == null)
                    {
                        context.Response.StatusCode = 400;
                        return "Invalid request format";
                    }

                    // Redirect console output to our capture
                    var originalOutput = System.Console.Out;
                    using var writer = new StringWriter();
                    System.Console.SetOut(writer);

                    try
                    {
                        // Create and configure the Dig instance
                        var dig = new Dig();
                        dig.resolver.Recursion = queryRequest.Recursion;
                        dig.resolver.UseCache = queryRequest.UseCache;
                        dig.resolver.DnsServer = queryRequest.DnsServer;
                        dig.resolver.TimeOut = queryRequest.Timeout;
                        dig.resolver.Retries = queryRequest.Attempts;

                        // Set transport type
                        dig.resolver.TransportType = queryRequest.Transport.ToUpper() == "TCP"
                            ? TransportType.Tcp
                            : TransportType.Udp;

                        // Parse QType and QClass from strings
                        if (!System.Enum.TryParse(queryRequest.QType, true, out QType qType))
                        {
                            context.Response.StatusCode = 400;
                            return $"Invalid QType: {queryRequest.QType}";
                        }

                        if (!System.Enum.TryParse(queryRequest.QClass, true, out QClass qClass))
                        {
                            context.Response.StatusCode = 400;
                            return $"Invalid QClass: {queryRequest.QClass}";
                        }

                        string query = queryRequest.Query;

                        // Handle ARPA conversion if needed
                        if (queryRequest.ArpaConvert)
                        {
                            if (qType == QType.PTR)
                            {
                                if (IPAddress.TryParse(query, out IPAddress ip))
                                {
                                    query = Resolver.GetArpaFromIp(ip);
                                }
                            }
                            else if (qType == QType.NAPTR)
                            {
                                query = Resolver.GetArpaFromEnum(query);
                            }
                        }

                        // Force TCP for AXFR requests
                        if (qType == QType.AXFR)
                        {
                            dig.resolver.TransportType = TransportType.Tcp;
                        }

                        // Perform the DNS query (synchronously for simplicity)
                        dig.DigIt(query, qType, qClass);

                        // Return the console output
                        return writer.ToString();
                    }
                    finally
                    {
                        System.Console.SetOut(originalOutput);
                    }
                }
                catch (System.Exception ex)
                {
                    context.Response.StatusCode = 500;
                    return $"Error processing request: {ex.Message}";
                }
            });

            await app.RunAsync();
            return 0;
        }
    }
}
