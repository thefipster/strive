using AwesomeAssertions;
using Fip.Strive.Core.Domain.Exceptions;

namespace Fip.Strive.Core.Domain.UnitTests.Exceptions
{
    public class UploadExceptionTests
    {
        [Fact]
        public void Type_should_derive_from_Exception()
        {
            typeof(UploadException).Should().BeAssignableTo<Exception>();
        }

        [Fact]
        public void String_only_constructor_message_should_contain_last_argument()
        {
            var type = typeof(UploadException);
            var ctor = type.GetConstructors()
                .FirstOrDefault(c => c.GetParameters().All(p => p.ParameterType == typeof(string)));

            ctor.Should().NotBeNull("expected a constructor that only accepts string parameters");

            var paramCount = ctor.GetParameters().Length;
            var args = Enumerable.Range(1, paramCount).Select(i => $"val{i}").ToArray<object>();

            var ex = (Exception)ctor.Invoke(args);
            ex.Message.Should().Contain((string)args.Last());
        }

        [Fact]
        public void Constructor_with_inner_exception_should_set_inner_exception_and_include_strings_in_message()
        {
            var type = typeof(UploadException);
            var ctor = type.GetConstructors()
                .FirstOrDefault(c => c.GetParameters().Length >= 1 && c.GetParameters().Last().ParameterType == typeof(Exception));

            ctor.Should().NotBeNull("expected a constructor whose last parameter is an Exception");

            var parameters = ctor.GetParameters();
            var args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length - 1; i++)
                args[i] = $"s{i + 1}";

            var inner = new InvalidOperationException("inner");
            args[args.Length - 1] = inner;

            var ex = (Exception)ctor.Invoke(args);
            ex.InnerException.Should().BeSameAs(inner);
            ex.Message.Should().Contain((string)args[Math.Max(0, args.Length - 2)]);
        }
    }
}
