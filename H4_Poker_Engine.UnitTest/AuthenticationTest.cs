using H4_Poker_Engine.Authentication;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H4_Poker_Engine.UnitTest
{
    public class AuthenticationTest
    {
        private IConfiguration _configuration;
        private TokenGenerator _tokenGenerator;

        [SetUp]
        public void Setup()
        {
            _configuration = new ConfigurationBuilder()
                .AddUserSecrets<AuthenticationTest>()
                .AddEnvironmentVariables()
                .Build();
            _tokenGenerator = new TokenGenerator(_configuration);
        }

        [Test]
        public void GuestTokenShouldWork()
        {
            
            var loginToken = _tokenGenerator.GenerateLoginToken();
            Thread.Sleep(500);
            var newLoginToken = _tokenGenerator.GenerateLoginToken();

            Assert.That(loginToken, Is.Not.Null);
            Assert.That(loginToken, Is.Not.EqualTo(""));
            Assert.That(newLoginToken, Is.Not.Null);
            Assert.That(newLoginToken, Is.Not.EqualTo(""));
            Assert.That(loginToken, Is.Not.EqualTo(newLoginToken));
        }
    }
}
