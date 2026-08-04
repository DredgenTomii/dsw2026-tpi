namespace Dsw2026Tpi.Domain.Entities;

public class Patient : EntityBase
{
    public long Dni { get; init; }
    public string Email { get; private set; }
    public string? Name { get; private set; }
    public string? Phone { get; private set; }

    #region Constructor for EF
        #pragma warning disable CS8618
    private Patient() { }
        #pragma warning restore CS8618
    #endregion

    public Patient(long dni, string email, string? name = null, string? telefono = null, Guid? id = null) : base(id)
    {
        Dni = dni;
        Email = email;
        Name = name;
        Phone = telefono;
    }
}
