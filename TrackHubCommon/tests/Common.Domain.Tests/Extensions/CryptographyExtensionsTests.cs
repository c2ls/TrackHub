using Common.Domain.Extensions;
using FluentAssertions;

namespace Common.Domain.Tests.Extensions;

public class CryptographyExtensionsTests
{
    [Fact]
    public void HashPassword_ReturnsNonEmptyHash()
    {
        var hash = "MyPassword123".HashPassword();
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe("MyPassword123");
    }

    [Fact]
    public void VerifyHashedPassword_CorrectPassword_ReturnsTrue()
    {
        var password = "TestPassword!@#";
        var hash = password.HashPassword();
        hash.VerifyHashedPassword(password).Should().BeTrue();
    }

    [Fact]
    public void VerifyHashedPassword_WrongPassword_ReturnsFalse()
    {
        var hash = "CorrectPassword".HashPassword();
        hash.VerifyHashedPassword("WrongPassword").Should().BeFalse();
    }

    [Theory]
    [InlineData(128)]
    [InlineData(192)]
    [InlineData(256)]
    public void GenerateAesKey_ValidKeySizes_ReturnsCorrectLength(int keySizeBits)
    {
        var key = CryptographyExtensions.GenerateAesKey(keySizeBits);
        key.Length.Should().Be(keySizeBits / 8);
    }

    [Theory]
    [InlineData(64)]
    [InlineData(512)]
    [InlineData(100)]
    public void GenerateAesKey_InvalidKeySize_ThrowsArgumentException(int keySizeBits)
    {
        var act = () => CryptographyExtensions.GenerateAesKey(keySizeBits);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateAesKey_ProducesDifferentKeysEachTime()
    {
        var key1 = CryptographyExtensions.GenerateAesKey(256);
        var key2 = CryptographyExtensions.GenerateAesKey(256);
        key1.Should().NotEqual(key2);
    }

    [Fact]
    public void DeriveKey_ReturnsConsistentKeyForSameInput()
    {
        var salt = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        var key1 = CryptographyExtensions.DeriveKey("passphrase", salt);
        var key2 = CryptographyExtensions.DeriveKey("passphrase", salt);
        key1.Should().Equal(key2);
    }

    [Fact]
    public void DeriveKey_DifferentPassphrases_DifferentKeys()
    {
        var salt = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        var key1 = CryptographyExtensions.DeriveKey("passphrase1", salt);
        var key2 = CryptographyExtensions.DeriveKey("passphrase2", salt);
        key1.Should().NotEqual(key2);
    }

    [Fact]
    public void EncryptData_DecryptData_RoundTrip()
    {
        var original = "Hello World! This is secret data.";
        var passphrase = "super-secret-passphrase";
        var salt = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

        var encrypted = original.EncryptData(passphrase, salt);
        encrypted.Should().NotBe(original);

        var decrypted = encrypted.DecryptData(passphrase, salt);
        decrypted.Should().Be(original);
    }

    [Fact]
    public void EncryptData_DifferentPassphrase_CannotDecrypt()
    {
        var original = "Sensitive info";
        var salt = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

        var encrypted = original.EncryptData("pass1", salt);
        var act = () => encrypted.DecryptData("pass2", salt);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void EncryptData_EmptyString_RoundTrip()
    {
        var original = "";
        var passphrase = "passphrase";
        var salt = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        var encrypted = original.EncryptData(passphrase, salt);
        var decrypted = encrypted.DecryptData(passphrase, salt);
        decrypted.Should().Be(original);
    }
}
