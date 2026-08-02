namespace Dsw2026Tpi.Domain.Entities;

public class Paciente : EntityBase
{
    public long Dni { get; init; }
    public string Email { get; private set; }
    public string? Nombre { get; private set; }
    public string? Telefono { get; private set; }

    #region Constructor for EF
        #pragma warning disable CS8618
    private Paciente() { }
        #pragma warning restore CS8618
    #endregion

    public Paciente(long dni, string email, string? nombre = null, string? telefono = null, Guid? id = null) : base(id)
    {
        Dni = dni;
        Email = email;
        Nombre = nombre;
        Telefono = telefono;
    }
}
