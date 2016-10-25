using DynamicClass;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Run
{
    class Program
    {
        static void Main(string[] args)
        {
            // List of properties wich I want to create in class
            var properties = new List<DynamicProperty>
            {
                new DynamicProperty("Id", typeof(System.Int32)),
                new DynamicProperty("Name", typeof(System.String))
            };

            // Create the dynamic class
            Type @class = ClassFactory.Instance.Create(properties);

            Console.WriteLine($"Nº of properties: {@class.GetProperties().Count()}");

            Console.WriteLine($"GetHashCode: {@class.GetHashCode()}");

            Console.ReadKey();
        }
    }
}
