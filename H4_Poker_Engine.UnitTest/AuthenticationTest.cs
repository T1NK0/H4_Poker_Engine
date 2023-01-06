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
        //private TokenGenerator _tokenGenerator;

        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void GuestTokenShouldWork()
        {
            var mock = new Mock<TokenGenerator>();
            string loginToken = string.Empty;
            mock.Setup( x => x.GenerateLoginToken());

            Assert.That(loginToken, Is.Not.Null);
            Assert.That(loginToken, Is.Not.EqualTo(""));
        }
    }
}
