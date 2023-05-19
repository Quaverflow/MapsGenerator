﻿namespace MapsGenerator.POC;

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public int Height { get; set; }
    public Address Address { get; set; }
    public Traits Traits { get; set; }
}