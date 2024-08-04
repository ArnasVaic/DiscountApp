
using DiscountApp.Driver.Core;
using DiscountApp.Driver.Enums;
using DiscountApp.Driver.Models;
using DiscountApp.Driver.Repositories;
using DiscountApp.Driver.Rules;
using DiscountApp.Driver.Services;
using Moq;

namespace DiscountApp.UnitTests;

public class DiscountApplicationServiceTests
{
    private readonly Mock<IDiscountApplicationRule<LargePackageDiscountApplicationContext>> _largePackageRuleMock = new();

    [Fact]
    public void Apply_ThreeLargeLaPosteTransactions_ShipmentCoveredForLastTransaction()
    {
        // ARRANGE

        IEnumerable<Result<TransactionInputModel>> inputTransactions = [
            new TransactionInputModel(DateOnly.Parse("2024-01-01"), PackageSize.Large, Carrier.LaPoste),
            new TransactionInputModel(DateOnly.Parse("2024-01-02"), PackageSize.Large, Carrier.LaPoste),
            new TransactionInputModel(DateOnly.Parse("2024-01-03"), PackageSize.Large, Carrier.LaPoste),
        ];

        _largePackageRuleMock
            .Setup(x => x.ApplyRule(It.Is<LargePackageDiscountApplicationContext>(tr => tr.Date.Day == 3)))
            .Returns(new DiscountedTransactionModel(DateOnly.Parse("2024-01-03"), PackageSize.Large, Carrier.LaPoste, 0, 6.90m));

        _largePackageRuleMock
            .Setup(x => x.ApplyRule(It.Is<LargePackageDiscountApplicationContext>(tr => tr.Date.Day != 3)))
            .Returns(new DiscountedTransactionModel(DateOnly.Parse("2024-01-02"), PackageSize.Large, Carrier.LaPoste, 6.90m, 0));

        var service = CreateService();
        // ACT

        var results = service.Apply(inputTransactions);

        // ASSERT
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.True(r.IsSuccess));
        Assert.Equal(0, results.First().Value!.Discount);
        Assert.Equal(0, results.Skip(1).First().Value!.Discount);
        Assert.Equal(6.90m, results.Last().Value!.Discount);
    }

    private DiscountApplicationService CreateService()
    {
        var transactionPriceRepository = new HardcodedTransactionPriceRepository();
        return new(
            new SmallPackageRule(transactionPriceRepository),
            new MediumPackageRule(transactionPriceRepository),
            _largePackageRuleMock.Object,
            new DiscountRuleApplicationContextBuilder()
        );
    }
}