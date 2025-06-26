namespace RealEstate.Properties.Domain.Entities;

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string? StreetNumber { get; set; }
    public string? Unit { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? Province { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? CountryCode { get; set; }
    public string? Neighborhood { get; set; }
    public string? District { get; set; }
    public string? Region { get; set; }

    public string GetFullAddress()
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(StreetNumber))
            parts.Add(StreetNumber);

        if (!string.IsNullOrEmpty(Street))
            parts.Add(Street);

        if (!string.IsNullOrEmpty(Unit))
            parts.Add($"Unit {Unit}");

        if (!string.IsNullOrEmpty(City))
            parts.Add(City);

        if (!string.IsNullOrEmpty(State))
            parts.Add(State);
        else if (!string.IsNullOrEmpty(Province))
            parts.Add(Province);

        if (!string.IsNullOrEmpty(PostalCode))
            parts.Add(PostalCode);

        if (!string.IsNullOrEmpty(Country))
            parts.Add(Country);

        return string.Join(", ", parts);
    }

    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(Street) &&
               string.IsNullOrEmpty(City) &&
               string.IsNullOrEmpty(Country);
    }
}