using FluentValidation;
using GestAI.Application.Reports;
using Xunit;

namespace GestAI.Tests;

public class ReportsQueryValidationTests
{
    private readonly GetReportsQueryValidator _validator = new();

    [Fact]
    public void GetReportsQuery_Should_Fail_When_Date_Range_Is_Not_Exclusive()
    {
        var result = _validator.Validate(new GetReportsQuery(1, new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 10)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetReportsQuery.ToExclusive));
    }

    [Fact]
    public void GetReportsQuery_Should_Pass_When_Date_Range_Is_Valid()
    {
        var result = _validator.Validate(new GetReportsQuery(1, new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 11)));

        Assert.True(result.IsValid);
    }
}
