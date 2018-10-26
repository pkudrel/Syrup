using System;
using Autofac;
using MediatR;
using Syrup.Tests._Main;
using Xunit;

namespace Syrup.Tests.PlayGround
{
    public class SimpleTest : FixtureMediator
    {
        public SimpleTest() : base(BuildAction)
        {
        }

        private static void BuildAction(ContainerBuilder builder)
        {
            //builder.RegisterType<Test2>();
        }


        
    }
}