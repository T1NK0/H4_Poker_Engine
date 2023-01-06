using H4_Poker_Engine.Authentication;
using H4_Poker_Engine.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H4_Poker_Engine.UnitTest
{
    internal class LoginTest
    {
        private IConfiguration _configuration;
        private TokenGenerator _tokenGenerator;
        private LoginController _loginController;
        [SetUp]
        public void Setup()
        {
            _configuration = new ConfigurationBuilder()
                .AddUserSecrets<LoginTest>()
                .AddEnvironmentVariables()
                .Build();
            _tokenGenerator = new TokenGenerator(_configuration);
            _loginController = new LoginController(_tokenGenerator);
        }

        [Test]
        public void Test1()
        {
            // Arrange
            var test = _loginController.GetLoginToken();
            //controller.Request = new HttpRequestMessage();
            //controller.Configuration = new HttpConfiguration();

            // Act
            //var response = controller.Get();

            // Assert
            //Assert.IsTrue(response.TryGetContentValue<Product>(out product));
            Assert.That(test == test);
        }
    }
}
