namespace DynamicClass.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class DynamicClass
    {
        [TestMethod]
        public void Dynamic_properties()
        {
            var properties = new List<DynamicProperty>()
            {
                new DynamicProperty("Id", typeof(System.Int32)),
                new DynamicProperty("Nome", typeof(System.String))
            };

            Type instance = ClassFactory.Instance.Create(properties);

            Assert.AreEqual(2, instance.GetProperties().Count());
        }

        [TestMethod]
        public void Dynamic_properties_hashcode()
        {
            var properties = new List<DynamicProperty>()
            {
                new DynamicProperty("Id", typeof(System.Int32)),
                new DynamicProperty("Nome", typeof(System.String))
            };

            var t = ClassFactory.Instance.Create(properties);

            var instance = (dynamic)Activator.CreateInstance(t);

            Assert.AreNotEqual(0, instance.GetHashCode());
        }
    }
}
