using JwtTokenAPI.Data;
using JwtTokenAPI.Models;
using JwtTokenAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var connexionString = builder.Configuration.GetConnectionString("PostgreSQLConnection");
var emailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
var roles = new[] { "admin", "mod", "member", "user" };

/***Add services to the container***/
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connexionString), ServiceLifetime.Scoped);

/***Add services for Identity and Roles***/
builder.Services.AddDefaultIdentity<User>(options => {
    options.SignIn.RequireConfirmedAccount = true;
    //options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
}).AddRoles<IdentityRole>()
  .AddEntityFrameworkStores<ApplicationDbContext>()
  .AddDefaultTokenProviders();

builder.Services.AddSingleton(emailConfig!);

/***My Own Services***/
builder.Services.AddScoped<IEmailservice, Emailservice>();
builder.Services.AddScoped<AuthorisationServices>();

/***Policies builder***/
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminAccess", policy => policy.RequireRole(roles[0]))
    .AddPolicy("ModsAccess", policy => policy.RequireAssertion(context => context.User.IsInRole(roles[0]) ||
                                                                          context.User.IsInRole(roles[1])))
    .AddPolicy("MemberAcces", policy => policy.RequireAssertion(context => context.User.IsInRole(roles[0]) ||
                                                                          context.User.IsInRole(roles[1]) ||
                                                                          context.User.IsInRole(roles[3])));
/***Injecting time in the recovery token***/
builder.Services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(24));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

var app = builder.Build();

app.UseCors(builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});

/***Configure the HTTP request pipeline.***/
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production
    // scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHttpsRedirection();
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRouting();
app.MapIdentityApi<User>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

/***Adding the role we want in the database***/
/***On Comments this sections when your connections string will be set***/
/*
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}
*/

app.Run();