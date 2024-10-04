using Microsoft.AspNetCore.Mvc.Diagnostics;
using miminal_api.Dominio.Entidades;

namespace Test.Domain.Entidade;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {

        // Arrange 
        var adm = new Administrador();
        var veiculos = new Veiculo();


        // Actions
        adm.Id = 1;
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "Adm";


        /*
        veiculos.Id = 1;
        veiculos.Merca = "Toyota";
        veiculos.Nome = "Corolla";
        int anoVeiculo = int.Parse(DateTime.Now.ToLongDateString());
        veiculos.Ano = anoVeiculo;
        */
        // Assert 

        Assert.AreEqual(1, adm.Id);
        Assert.AreEqual("teste@teste.com", adm.Email);
        Assert.AreEqual("teste", adm.Senha);
        Assert.AreEqual("Adm", adm.Perfil);


        /*
        Assert.AreEqual(1, veiculos.Id);
        Assert.AreEqual("Toyota", veiculos.Merca);
        Assert.AreEqual("Corolla", veiculos.Nome);
        Assert.AreEqual(anoVeiculo, veiculos.Ano);
        */
    }
}