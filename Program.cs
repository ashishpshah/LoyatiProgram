using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Seed_Admin.Infra;
using System.Globalization;

namespace Seed_Admin
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllersWithViews().AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.PropertyNamingPolicy = null;
			});

			builder.Services.AddHttpClient();

			builder.Services.AddHttpContextAccessor();

			var cultureInfo = new CultureInfo("en-IN") { DateTimeFormat = { ShortDatePattern = "dd/MM/yyyy", LongDatePattern = "dd/MM/yyyy HH:mm:ss" } };

			// Set the default culture globally
			CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
			CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
			//builder.Services.Configure<RequestLocalizationOptions>(options =>
			//{
			//	var cultureInfo = new CultureInfo("en-IN");
			//	cultureInfo.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
			//	cultureInfo.DateTimeFormat.LongDatePattern = "dd/MM/yyyy HH:mm:sss";

			//	var supportedCultures = new List<CultureInfo> { cultureInfo };

			//	options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-IN");

			//	options.DefaultRequestCulture.Culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
			//	options.DefaultRequestCulture.Culture.DateTimeFormat.LongDatePattern = "dd/MM/yyyy HH:mm:sss";

			//	options.SupportedCultures = supportedCultures;
			//	options.SupportedUICultures = supportedCultures;
			//});

			ConfigurationManager configuration = builder.Configuration; // allows both to access and to set up the config
			IWebHostEnvironment environment = builder.Environment;

			var culture = CultureInfo.CreateSpecificCulture("en-IN");

			var dateformat = new DateTimeFormatInfo { ShortDatePattern = "dd/MM/yyyy", LongDatePattern = "dd/MM/yyyy HH:mm:sss" };

			culture.DateTimeFormat = dateformat;

			var supportedCultures = new[] { culture };

			builder.Services.Configure<RequestLocalizationOptions>(options =>
			{
				options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(culture);
				options.SupportedCultures = supportedCultures;
				options.SupportedUICultures = supportedCultures;
			});

			builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(120); });

			builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			builder.Services.AddDbContextFactory<DataContext>(db => db.UseSqlServer(builder.Configuration.GetSection("DataConnection").Value), ServiceLifetime.Singleton);

			builder.Services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();


			var app = builder.Build();

			AppHttpContextAccessor.Configure(((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IHttpContextAccessor>(), ((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IHostEnvironment>(), environment, ((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IDataProtectionProvider>(), ((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IConfiguration>(), ((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IHttpClientFactory>());

			app.UseExceptionHandler("/Home/Error");
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseSession();
			app.UseRouting();

			app.UseAuthorization();

			app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

			app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

			app.MapControllerRoute(name: "qr", pattern: "{qr_code}", defaults: new { controller = "Home", action = "Get_QR_Code_Details" });

			app.Run();
		}
	}
}
