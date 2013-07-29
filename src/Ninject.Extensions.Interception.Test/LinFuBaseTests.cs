#if !SILVERLIGHT
namespace Ninject.Extensions.Interception
{
    using FluentAssertions;
    using LinFu.DynamicProxy;
    using Ninject.Extensions.Interception.Fakes;
    using Ninject.Extensions.Interception.Infrastructure.Language;
    using Ninject.Extensions.Interception.Interceptors;
    using Xunit;
    
    public class LinFuBaseTests : LinFuInterceptionContext
    {
        [Fact]
        public void SelfBoundTypesDeclaringInterceptorsOnGenericMethodsAreIntercepted()
        {
            using (var kernel = CreateDefaultInterceptionKernel())
            {
                kernel.Bind<ObjectWithGenericMethod>().ToSelf();
                var obj = kernel.Get<ObjectWithGenericMethod>();
                obj.Should().NotBeNull();
                typeof(IProxy).IsAssignableFrom(obj.GetType()).Should().BeTrue();

                FlagInterceptor.Reset();
                string result = obj.ConvertGeneric("", 42);

                result.Should().Be("42");
                FlagInterceptor.WasCalled.Should().BeTrue();
            }
        }

        [Fact]
        public void ServiceBoundTypesDeclaringMethodInterceptorsAreProxied()
        {
            using (var kernel = CreateDefaultInterceptionKernel())
            {
                kernel.Bind<IFoo>().To<ObjectWithMethodInterceptor>();
                var obj = kernel.Get<IFoo>();
                obj.Should().NotBeNull();
                typeof(IProxy).IsAssignableFrom(obj.GetType()).Should().BeTrue();
            }
        }

        [Fact]
        public void ServiceBoundTypesDeclaringMethodInterceptorsAreIntercepted()
        {
            using (var kernel = CreateDefaultInterceptionKernel())
            {
                kernel.Bind<IFoo>().To<ObjectWithMethodInterceptor>();
                var obj = kernel.Get<IFoo>();
                obj.Should().NotBeNull();
                typeof(IProxy).IsAssignableFrom(obj.GetType()).Should().BeTrue();

                CountInterceptor.Reset();

                obj.Foo();

                CountInterceptor.Count.Should().Be(1);
            }
        }

        [Fact]
        public void ServiceBoundTypesDeclaringInterceptorsOnGenericMethodsAreIntercepted()
        {
            using (var kernel = CreateDefaultInterceptionKernel())
            {
                kernel.Bind<IGenericMethod>().To<ObjectWithGenericMethod>();
                var obj = kernel.Get<IGenericMethod>();
                obj.Should().NotBeNull();
                typeof(IProxy).IsAssignableFrom(obj.GetType()).Should().BeTrue();

                FlagInterceptor.Reset();

                string result = obj.ConvertGeneric("", 42);

                result.Should().Be("42");
                FlagInterceptor.WasCalled.Should().BeTrue();
            }
        }

        [Fact]
        public void SelfBoundTypesThatAreProxiedReceiveConstructorInjections()
        {
            using (var kernel = CreateDefaultInterceptionKernel())
            {
                kernel.Bind<RequestsConstructorInjection>().ToSelf();
                // This is just here to trigger proxying, but we won't intercept any calls
                kernel.Intercept( ( request ) => true ).With<FlagInterceptor>();

                var obj = kernel.Get<RequestsConstructorInjection>();

                typeof(IProxy).IsAssignableFrom(obj.GetType()).Should().BeTrue();
                obj.Child.Should().NotBeNull();
            }
        }

        [Fact]
        public void SingletonTests()
        {
            using (var kernel = CreateDefaultInterceptionKernel())
            {
                kernel.Bind<RequestsConstructorInjection>().ToSelf().InSingletonScope();
                // This is just here to trigger proxying, but we won't intercept any calls
                kernel.Intercept(request => true).With<FlagInterceptor>();

                var obj = kernel.Get<RequestsConstructorInjection>();

                obj.Should().NotBeNull();
                typeof(IProxy).IsAssignableFrom(obj.GetType()).Should().BeTrue();
                obj.Child.Should().NotBeNull();
            }
        }

        [Fact]
        public void SinsgletonTests()
        {
            using (var kernel = CreateDefaultInterceptionKernel())
            {
                kernel.Bind<RequestsConstructorInjection>().ToSelf().InSingletonScope().Intercept().With<FlagInterceptor>();
                var obj = kernel.Get<RequestsConstructorInjection>();

                obj.Should().NotBeNull();
                typeof(IProxy).IsAssignableFrom(obj.GetType()).Should().BeTrue();
                FlagInterceptor.Reset();

                obj.Child.Should().NotBeNull();
                FlagInterceptor.WasCalled.Should().BeTrue();
            }
        }
    }
}
#endif