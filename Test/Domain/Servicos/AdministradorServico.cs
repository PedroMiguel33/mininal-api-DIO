using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using miminal_api.Dominio.Entidades;
using miminal_api.Dominio.Servicos;
using miminal_api.Infraestrutura.Db;

namespace Test.Domain.Servicos
{
[TestClass]
public class AdministradorServicoTest
{
    private DbContexto CriarContextoTeste()
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));

            var builder = new ConfigurationBuilder()
            .SetBasePath(path ?? Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        return new DbContexto(configuration);
    }

    [TestMethod]
    public void TestandoSalvarAdministrador()
    {

        // Arrange
        var context = CriarContextoTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

        var adm = new Administrador();
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "Adm";

        var administradorServico = new AdministradorServico(context);

        // Actions
        administradorServico.Salvar(adm);

        // Assert
        Assert.AreEqual(1, administradorServico.Todos(1).Count());

    }

    [TestMethod]
    public void TestandoBuscaPorId()
    {
        // Arrange
        var context = CriarContextoTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

        var adm = new Administrador();
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "Adm";

        var administradorServico = new AdministradorServico(context);

        // Act
        administradorServico.Salvar(adm);
        var admDoBanco = administradorServico.BuscarPorId(adm.Id);

        // Assert
        Assert.AreEqual(1, admDoBanco?.Id);
    }
    
}
}