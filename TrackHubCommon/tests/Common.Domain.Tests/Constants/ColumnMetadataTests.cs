using Common.Domain.Constants;
using FluentAssertions;

namespace Common.Domain.Tests.Constants;

public class ColumnMetadataTests
{
    [Fact]
    public void DefaultFieldLength_Is100() => ColumnMetadata.DefaultFieldLength.Should().Be(100);

    [Fact]
    public void DefaultNameLength_Is200() => ColumnMetadata.DefaultNameLength.Should().Be(200);

    [Fact]
    public void DefaultDescriptionLength_Is500() => ColumnMetadata.DefaultDescriptionLength.Should().Be(500);

    [Fact]
    public void DefaultUserNameLength_Is200() => ColumnMetadata.DefaultUserNameLength.Should().Be(200);

    [Fact]
    public void DefaultEmailLength_Is200() => ColumnMetadata.DefaultEmailLength.Should().Be(200);

    [Fact]
    public void DefaultTokenLength_Is200() => ColumnMetadata.DefaultTokenLength.Should().Be(200);

    [Fact]
    public void DefaultPasswordLength_Is200() => ColumnMetadata.DefaultPasswordLength.Should().Be(200);

    [Fact]
    public void DefaultAddressLength_Is250() => ColumnMetadata.DefaultAddressLength.Should().Be(250);

    [Fact]
    public void DefaultPhoneNumberLength_Is200() => ColumnMetadata.DefaultPhoneNumberLength.Should().Be(200);

    [Fact]
    public void MinimumPasswordLength_Is8() => ColumnMetadata.MinimumPasswordLength.Should().Be(8);

    [Fact]
    public void TextField_IsText() => ColumnMetadata.TextField.Should().Be("text");
}
