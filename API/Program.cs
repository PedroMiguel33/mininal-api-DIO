using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using miminal_api.Dominio.DTOs;
using miminal_api.Dominio.Interfaces;
using miminal_api.Dominio.Servicos;
using miminal_api.Infraestrutura.Db;
using miminal_api.Dominio.ModelsViews;
using miminal_api.Dominio.Entidades;
using miminal_api.Dominio.Enums;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;


#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();
if (string.IsNullOrEmpty(key))
{
    key = "12345";
}


builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token do JWT "
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
         new string[] {}
    } });
});


builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    );
});


var app = builder.Build();

#endregion

#region Home

app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");

#endregion

#region Administradores

string GerarTokekJwt(Administrador administrador)
{
    if (string.IsNullOrEmpty(key))
    {
        return string.Empty;
    }
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim("Email", administrador.Email),
        new Claim("Perfil", administrador.Perfil),
        new Claim(ClaimTypes.Role, administrador.Perfil),
    };
    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
        );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{

    var adm = administradorServico.Login(loginDTO);

    if (adm != null)
    {
        string token = GerarTokekJwt(adm);
        return Results.Ok(new AdministradorLogado
        {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    }
    else
    {
        return Results.Unauthorized();
    }

}).WithTags("Administradores");



app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{

    var adms = new List<AdministradorModelView>();
    var administradores = administradorServico.Todos(pagina);
    foreach (var adm in administradores)
    {
        adms.Add(new AdministradorModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }
    return Results.Ok(adms);

}).AllowAnonymous().RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");


app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscarPorId(id);
    if (administrador == null) return Results.NotFound();
    return Results.Ok(new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");


app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var validacao = new ErrosDeValidacao { Mensagens = new List<string>() };

    if (string.IsNullOrEmpty(administradorDTO.Email)) { validacao.Mensagens.Add("Email não podo está vazio."); }
    if (string.IsNullOrEmpty(administradorDTO.Senha)) { validacao.Mensagens.Add("Senha não podo está vazia."); }
    if (administradorDTO.Pefil == null) { validacao.Mensagens.Add("Perfil não podo está vazio."); }

    if (validacao.Mensagens.Count() > 0)
    {
        return Results.BadRequest(validacao);
    }

    var administrador = new Administrador
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Pefil.ToString() ?? Perfil.Editor.ToString(),
    };
    administradorServico.Salvar(administrador);

    return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });

}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");

#endregion

#region Veiculos

ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{

    var validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };


    if (string.IsNullOrEmpty(veiculoDTO.Nome))
        validacao.Mensagens.Add("O nome não pode estar vázio.");

    if (string.IsNullOrEmpty(veiculoDTO.Marca))
        validacao.Mensagens.Add("A marca não pode estar vázia.");

    if (veiculoDTO.Ano < 1940)
        validacao.Mensagens.Add("Favor colocar veículos de fabricação superior a 1940 (Mil novecentos e quarenta)");

    return validacao;
}

// CRIAR 
app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{

    var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count() > 0)
    {
        return Results.BadRequest(validacao);
    }

    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Merca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano,
    };
    veiculoServico.Salvar(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" }).WithTags("Veiculos");


// BUSCAR (Por páginas)
app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);

    return Results.Ok(veiculos);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" }).WithTags("Veiculos");


//ALTERAR 
app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{

    var veiculos = veiculoServico.BuscarPorId(id);
    if (veiculos == null) { return Results.NotFound(); }


    var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count() > 0)
    {
        return Results.BadRequest(validacao);
    }

    veiculos.Nome = veiculoDTO.Nome;
    veiculos.Merca = veiculoDTO.Marca;
    veiculos.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculos);

    return Results.Ok(veiculos);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Veiculos");


//DELETAR
app.MapDelete("/veiculos", ([FromQuery] int id, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.BuscarPorId(id);
    if (veiculos == null) { return Results.NotFound(); }

    veiculoServico.Apagar(veiculos);

    return Results.NoContent();
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Veiculos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

//1. Autenticação | 2. Autorização
app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion