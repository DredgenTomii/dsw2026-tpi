namespace Dsw2026Tpi.Domain.Entities;

public class Doctor : EntityBase
{

    public string Name { get; private set; }
    public string LicenseNumber { get; private set; }
    public bool Deleted { get; private set; }
    public Guid? SpecialityId { get; set; }
    public Speciality? Speciality { get; private set; }

    private Doctor()
    {
    }

    public Doctor(string name, string licenseNumber, Speciality speciality, Guid? id = null) : base(id)
    {
        Name = name;
        LicenseNumber = licenseNumber;
        Speciality = speciality;
        SpecialityId = speciality?.Id;
        Deleted = false;
    }

    public void Update(string name, string licenseNumber, Speciality speciality)
    {
        Name = name;
        LicenseNumber = licenseNumber;
        Speciality = speciality;
        SpecialityId = speciality.Id;
    }

    public void Delete()
    {
        Deleted = true;
    }
}