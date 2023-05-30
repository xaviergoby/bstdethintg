namespace Hodl.Crypto.UnitTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    [TestCase("", false)]
    [TestCase("Some text", false)]
    [TestCase("1RAHUEYstWetqabcFn5Au4m4GFg7xJaNVN2", true)] // Bitcoin / Bitcoin Cash legacy
    [TestCase("3J98t1RHT73CNmQwertyyWrnqRhWNLy", true)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e", true)]// Bitcoin
    [TestCase("0x1F4BA909F005a761e045E6385E671C699a904414", false)] // Ethereum / Avalaunche / Polygon
    [TestCase("XuU3CEUUANJYDPJ8ayCKDQo51TJ4Xp3Z3x", false)] // Dash
    [TestCase("888tNkZrPN6JsEgekjMnABU4TBzc2Dt29EPAvkRxbANsAnjyPbb3iQ1YBRk1UXcdRsiKc9dhwMVgN5S9cQUiyoogDavup3H", false)] // Monero
    [TestCase("32uLhn19ZasD5bsVhLdDthhM37JhJHiEE2", true)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", false)] // Bitcoin Cash
    [TestCase("MGxNPPB7eBoWPUaprtX9v9CXJZoD2465zN", false)] // LiteCoin
    [TestCase("bnb1fnd0k5l4p3ck2j9x9dp36chk059w977pszdgdz", false)] // BNB BEP20
    [TestCase("X-avax1tzdcgj4ehsvhhgpl7zylwpw0gl2rxcg4r5afk5", false)] // Avalaunche
    [TestCase("HN7cABqLq46Es1jh92dQQisAq662SmxELLLsHHe4YWrH", false)] // Solana
    [TestCase("253dfc2d-ac8b-413b-9a8b-095a1a7b621b:290e7851-7f6f-4827-a980-ce465dc38676", false)] // Exchange Wallet
    [TestCase("test.near", false)] // NEAR Wallet
    [TestCase("test.testnet", false)] // NEAR Wallet
    [TestCase("123456789012345678901234567890123456789012345678901234567890abcd", false)] // NEAR Wallet
    public void IsBitcoinAddressTest(string test, bool expected)
    {
        var result = WalletFormat.IsBitcoinAddress(test);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", false)]
    [TestCase("Some text", false)]
    [TestCase("1RAHUEYstWetqabcFn5Au4m4GFg7xJaNVN2", false)] // Bitcoin / Bitcoin Cash legacy
    [TestCase("3J98t1RHT73CNmQwertyyWrnqRhWNLy", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e", false)]// Bitcoin
    [TestCase("0x1F4BA909F005a761e045E6385E671C699a904414", true)] // Ethereum / Avalaunche / Polygon / Bnb Bep20
    [TestCase("XuU3CEUUANJYDPJ8ayCKDQo51TJ4Xp3Z3x", false)] // Dash
    [TestCase("888tNkZrPN6JsEgekjMnABU4TBzc2Dt29EPAvkRxbANsAnjyPbb3iQ1YBRk1UXcdRsiKc9dhwMVgN5S9cQUiyoogDavup3H", false)] // Monero
    [TestCase("32uLhn19ZasD5bsVhLdDthhM37JhJHiEE2", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", false)] // Bitcoin Cash
    [TestCase("MGxNPPB7eBoWPUaprtX9v9CXJZoD2465zN", false)] // LiteCoin
    [TestCase("bnb1fnd0k5l4p3ck2j9x9dp36chk059w977pszdgdz", false)] // BNB BEP20
    [TestCase("X-avax1tzdcgj4ehsvhhgpl7zylwpw0gl2rxcg4r5afk5", false)] // Avalaunche
    [TestCase("HN7cABqLq46Es1jh92dQQisAq662SmxELLLsHHe4YWrH", false)] // Solana
    [TestCase("253dfc2d-ac8b-413b-9a8b-095a1a7b621b:290e7851-7f6f-4827-a980-ce465dc38676", false)] // Exchange Wallet
    [TestCase("test.near", false)] // NEAR Wallet
    [TestCase("test.testnet", false)] // NEAR Wallet
    [TestCase("123456789012345678901234567890123456789012345678901234567890abcd", false)] // NEAR Wallet
    public void IsEthereumAddressTest(string test, bool expected)
    {
        var result = WalletFormat.IsEthereumAddress(test);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", false)]
    [TestCase("Some text", false)]
    [TestCase("1RAHUEYstWetqabcFn5Au4m4GFg7xJaNVN2", false)] // Bitcoin / Bitcoin Cash legacy
    [TestCase("3J98t1RHT73CNmQwertyyWrnqRhWNLy", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e", false)]// Bitcoin
    [TestCase("0x1F4BA909F005a761e045E6385E671C699a904414", false)] // Ethereum / Avalaunche / Polygon / Bnb Bep20
    [TestCase("XuU3CEUUANJYDPJ8ayCKDQo51TJ4Xp3Z3x", true)] // Dash
    [TestCase("888tNkZrPN6JsEgekjMnABU4TBzc2Dt29EPAvkRxbANsAnjyPbb3iQ1YBRk1UXcdRsiKc9dhwMVgN5S9cQUiyoogDavup3H", false)] // Monero
    [TestCase("32uLhn19ZasD5bsVhLdDthhM37JhJHiEE2", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", false)] // Bitcoin Cash
    [TestCase("MGxNPPB7eBoWPUaprtX9v9CXJZoD2465zN", false)] // LiteCoin
    [TestCase("bnb1fnd0k5l4p3ck2j9x9dp36chk059w977pszdgdz", false)] // BNB BEP20
    [TestCase("X-avax1tzdcgj4ehsvhhgpl7zylwpw0gl2rxcg4r5afk5", false)] // Avalaunche
    [TestCase("HN7cABqLq46Es1jh92dQQisAq662SmxELLLsHHe4YWrH", false)] // Solana
    [TestCase("253dfc2d-ac8b-413b-9a8b-095a1a7b621b:290e7851-7f6f-4827-a980-ce465dc38676", false)] // Exchange Wallet
    [TestCase("test.near", false)] // NEAR Wallet
    [TestCase("test.testnet", false)] // NEAR Wallet
    [TestCase("123456789012345678901234567890123456789012345678901234567890abcd", false)] // NEAR Wallet
    public void IsDashAddressTest(string test, bool expected)
    {
        var result = WalletFormat.IsDashAddress(test);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", false)]
    [TestCase("Some text", false)]
    [TestCase("1RAHUEYstWetqabcFn5Au4m4GFg7xJaNVN2", false)] // Bitcoin / Bitcoin Cash legacy
    [TestCase("3J98t1RHT73CNmQwertyyWrnqRhWNLy", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e", false)]// Bitcoin
    [TestCase("0x1F4BA909F005a761e045E6385E671C699a904414", false)] // Ethereum / Avalaunche / Polygon / Bnb Bep20
    [TestCase("XuU3CEUUANJYDPJ8ayCKDQo51TJ4Xp3Z3x", false)] // Dash
    [TestCase("888tNkZrPN6JsEgekjMnABU4TBzc2Dt29EPAvkRxbANsAnjyPbb3iQ1YBRk1UXcdRsiKc9dhwMVgN5S9cQUiyoogDavup3H", true)] // Monero
    [TestCase("32uLhn19ZasD5bsVhLdDthhM37JhJHiEE2", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", false)] // Bitcoin Cash
    [TestCase("MGxNPPB7eBoWPUaprtX9v9CXJZoD2465zN", false)] // LiteCoin
    [TestCase("bnb1fnd0k5l4p3ck2j9x9dp36chk059w977pszdgdz", false)] // BNB BEP20
    [TestCase("X-avax1tzdcgj4ehsvhhgpl7zylwpw0gl2rxcg4r5afk5", false)] // Avalaunche
    [TestCase("HN7cABqLq46Es1jh92dQQisAq662SmxELLLsHHe4YWrH", false)] // Solana
    [TestCase("253dfc2d-ac8b-413b-9a8b-095a1a7b621b:290e7851-7f6f-4827-a980-ce465dc38676", false)] // Exchange Wallet
    [TestCase("test.near", false)] // NEAR Wallet
    [TestCase("test.testnet", false)] // NEAR Wallet
    [TestCase("123456789012345678901234567890123456789012345678901234567890abcd", false)] // NEAR Wallet
    public void IsMoneroAddressTest(string test, bool expected)
    {
        var result = WalletFormat.IsMoneroAddress(test);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", false)]
    [TestCase("Some text", false)]
    [TestCase("1RAHUEYstWetqabcFn5Au4m4GFg7xJaNVN2", true)] // Bitcoin / Bitcoin Cash legacy
    [TestCase("3J98t1RHT73CNmQwertyyWrnqRhWNLy", true)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e", false)]// Bitcoin
    [TestCase("0x1F4BA909F005a761e045E6385E671C699a904414", false)] // Ethereum / Avalaunche / Polygon / Bnb Bep20
    [TestCase("XuU3CEUUANJYDPJ8ayCKDQo51TJ4Xp3Z3x", false)] // Dash
    [TestCase("888tNkZrPN6JsEgekjMnABU4TBzc2Dt29EPAvkRxbANsAnjyPbb3iQ1YBRk1UXcdRsiKc9dhwMVgN5S9cQUiyoogDavup3H", false)] // Monero
    [TestCase("32uLhn19ZasD5bsVhLdDthhM37JhJHiEE2", true)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", true)] // Bitcoin Cash
    [TestCase("bitcoincash:pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", true)] // Bitcoin Cash
    [TestCase("MGxNPPB7eBoWPUaprtX9v9CXJZoD2465zN", false)] // LiteCoin
    [TestCase("bnb1fnd0k5l4p3ck2j9x9dp36chk059w977pszdgdz", false)] // BNB BEP20
    [TestCase("X-avax1tzdcgj4ehsvhhgpl7zylwpw0gl2rxcg4r5afk5", false)] // Avalaunche
    [TestCase("HN7cABqLq46Es1jh92dQQisAq662SmxELLLsHHe4YWrH", false)] // Solana
    [TestCase("253dfc2d-ac8b-413b-9a8b-095a1a7b621b:290e7851-7f6f-4827-a980-ce465dc38676", false)] // Exchange Wallet
    [TestCase("test.near", false)] // NEAR Wallet
    [TestCase("test.testnet", false)] // NEAR Wallet
    [TestCase("123456789012345678901234567890123456789012345678901234567890abcd", false)] // NEAR Wallet
    public void IsBitcoinCashAddressTest(string test, bool expected)
    {
        var result = WalletFormat.IsBitcoinCashAddress(test);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", false)]
    [TestCase("Some text", false)]
    [TestCase("1RAHUEYstWetqabcFn5Au4m4GFg7xJaNVN2", false)] // Bitcoin / Bitcoin Cash legacy
    [TestCase("3J98t1RHT73CNmQwertyyWrnqRhWNLy", true)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e", false)]// Bitcoin
    [TestCase("0x1F4BA909F005a761e045E6385E671C699a904414", false)] // Ethereum / Avalaunche / Polygon / Bnb Bep20
    [TestCase("XuU3CEUUANJYDPJ8ayCKDQo51TJ4Xp3Z3x", false)] // Dash
    [TestCase("888tNkZrPN6JsEgekjMnABU4TBzc2Dt29EPAvkRxbANsAnjyPbb3iQ1YBRk1UXcdRsiKc9dhwMVgN5S9cQUiyoogDavup3H", false)] // Monero
    [TestCase("32uLhn19ZasD5bsVhLdDthhM37JhJHiEE2", true)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", false)] // Bitcoin Cash
    [TestCase("MGxNPPB7eBoWPUaprtX9v9CXJZoD2465zN", true)] // LiteCoin
    [TestCase("bnb1fnd0k5l4p3ck2j9x9dp36chk059w977pszdgdz", false)] // BNB BEP20
    [TestCase("X-avax1tzdcgj4ehsvhhgpl7zylwpw0gl2rxcg4r5afk5", false)] // Avalaunche
    [TestCase("HN7cABqLq46Es1jh92dQQisAq662SmxELLLsHHe4YWrH", false)] // Solana
    [TestCase("253dfc2d-ac8b-413b-9a8b-095a1a7b621b:290e7851-7f6f-4827-a980-ce465dc38676", false)] // Exchange Wallet
    [TestCase("test.near", false)] // NEAR Wallet
    [TestCase("test.testnet", false)] // NEAR Wallet
    [TestCase("123456789012345678901234567890123456789012345678901234567890abcd", false)] // NEAR Wallet
    public void IsLitecoinAddressTest(string test, bool expected)
    {
        var result = WalletFormat.IsLitecoinAddress(test);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", false)]
    [TestCase("Some text", false)]
    [TestCase("1RAHUEYstWetqabcFn5Au4m4GFg7xJaNVN2", false)] // Bitcoin / Bitcoin Cash legacy
    [TestCase("3J98t1RHT73CNmQwertyyWrnqRhWNLy", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e", false)]// Bitcoin
    [TestCase("0x1F4BA909F005a761e045E6385E671C699a904414", true)] // Ethereum / Avalaunche / Polygon / Bnb Bep20
    [TestCase("XuU3CEUUANJYDPJ8ayCKDQo51TJ4Xp3Z3x", false)] // Dash
    [TestCase("888tNkZrPN6JsEgekjMnABU4TBzc2Dt29EPAvkRxbANsAnjyPbb3iQ1YBRk1UXcdRsiKc9dhwMVgN5S9cQUiyoogDavup3H", false)] // Monero
    [TestCase("32uLhn19ZasD5bsVhLdDthhM37JhJHiEE2", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", false)] // Bitcoin Cash
    [TestCase("MGxNPPB7eBoWPUaprtX9v9CXJZoD2465zN", false)] // LiteCoin
    [TestCase("bnb1fnd0k5l4p3ck2j9x9dp36chk059w977pszdgdz", true)] // BNB BEP20
    [TestCase("X-avax1tzdcgj4ehsvhhgpl7zylwpw0gl2rxcg4r5afk5", false)] // Avalaunche
    [TestCase("HN7cABqLq46Es1jh92dQQisAq662SmxELLLsHHe4YWrH", false)] // Solana
    [TestCase("253dfc2d-ac8b-413b-9a8b-095a1a7b621b:290e7851-7f6f-4827-a980-ce465dc38676", false)] // Exchange Wallet
    [TestCase("test.near", false)] // NEAR Wallet
    [TestCase("test.testnet", false)] // NEAR Wallet
    [TestCase("123456789012345678901234567890123456789012345678901234567890abcd", false)] // NEAR Wallet
    public void IsBnbBep20AddressTest(string test, bool expected)
    {
        var result = WalletFormat.IsBnbBep20Address(test);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", false)]
    [TestCase("Some text", false)]
    [TestCase("1RAHUEYstWetqabcFn5Au4m4GFg7xJaNVN2", false)] // Bitcoin / Bitcoin Cash legacy
    [TestCase("3J98t1RHT73CNmQwertyyWrnqRhWNLy", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e", false)]// Bitcoin
    [TestCase("0x1F4BA909F005a761e045E6385E671C699a904414", true)] // Ethereum / Avalaunche
    [TestCase("XuU3CEUUANJYDPJ8ayCKDQo51TJ4Xp3Z3x", false)] // Dash
    [TestCase("888tNkZrPN6JsEgekjMnABU4TBzc2Dt29EPAvkRxbANsAnjyPbb3iQ1YBRk1UXcdRsiKc9dhwMVgN5S9cQUiyoogDavup3H", false)] // Monero
    [TestCase("32uLhn19ZasD5bsVhLdDthhM37JhJHiEE2", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", false)] // Bitcoin Cash
    [TestCase("MGxNPPB7eBoWPUaprtX9v9CXJZoD2465zN", false)] // LiteCoin
    [TestCase("bnb1fnd0k5l4p3ck2j9x9dp36chk059w977pszdgdz", false)] // BNB BEP20
    [TestCase("X-avax1tzdcgj4ehsvhhgpl7zylwpw0gl2rxcg4r5afk5", true)] // Avalaunche
    [TestCase("HN7cABqLq46Es1jh92dQQisAq662SmxELLLsHHe4YWrH", false)] // Solana
    [TestCase("253dfc2d-ac8b-413b-9a8b-095a1a7b621b:290e7851-7f6f-4827-a980-ce465dc38676", false)] // Exchange Wallet
    [TestCase("test.near", false)] // NEAR Wallet
    [TestCase("test.testnet", false)] // NEAR Wallet
    [TestCase("123456789012345678901234567890123456789012345678901234567890abcd", false)] // NEAR Wallet
    public void IsAvaxAddressTest(string test, bool expected)
    {
        var result = WalletFormat.IsAvaxAddress(test);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", false)]
    [TestCase("Some text", false)]
    [TestCase("1RAHUEYstWetqabcFn5Au4m4GFg7xJaNVN2", true)] // Bitcoin / Bitcoin Cash legacy / (Solana)
    [TestCase("3J98t1RHT73CNmQwertyyWrnqRhWNLy", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e", false)]// Bitcoin
    [TestCase("0x1F4BA909F005a761e045E6385E671C699a904414", false)] // Ethereum / Avalaunche
    [TestCase("XuU3CEUUANJYDPJ8ayCKDQo51TJ4Xp3Z3x", true)] // Dash / (Solana)
    [TestCase("888tNkZrPN6JsEgekjMnABU4TBzc2Dt29EPAvkRxbANsAnjyPbb3iQ1YBRk1UXcdRsiKc9dhwMVgN5S9cQUiyoogDavup3H", false)] // Monero
    [TestCase("32uLhn19ZasD5bsVhLdDthhM37JhJHiEE2", true)] // Bitcoin / Bitcoin Cash legacy / Litecoin / (Solana)
    [TestCase("pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", true)] // Bitcoin Cash / (Solana)
    [TestCase("MGxNPPB7eBoWPUaprtX9v9CXJZoD2465zN", true)] // LiteCoin / (Solana)
    [TestCase("bnb1fnd0k5l4p3ck2j9x9dp36chk059w977pszdgdz", false)] // BNB BEP20
    [TestCase("X-avax1tzdcgj4ehsvhhgpl7zylwpw0gl2rxcg4r5afk5", false)] // Avalaunche
    [TestCase("HN7cABqLq46Es1jh92dQQisAq662SmxELLLsHHe4YWrH", true)] // Solana
    [TestCase("253dfc2d-ac8b-413b-9a8b-095a1a7b621b:290e7851-7f6f-4827-a980-ce465dc38676", false)] // Exchange Wallet
    [TestCase("test.near", false)] // NEAR Wallet
    [TestCase("test.testnet", false)] // NEAR Wallet
    [TestCase("123456789012345678901234567890123456789012345678901234567890abcd", false)] // NEAR Wallet
    public void IsSolanaAddressTest(string test, bool expected)
    {
        var result = WalletFormat.IsSolanaAddress(test);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", false)]
    [TestCase("Some text", false)]
    [TestCase("1RAHUEYstWetqabcFn5Au4m4GFg7xJaNVN2", false)] // Bitcoin / Bitcoin Cash legacy / (Solana)
    [TestCase("3J98t1RHT73CNmQwertyyWrnqRhWNLy", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e", false)]// Bitcoin
    [TestCase("0x1F4BA909F005a761e045E6385E671C699a904414", false)] // Ethereum / Avalaunche
    [TestCase("XuU3CEUUANJYDPJ8ayCKDQo51TJ4Xp3Z3x", false)] // Dash / (Solana)
    [TestCase("888tNkZrPN6JsEgekjMnABU4TBzc2Dt29EPAvkRxbANsAnjyPbb3iQ1YBRk1UXcdRsiKc9dhwMVgN5S9cQUiyoogDavup3H", false)] // Monero
    [TestCase("32uLhn19ZasD5bsVhLdDthhM37JhJHiEE2", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin / (Solana)
    [TestCase("pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", false)] // Bitcoin Cash / (Solana)
    [TestCase("MGxNPPB7eBoWPUaprtX9v9CXJZoD2465zN", false)] // LiteCoin / (Solana)
    [TestCase("bnb1fnd0k5l4p3ck2j9x9dp36chk059w977pszdgdz", false)] // BNB BEP20
    [TestCase("X-avax1tzdcgj4ehsvhhgpl7zylwpw0gl2rxcg4r5afk5", false)] // Avalaunche
    [TestCase("HN7cABqLq46Es1jh92dQQisAq662SmxELLLsHHe4YWrH", false)] // Solana
    [TestCase("253dfc2d-ac8b-413b-9a8b-095a1a7b621b:290e7851-7f6f-4827-a980-ce465dc38676", false)] // Exchange Wallet
    [TestCase("test.near", true)] // NEAR Wallet
    [TestCase("test.testnet", true)] // NEAR Wallet
    [TestCase("123456789012345678901234567890123456789012345678901234567890abcd", true)] // NEAR Wallet
    [TestCase("shouldnotendwithperiod.testnet.", false)] // NEAR Wallet
    [TestCase("space should fail.testnet", false)] // NEAR Wallet
    [TestCase("touchingDotsShouldfail..testnet", false)] // NEAR Wallet
    public void IsNearAddressTest(string test, bool expected)
    {
        var result = WalletFormat.IsNearAddress(test);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", false)]
    [TestCase("Some text", false)]
    [TestCase("1RAHUEYstWetqabcFn5Au4m4GFg7xJaNVN2", false)] // Bitcoin / Bitcoin Cash legacy
    [TestCase("3J98t1RHT73CNmQwertyyWrnqRhWNLy", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e", false)]// Bitcoin
    [TestCase("0x1F4BA909F005a761e045E6385E671C699a904414", true)] // Ethereum / Avalaunche / Polygon
    [TestCase("XuU3CEUUANJYDPJ8ayCKDQo51TJ4Xp3Z3x", false)] // Dash
    [TestCase("888tNkZrPN6JsEgekjMnABU4TBzc2Dt29EPAvkRxbANsAnjyPbb3iQ1YBRk1UXcdRsiKc9dhwMVgN5S9cQUiyoogDavup3H", false)] // Monero
    [TestCase("32uLhn19ZasD5bsVhLdDthhM37JhJHiEE2", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", false)] // Bitcoin Cash
    [TestCase("MGxNPPB7eBoWPUaprtX9v9CXJZoD2465zN", false)] // LiteCoin
    [TestCase("bnb1fnd0k5l4p3ck2j9x9dp36chk059w977pszdgdz", false)] // BNB BEP20
    [TestCase("X-avax1tzdcgj4ehsvhhgpl7zylwpw0gl2rxcg4r5afk5", false)] // Avalaunche
    [TestCase("HN7cABqLq46Es1jh92dQQisAq662SmxELLLsHHe4YWrH", false)] // Solana
    [TestCase("253dfc2d-ac8b-413b-9a8b-095a1a7b621b:290e7851-7f6f-4827-a980-ce465dc38676", false)] // Exchange Wallet
    [TestCase("test.near", false)] // NEAR Wallet
    [TestCase("test.testnet", false)] // NEAR Wallet
    [TestCase("123456789012345678901234567890123456789012345678901234567890abcd", false)] // NEAR Wallet
    public void IsPolygonAddressTest(string test, bool expected)
    {
        var result = WalletFormat.IsPolygonAddress(test);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", false)]
    [TestCase("Some text", false)]
    [TestCase("1RAHUEYstWetqabcFn5Au4m4GFg7xJaNVN2", false)] // Bitcoin / Bitcoin Cash legacy
    [TestCase("3J98t1RHT73CNmQwertyyWrnqRhWNLy", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e", false)]// Bitcoin
    [TestCase("0x1F4BA909F005a761e045E6385E671C699a904414", true)] // Ethereum / Avalaunche / Polygon
    [TestCase("XuU3CEUUANJYDPJ8ayCKDQo51TJ4Xp3Z3x", false)] // Dash
    [TestCase("888tNkZrPN6JsEgekjMnABU4TBzc2Dt29EPAvkRxbANsAnjyPbb3iQ1YBRk1UXcdRsiKc9dhwMVgN5S9cQUiyoogDavup3H", false)] // Monero
    [TestCase("32uLhn19ZasD5bsVhLdDthhM37JhJHiEE2", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", false)] // Bitcoin Cash
    [TestCase("MGxNPPB7eBoWPUaprtX9v9CXJZoD2465zN", false)] // LiteCoin
    [TestCase("bnb1fnd0k5l4p3ck2j9x9dp36chk059w977pszdgdz", false)] // BNB BEP20
    [TestCase("X-avax1tzdcgj4ehsvhhgpl7zylwpw0gl2rxcg4r5afk5", false)] // Avalaunche
    [TestCase("HN7cABqLq46Es1jh92dQQisAq662SmxELLLsHHe4YWrH", false)] // Solana
    [TestCase("253dfc2d-ac8b-413b-9a8b-095a1a7b621b:290e7851-7f6f-4827-a980-ce465dc38676", false)] // Exchange Wallet
    [TestCase("test.near", false)] // NEAR Wallet
    [TestCase("test.testnet", false)] // NEAR Wallet
    [TestCase("123456789012345678901234567890123456789012345678901234567890abcd", false)] // NEAR Wallet
    public void IsFantomAddressTest(string test, bool expected)
    {
        var result = WalletFormat.IsFantomAddress(test);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", false)]
    [TestCase("Some text", false)]
    [TestCase("1RAHUEYstWetqabcFn5Au4m4GFg7xJaNVN2", false)] // Bitcoin / Bitcoin Cash legacy
    [TestCase("3J98t1RHT73CNmQwertyyWrnqRhWNLy", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e", false)]// Bitcoin
    [TestCase("0x1F4BA909F005a761e045E6385E671C699a904414", true)] // Ethereum / Avalaunche / Polygon
    [TestCase("XuU3CEUUANJYDPJ8ayCKDQo51TJ4Xp3Z3x", false)] // Dash
    [TestCase("888tNkZrPN6JsEgekjMnABU4TBzc2Dt29EPAvkRxbANsAnjyPbb3iQ1YBRk1UXcdRsiKc9dhwMVgN5S9cQUiyoogDavup3H", false)] // Monero
    [TestCase("32uLhn19ZasD5bsVhLdDthhM37JhJHiEE2", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", false)] // Bitcoin Cash
    [TestCase("MGxNPPB7eBoWPUaprtX9v9CXJZoD2465zN", false)] // LiteCoin
    [TestCase("bnb1fnd0k5l4p3ck2j9x9dp36chk059w977pszdgdz", false)] // BNB BEP20
    [TestCase("X-avax1tzdcgj4ehsvhhgpl7zylwpw0gl2rxcg4r5afk5", false)] // Avalaunche
    [TestCase("HN7cABqLq46Es1jh92dQQisAq662SmxELLLsHHe4YWrH", false)] // Solana
    [TestCase("253dfc2d-ac8b-413b-9a8b-095a1a7b621b:290e7851-7f6f-4827-a980-ce465dc38676", false)] // Exchange Wallet
    [TestCase("test.near", false)] // NEAR Wallet
    [TestCase("test.testnet", false)] // NEAR Wallet
    [TestCase("123456789012345678901234567890123456789012345678901234567890abcd", false)] // NEAR Wallet
    [TestCase("5GrpknVvGGrGH3EFuURXeMrWHvbpj3VfER1oX5jFtuGbfzCE", false)] // Polkadot / Aleph Zero
    public void IsMetisAddressTest(string test, bool expected)
    {
        var result = WalletFormat.IsMetisAddress(test);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", false)]
    [TestCase("Some text", false)]
    [TestCase("1RAHUEYstWetqabcFn5Au4m4GFg7xJaNVN2", false)] // Bitcoin / Bitcoin Cash legacy
    [TestCase("3J98t1RHT73CNmQwertyyWrnqRhWNLy", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e", false)]// Bitcoin
    [TestCase("0x1F4BA909F005a761e045E6385E671C699a904414", false)] // Ethereum / Avalaunche / Polygon
    [TestCase("XuU3CEUUANJYDPJ8ayCKDQo51TJ4Xp3Z3x", false)] // Dash
    [TestCase("888tNkZrPN6JsEgekjMnABU4TBzc2Dt29EPAvkRxbANsAnjyPbb3iQ1YBRk1UXcdRsiKc9dhwMVgN5S9cQUiyoogDavup3H", false)] // Monero
    [TestCase("32uLhn19ZasD5bsVhLdDthhM37JhJHiEE2", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", false)] // Bitcoin Cash
    [TestCase("MGxNPPB7eBoWPUaprtX9v9CXJZoD2465zN", false)] // LiteCoin
    [TestCase("bnb1fnd0k5l4p3ck2j9x9dp36chk059w977pszdgdz", false)] // BNB BEP20
    [TestCase("X-avax1tzdcgj4ehsvhhgpl7zylwpw0gl2rxcg4r5afk5", false)] // Avalaunche
    [TestCase("HN7cABqLq46Es1jh92dQQisAq662SmxELLLsHHe4YWrH", false)] // Solana
    [TestCase("253dfc2d-ac8b-413b-9a8b-095a1a7b621b:290e7851-7f6f-4827-a980-ce465dc38676", false)] // Exchange Wallet
    [TestCase("test.near", false)] // NEAR Wallet
    [TestCase("test.testnet", false)] // NEAR Wallet
    [TestCase("123456789012345678901234567890123456789012345678901234567890abcd", false)] // NEAR Wallet
    [TestCase("5E28FKBPwmecVyTQ299AtotqnJExrjVQYueGDkxH8GV9DTPW", true)] // Polkadot / Aleph Zero
    [TestCase("5FskcgnxgwMtUxjpEtBEPhajkVK81mhNCHDB6H6P1brCCXXM", true)] // Polkadot / Aleph Zero
    public void IsPolkadotAddressTest(string test, bool expected)
    {
        var result = WalletFormat.IsPolkadotAddress(test);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", false)]
    [TestCase("Some text", false)]
    [TestCase("1RAHUEYstWetqabcFn5Au4m4GFg7xJaNVN2", false)] // Bitcoin / Bitcoin Cash legacy
    [TestCase("3J98t1RHT73CNmQwertyyWrnqRhWNLy", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e", false)]// Bitcoin
    [TestCase("0x1F4BA909F005a761e045E6385E671C699a904414", false)] // Ethereum / Avalaunche / Polygon
    [TestCase("XuU3CEUUANJYDPJ8ayCKDQo51TJ4Xp3Z3x", false)] // Dash
    [TestCase("888tNkZrPN6JsEgekjMnABU4TBzc2Dt29EPAvkRxbANsAnjyPbb3iQ1YBRk1UXcdRsiKc9dhwMVgN5S9cQUiyoogDavup3H", false)] // Monero
    [TestCase("32uLhn19ZasD5bsVhLdDthhM37JhJHiEE2", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", false)] // Bitcoin Cash
    [TestCase("MGxNPPB7eBoWPUaprtX9v9CXJZoD2465zN", false)] // LiteCoin
    [TestCase("bnb1fnd0k5l4p3ck2j9x9dp36chk059w977pszdgdz", false)] // BNB BEP20
    [TestCase("X-avax1tzdcgj4ehsvhhgpl7zylwpw0gl2rxcg4r5afk5", false)] // Avalaunche
    [TestCase("HN7cABqLq46Es1jh92dQQisAq662SmxELLLsHHe4YWrH", false)] // Solana
    [TestCase("253dfc2d-ac8b-413b-9a8b-095a1a7b621b:290e7851-7f6f-4827-a980-ce465dc38676", false)] // Exchange Wallet
    [TestCase("test.near", false)] // NEAR Wallet
    [TestCase("test.testnet", false)] // NEAR Wallet
    [TestCase("123456789012345678901234567890123456789012345678901234567890abcd", false)] // NEAR Wallet
    [TestCase("5E28FKBPwmecVyTQ299AtotqnJExrjVQYueGDkxH8GV9DTPW", true)] // Polkadot / Aleph Zero
    [TestCase("5FskcgnxgwMtUxjpEtBEPhajkVK81mhNCHDB6H6P1brCCXXM", true)] // Polkadot / Aleph Zero
    public void IsAlephZeroAddressTest(string test, bool expected)
    {
        var result = WalletFormat.IsAlephZeroAddress(test);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("", false)]
    [TestCase("Some text", false)]
    [TestCase("1RAHUEYstWetqabcFn5Au4m4GFg7xJaNVN2", false)] // Bitcoin / Bitcoin Cash legacy
    [TestCase("3J98t1RHT73CNmQwertyyWrnqRhWNLy", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e", false)]// Bitcoin
    [TestCase("0x1F4BA909F005a761e045E6385E671C699a904414", false)] // Ethereum / Avalaunche / Polygon
    [TestCase("XuU3CEUUANJYDPJ8ayCKDQo51TJ4Xp3Z3x", false)] // Dash
    [TestCase("888tNkZrPN6JsEgekjMnABU4TBzc2Dt29EPAvkRxbANsAnjyPbb3iQ1YBRk1UXcdRsiKc9dhwMVgN5S9cQUiyoogDavup3H", false)] // Monero
    [TestCase("32uLhn19ZasD5bsVhLdDthhM37JhJHiEE2", false)] // Bitcoin / Bitcoin Cash legacy / Litecoin
    [TestCase("pqx5ej6z9cvxc2c7nw5p4s5kf8nzmzc5cqapu8xprq", false)] // Bitcoin Cash
    [TestCase("MGxNPPB7eBoWPUaprtX9v9CXJZoD2465zN", false)] // LiteCoin
    [TestCase("bnb1fnd0k5l4p3ck2j9x9dp36chk059w977pszdgdz", false)] // BNB BEP20
    [TestCase("X-avax1tzdcgj4ehsvhhgpl7zylwpw0gl2rxcg4r5afk5", false)] // Avalaunche
    [TestCase("HN7cABqLq46Es1jh92dQQisAq662SmxELLLsHHe4YWrH", false)] // Solana
    [TestCase("253dfc2d-ac8b-413b-9a8b-095a1a7b621b:290e7851-7f6f-4827-a980-ce465dc38676", false)] // Exchange Wallet
    [TestCase("test.near", false)] // NEAR Wallet
    [TestCase("test.testnet", false)] // NEAR Wallet
    [TestCase("123456789012345678901234567890123456789012345678901234567890abcd", false)] // NEAR Wallet
    [TestCase("5E28FKBPwmecVyTQ299AtotqnJExrjVQYueGDkxH8GV9DTPW", false)] // Polkadot / Aleph Zero
    [TestCase("addr1x80de0mz3m9xmgtlmqqzu06s0uvfsczskdec8k7v4jhr7077mjlk9rk2dkshlkqq9cl4qlccnps9pvmns0duet9w8ulsylzv28", true)] // Cardano
    public void IsCardanoAddressTest(string test, bool expected)
    {
        var result = WalletFormat.IsCardanoAddress(test);

        Assert.That(result, Is.EqualTo(expected));
    }
}