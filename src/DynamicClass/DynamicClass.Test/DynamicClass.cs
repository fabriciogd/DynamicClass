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

            Type @class = ClassFactory.Instance.Create(properties);

            Assert.AreEqual(2, @class.GetProperties().Count());
        }

        [TestMethod]
        public void Dynamic_properties_hashcode()
        {
            var properties = new List<DynamicProperty>()
            {
                new DynamicProperty("Id", typeof(System.Int32)),
                new DynamicProperty("Nome", typeof(System.String))
            };

            Type @class = ClassFactory.Instance.Create(properties);

            Assert.AreNotEqual(0, @class.GetHashCode());
        }
    }
}
