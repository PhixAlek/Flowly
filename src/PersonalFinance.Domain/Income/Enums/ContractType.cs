namespace PersonalFinance.Domain.Income.Enums;

/// <summary>Affects deduction rules (TaxInfo) and classification for reports.</summary>
public enum ContractType
{
    Indefinite,        // Contrato indefinido
    FixedTerm,         // Contrato a término fijo
    ServiceProvision,  // Prestación de servicios
    Freelance,
    Informal,
    None
}
