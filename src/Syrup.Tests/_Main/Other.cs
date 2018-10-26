//using System;
//using Autofac;
//using Syrup.Core._Infrastructure.Misc;

//namespace Syrup.Tests._Main

//{
//    public class Fixture
//    {
//        private static readonly IContainer MainContainer = Ioc.Build();
//        private readonly TestLifetime _testLifetime = new TestLifetime(MainContainer);

//        [SetUp]
//        public void SetUp()
//        {
//            _testLifetime.SetUp();
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            _testLifetime.TearDown();
//        }

//        protected TService Resolve<TService>()
//        {
//            return _testLifetime.Resolve<TService>();
//        }

//        protected void Override(Action<ContainerBuilder> configurationAction)
//        {
//            _testLifetime.Override(configurationAction);
//        }
//    }

//    public class TestLifetime
//    {
//        private readonly IContainer _mainContainer;

//        private bool _canOverride;
//        private ILifetimeScope _testScope;

//        public TestLifetime(IContainer mainContainer)
//        {
//            _mainContainer = mainContainer;
//        }

//        public void SetUp()
//        {
//            _testScope = _mainContainer.BeginLifetimeScope();
//            _canOverride = true;
//        }

//        public void TearDown()
//        {
//            _testScope.Dispose();
//            _testScope = null;
//        }

//        public TService Resolve<TService>()
//        {
//            _canOverride = false;
//            return _testScope.Resolve<TService>();
//        }

//        public void Override(Action<ContainerBuilder> configurationAction)
//        {
//            _testScope.Dispose();

//            if (!_canOverride)
//                throw new InvalidOperationException("Override can only be called once per test and must be before any calls to Resolve.");

//            _canOverride = false;
//            _testScope = _mainContainer.BeginLifetimeScope(configurationAction);
//        }
//    }