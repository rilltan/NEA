using System;

struct OrbitalElements
{
    public float SemiMajorAxis, Eccentricity, Inclination, AscendingNodeLongitude, PeriapsisArgument;
    public OrbitalElements(float semiMajorAxis, float eccentricity, float inclination, float ascendingNodeLongitude, float periapsisArgument)
    {
        SemiMajorAxis = semiMajorAxis;
        Eccentricity = eccentricity;
        Inclination = inclination;
        AscendingNodeLongitude = ascendingNodeLongitude;
        PeriapsisArgument = periapsisArgument;
    }
}