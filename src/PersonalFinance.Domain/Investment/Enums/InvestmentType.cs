namespace PersonalFinance.Domain.Investment.Enums;

/// <summary>Design spec mentions CDTs (Certificado de Depósito a Término) specifically.</summary>
public enum InvestmentType
{
    CDT,            // Certificado de Depósito a Término (Colombia)
    FixedDeposit,
    Stock,
    ETF,
    MutualFund,
    CryptoCurrency,
    Bond,
    RealEstate,
    Other
}
