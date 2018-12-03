//------------------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Swashbuckle.AspNetCore.Swagger;

using HolidayExtras.Models;

//------------------------------------------------------------------------------------------------------------------------------

namespace HolidayExtras
{
	//--------------------------------------------------------------------------------------------------------------------------

	public class Startup
	{
		//----------------------------------------------------------------------------------------------------------------------

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<DbaseContext>(opt => opt.UseInMemoryDatabase("HolidayExtras"));
			services.AddMvc();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Info { Title = "Holiday Extras Web Development API Task", Version = "v1" });

				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				c.IncludeXmlComments(xmlPath);
			});
		}

		//----------------------------------------------------------------------------------------------------------------------

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			if (env.IsDevelopment())
			{
				loggerFactory.AddConsole().AddDebug();

				app.UseDeveloperExceptionPage();
			}

			app.UseMvc();
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Version 1");
			});
		}

		//----------------------------------------------------------------------------------------------------------------------
	}

	//--------------------------------------------------------------------------------------------------------------------------
}

//------------------------------------------------------------------------------------------------------------------------------
